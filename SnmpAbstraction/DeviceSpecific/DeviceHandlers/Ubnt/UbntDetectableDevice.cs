using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Detectable device implementation for Ubiquiti devices
    /// </summary>
    internal class UbntDetectableDevice : DetectableDeviceBase
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// String in system description to possibly detect Ubiquiti devices
        /// </summary>
        private const string PossiblyUbiquitiDetectionDescriptionString = "Linux";

        /// <summary>
        /// The OID to obtain the string including the OS version.<br/>
        /// Example: &quot;Ubiquiti Networks, Inc.&quot;
        /// </summary>
        private readonly Oid UbntManufacturerDetectionOid = new Oid(".1.2.840.10036.3.1.2.1.2");

        /// <summary>
        /// String in <see cref="UbntManufacturerDetectionOid" /> to detect Ubiquiti devices
        /// </summary>
        private const string UbiquitiManufactorerDetectionString = "Ubiquiti";

        /// <summary>
        /// Device model string for AirFiber.
        /// </summary>
        private const string AirFiberFakeModelString = "AirFiber";

        /// <summary>
        /// An OID to obtain the string including the OS version.<br/>
        /// Example: &quot;XM.ar7240.v6.2.0.33033.190703.1147&quot;
        /// </summary>
        private readonly Oid OsVersionRootOid = new Oid(".1.2.840.10036.3.1.2.1.4");

        /// <summary>
        /// Regex to extract version.<br/>
        /// Example: From &quot;XM.ar7240.v6.2.0.33033.190703.1147&quot; it will extract &quot;6.2.0&quot;.
        /// </summary>
        private static readonly Regex OsVersionExtractionRegex = new Regex(".*v([0-9.]{1,})");

        /// <summary>
        /// An OID to obtain the string representing the device model (used for database lookup).<br/>
        /// Example: &quot;AirGrid M5&quot;
        /// </summary>
        private readonly Oid ModelRootOid = new Oid(".1.2.840.10036.3.1.2.1.3");

        /// <summary>
        /// The ID (to be appended to the root OIDs) for which the device has been detected as UBNT device.
        /// </summary>
        private uint? detectionId = null;

        /// <summary>
        /// Walk root for detecting AirFiber -> should actually return the OS version if it's an AirFiber device
        /// Rroot OID of AirFiber is: .1.3.6.1.4.1.41112.1.3 according to UBNT-MIB-airfiber.txt.
        /// </summary>
        Oid AirFiberDetectionWalkRootOid = new Oid(".1.3.6.1.4.1.41112.1.3.2.1.40");

        /// <summary>
        /// Field to store an already detect OS version (during IsApplicable) for later use by CreateHandler.
        /// </summary>
        private string osDetectedVersion = null;

        /// <summary>
        /// Field to store an already detect model (during IsApplicable) for later use by CreateHandler.
        /// </summary>
        private string detectedModel = null;

        /// <inheritdoc />
        public override QueryApis SupportedApi { get; } = QueryApis.Snmp;

        /// <inheritdoc />
        public override bool IsApplicableVendorSpecific(IpAddress address, IQuerierOptions options)
        {
            // we only support SNMP
            return false;
        }

        /// <inheritdoc />
        public override bool IsApplicableSnmp(ISnmpLowerLayer snmpLowerLayer, IQuerierOptions options)
        {
            var description = snmpLowerLayer?.SystemData?.Description;
            if (string.IsNullOrWhiteSpace(description))
            {
                log.Warn($"Description in system data of device '{snmpLowerLayer.Address}' is null, empty or white-space-only: Assuming the device is not a Ubiquiti device");
                return false;
            }

            if (!description.Contains(PossiblyUbiquitiDetectionDescriptionString))
            {
                log.Info($"Description in system data of device '{snmpLowerLayer.Address}' doesn't contain string '{PossiblyUbiquitiDetectionDescriptionString}': Assuming the device is not a Ubiquiti device");
                return false;
            }

            var ubntManufacturer = snmpLowerLayer?.DoWalk(UbntManufacturerDetectionOid);
            if ((ubntManufacturer == null) || (ubntManufacturer.Count == 0))
            {
                log.Warn($"UBNT Manufacturer ID string of device '{snmpLowerLayer.Address}' is null or empty: The device could still be AirFiber");
                return this.DetectAirFiber(snmpLowerLayer);
            }

            var manufacturer = ubntManufacturer.FirstOrDefault(m => m.Value.ToString().Contains(UbiquitiManufactorerDetectionString));
            if (manufacturer == null)
            {
                log.Info($"UBNT Manufacturer ID string of device '{snmpLowerLayer.Address}' doesn't contain string '{UbiquitiManufactorerDetectionString}': Assuming the device is not a Ubiquiti device");
                return false;
            }

            this.detectionId = manufacturer.Oid[manufacturer.Oid.Length - 1];

            log.Info($"Device '{snmpLowerLayer.Address}' seems to be a Ubiquiti device (detection ID {this.detectionId})");
            
            return true;
        }

        /// <inheritdoc />
        public override IDeviceHandler CreateHandler(ISnmpLowerLayer lowerLayer, IQuerierOptions options)
        {
            if (!this.detectionId.HasValue)
            {
                throw new InvalidOperationException("Cannot perform CreateHandler() without previous and successful call to IsApplicable");
            }

            try
            {
                List<Oid> queryList = new List<Oid>();

                Oid osVersionOid = null;
                if (string.IsNullOrWhiteSpace(this.osDetectedVersion))
                {
                    osVersionOid = OsVersionRootOid + new Oid(new uint[] { this.detectionId.Value });
                    queryList.Add(osVersionOid);
                }

                Oid modelOid = null;
                if (string.IsNullOrWhiteSpace(this.detectedModel))
                {
                    modelOid = ModelRootOid + new Oid(new uint[] { this.detectionId.Value });
                    queryList.Add(modelOid);
                }

                VbCollection osVersionCollection = null;
                if (queryList.Count > 0)
                {
                    osVersionCollection = lowerLayer.Query(queryList);
                }

                string osVersionString = (osVersionOid != null)
                    ? osVersionCollection[osVersionOid].Value.ToString()
                    : this.osDetectedVersion;

                Match match = OsVersionExtractionRegex.Match(osVersionString);
                SemanticVersion osVersion = match.Success ? match.Groups[1].Value.ToSemanticVersion() : null;

                string model = (modelOid != null)
                    ? osVersionCollection[modelOid].Value.ToString()
                    : this.detectedModel;

                if (string.IsNullOrWhiteSpace(model))
                {
                    var info = $"Model (retrieved using OID '{modelOid}') is null, empty or white-space-only";
                    log.Warn(info);
                    throw new HamnetSnmpException(info, lowerLayer?.Address?.ToString());
                }

                log.Info($"Detected device '{lowerLayer.Address}' as Ubiquiti '{model}' v '{osVersion}'");

                DeviceVersion deviceVersion;
                IDeviceSpecificOidLookup oidTable = this.ObtainOidTable(model.Trim(), osVersion, out deviceVersion, lowerLayer?.Address);
                if (string.IsNullOrWhiteSpace(deviceVersion.HandlerClassName))
                {
                    return (model == AirFiberFakeModelString)
                        ? new UbiquitiAirFiberDeviceHandler(lowerLayer, oidTable, osVersion, model, options) as IDeviceHandler
                        : new UbiquitiAirOsAbove56DeviceHandler(lowerLayer, oidTable, osVersion, model, options) as IDeviceHandler;
                }
                else
                {
                    return this.GetHandlerViaReflection(deviceVersion.HandlerClassName, lowerLayer, oidTable, osVersion, model, options);
                }

            }
            catch(Exception ex)
            {
                // we want to catch and nest the exception here as the APIs involved are not able to append the infomration for which
                // device (i.e. IP address) the exception is for
                throw new HamnetSnmpException($"Failed to create handler for Ubiquiti device '{lowerLayer.Address}': {ex.Message}", ex, lowerLayer?.Address?.ToString());
            }
        }

        /// <summary>
        /// Detects an AirFiber device which does not really provide any manufacturer ID or similar
        /// </summary>
        /// <param name="snmpLowerLayer">The lower communication layer to user.</param>
        /// <returns><c>true</c> if an AirFiber device has been detected and the class fields have been set accordingly.</returns>
        private bool DetectAirFiber(ISnmpLowerLayer snmpLowerLayer)
        {
            var mibInfo41112 = snmpLowerLayer.DoWalk(AirFiberDetectionWalkRootOid);
            if (mibInfo41112.Count == 0)
            {
                // it's also not an AirFiber device
                log.Info($"Reading of OS version of device '{snmpLowerLayer.Address}' via AirFiber OID {AirFiberDetectionWalkRootOid} didn't reveal anything: Assuming the device is not an Ubiquiti AirFiber device");
                return false;
            }

            // never seen AirFiber returning more than one
            var firstValue = mibInfo41112.First();
            this.osDetectedVersion = firstValue.Value.ToString();
            this.detectionId = firstValue.Oid.Last();
            this.detectedModel = AirFiberFakeModelString;

            return true;
        }
    }
}
