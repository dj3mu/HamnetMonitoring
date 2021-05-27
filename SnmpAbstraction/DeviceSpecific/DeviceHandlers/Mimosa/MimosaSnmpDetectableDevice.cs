using System;
using System.Text.RegularExpressions;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Detectable device implementation for Mimosa devices
    /// </summary>
    internal class MimosaSnmpDetectableDevice : DetectableDeviceBase
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// String in system description to detect RouterOS
        /// </summary>
        private const string MimosaDetectionString = "Mimosa Firmware";

        /// <summary>
        /// The OID to obtain the string including the OS version.<br/>
        /// Example: "RouterOS 6.45.3 (stable) on RB711-5Hn-MMCX"
        /// </summary>
        private readonly Oid OsVersionOid = new Oid(".1.3.6.1.4.1.43356.2.1.2.1.3.0");
        private readonly Oid ModelOid = new Oid(".1.3.6.1.4.1.43356.2.1.2.1.1.0");

        /// <inheritdoc />
        public override QueryApis SupportedApi { get; } = QueryApis.Snmp;

        /// <inheritdoc />
        public override bool IsApplicableVendorSpecific(IpAddress address, IQuerierOptions options)
        {
            // at them moment vendor-specific is not yet implemented
            return false;
        }

        /// <inheritdoc />
        public override bool IsApplicableSnmp(ISnmpLowerLayer snmpLowerLayer, IQuerierOptions options)
        {
            var description = snmpLowerLayer?.SystemData?.Description;
            if (string.IsNullOrWhiteSpace(description))
            {
                var info = $"Description in system data of device '{snmpLowerLayer.Address}' is null, empty or white-space-only: Assuming the device is not a Mimosa device";
                log.Warn(info);
                this.CollectException("Mimosa Snmp: No device description", new HamnetSnmpException(info));
                return false;
            }

            if (!description.Contains(MimosaDetectionString))
            {
                var info = $"Description in system data of device '{snmpLowerLayer.Address}' doesn't contain string '{MimosaDetectionString}': Assuming the device is not a Mimosa device";
                log.Info(info);
                this.CollectException("Mimosa Snmp: No Router-OS-like string in device description", new HamnetSnmpException(info));
                return false;
            }

            log.Info($"Device '{snmpLowerLayer.Address}' seems to be a Mimosa device");

            return true;
        }

        /// <inheritdoc />
        public override IDeviceHandler CreateHandler(ISnmpLowerLayer lowerLayer, IQuerierOptions options)
        {
            string osVersionString = null;

            // try #1: In IEEE SNMP tree
            try
            {
                osVersionString = lowerLayer.QueryAsString(OsVersionOid, "Mimosa Firmware Version String #1");
            }
            catch(SnmpException ex)
            {
                this.CollectException("Mimosa Snmp: Getting version string", ex);
                osVersionString = null;
            }
            catch(HamnetSnmpException ex)
            {
                this.CollectException("Mimosa Snmp: Getting version string", ex);
                osVersionString = null;
            }

            SemanticVersion osVersion = osVersionString.ToSemanticVersion();

            var model = lowerLayer?.SystemData?.Description?.Trim();

            log.Info($"Detected device '{lowerLayer.Address}' as {model} v '{osVersion}'");

            DeviceVersion deviceVersion;
            IDeviceSpecificOidLookup oidTable = this.ObtainOidTable(model.Trim(), osVersion, out deviceVersion, lowerLayer.Address);
            if (string.IsNullOrWhiteSpace(deviceVersion.HandlerClassName))
            {
                try
                {
                    return new MimosaSnmpDeviceHandler(lowerLayer, oidTable, osVersion, model, options);
                }
                catch(Exception ex)
                {
                    this.CollectException("MimosaSnmp: OID table lookup", ex);

                    // we want to catch and nest the exception here as the APIs involved are not able to append the infomration for which
                    // device (i.e. IP address) the exception is for
                    throw new HamnetSnmpException($"Failed to create Mimosa handler for device '{lowerLayer.Address}': {ex.Message}", ex, lowerLayer.Address?.ToString());
                }
            }
            else
            {
                return this.GetHandlerViaReflection(deviceVersion.HandlerClassName, lowerLayer, oidTable, osVersion, model, options);
            }
        }
    }
}
