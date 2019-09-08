using System;
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
        private readonly Oid UbntManufacturerDetectionOid = new Oid(".1.2.840.10036.3.1.2.1.2.5");

        /// <summary>
        /// String in <see cref="UbntManufacturerDetectionOid" /> to detect Ubiquiti devices
        /// </summary>
        private const string UbiquitiManufactorerDetectionString = "Ubiquiti";

        /// <summary>
        /// An OID to obtain the string including the OS version.<br/>
        /// Example: &quot;XM.ar7240.v6.2.0.33033.190703.1147&quot;
        /// </summary>
        private readonly Oid OsVersionOid = new Oid(".1.2.840.10036.3.1.2.1.4.5");

        /// <summary>
        /// Regex to extract version.<br/>
        /// Example: From &quot;XM.ar7240.v6.2.0.33033.190703.1147&quot; it will extract &quot;6.2.0&quot;.
        /// </summary>
        private static readonly Regex OsVersionExtractionRegex = new Regex(".*v([0-9.]{1,5})");

        /// <summary>
        /// An OID to obtain the string representing the device model (used for database lookup).<br/>
        /// Example: &quot;AirGrid M5&quot;
        /// </summary>
        private readonly Oid ModelOid = new Oid(".1.2.840.10036.3.1.2.1.3.5");

        /// <inheritdoc />
        public override bool IsApplicable(ISnmpLowerLayer snmpLowerLayer)
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

            var ubntManufactorer = snmpLowerLayer?.QueryAsString(UbntManufacturerDetectionOid, "UBNT manufactorer detection string");
            if (string.IsNullOrWhiteSpace(ubntManufactorer))
            {
                log.Warn($"UBNT Manufactorer ID string of device '{snmpLowerLayer.Address}' is null, empty or white-space-only: Assuming the device is not a Ubiquiti device");
                return false;
            }

            if (!ubntManufactorer.Contains(UbiquitiManufactorerDetectionString))
            {
                log.Info($"UBNT Manufactorer ID string of device '{snmpLowerLayer.Address}' doesn't contain string '{UbiquitiManufactorerDetectionString}': Assuming the device is not a Ubiquiti device");
                return false;
            }

            log.Info($"Device '{snmpLowerLayer.Address}' seems to be a Ubiquiti device");
            
            return true;
        }

        /// <inheritdoc />
        public override IDeviceHandler CreateHandler(ISnmpLowerLayer lowerLayer)
        {
            try
            {
                string osVersionString = null;

                // try #1: In IEEE SNMP tree
                osVersionString = lowerLayer.QueryAsString(OsVersionOid, "Ubiquiti Version String #1");

                // Example: "RouterOS 6.45.3 (stable) on RB711-5Hn-MMCX"
                Match match = OsVersionExtractionRegex.Match(osVersionString);

                SemanticVersion osVersion = match.Success ? SemanticVersion.Parse(match.Groups[1].Value) : null;

                string model = lowerLayer.QueryAsString(ModelOid, "Ubiquiti device model");
                if (string.IsNullOrWhiteSpace(model))
                {
                    var info = $"Model (retrieved using OID '{ModelOid}') is null, empty or white-space-only";
                    log.Warn(info);
                    throw new HamnetSnmpException(info);
                }

                return new UbiquitiDeviceHandler(lowerLayer, this.ObtainOidTable(model.Trim(), osVersion), osVersion);
            }
            catch(Exception ex)
            {
                // we want to catch and nest the exception here as the APIs involved are not able to append the infomration for which
                // device (i.e. IP address) the exception is for
                throw new HamnetSnmpException($"Failed to create handler for Ubiquiti device '{lowerLayer.Address}': {ex.Message}", ex);
            }
        }
    }
}
