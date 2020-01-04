using System;
using System.Text.RegularExpressions;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Detectable device implementation for MikroTik devices
    /// </summary>
    internal class MikrotikSnmpDetectableDevice : DetectableDeviceBase
    {
        private static readonly Regex OsVersionExtractionRegex = new Regex(RouterOsDetectionString + @"\s+([0-9.]+)\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// String in system description to detect RouterOS
        /// </summary>
        private const string RouterOsDetectionString = "RouterOS";

        /// <summary>
        /// The OID to obtain the string including the OS version.<br/>
        /// Example: "RouterOS 6.45.3 (stable) on RB711-5Hn-MMCX"
        /// </summary>
        private readonly Oid OsVersionOid = new Oid(".1.3.6.1.2.1.47.1.1.1.1.2.65536");

        /// <summary>
        /// A second OID to obtain the string including the OS version.<br/>
        /// Example: "RouterOS v6.45.3 Jul/29/2019 12:11:49"
        /// </summary>
        private readonly Oid OsVersionOid2 = new Oid(".1.3.6.1.4.1.14988.1.1.17.1.1.4.1");

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
                var info = $"Description in system data of device '{snmpLowerLayer.Address}' is null, empty or white-space-only: Assuming the device is not a MikroTik device";
                log.Warn(info);
                this.CollectException("MtikSnmp: No device description", new HamnetSnmpException(info));
                return false;
            }

            if (!description.Contains(RouterOsDetectionString))
            {
                var info = $"Description in system data of device '{snmpLowerLayer.Address}' doesn't contain string '{RouterOsDetectionString}': Assuming the device is not a MikroTik device";
                log.Info(info);
                this.CollectException("MtikSnmp: No Router-OS-like string in device description", new HamnetSnmpException(info));
                return false;
            }

            log.Info($"Device '{snmpLowerLayer.Address}' seems to be a MikroTik device");
            
            return true;
        }

        /// <inheritdoc />
        public override IDeviceHandler CreateHandler(ISnmpLowerLayer lowerLayer, IQuerierOptions options)
        {
            string osVersionString = null;

            // try #1: In IEEE SNMP tree
            try
            {
                osVersionString = lowerLayer.QueryAsString(OsVersionOid, "MikroTik RouterOS Version String #1");
            }
            catch(SnmpException ex)
            {
                this.CollectException("MtikSnmp: Getting version string", ex);
                osVersionString = null;
            }
            catch(HamnetSnmpException ex)
            {
                this.CollectException("MtikSnmp: Getting version string", ex);
                osVersionString = null;
            }

            // try #2: Withing MikroTik enterprise tree
            if (string.IsNullOrWhiteSpace(osVersionString))
            {
                osVersionString = lowerLayer.QueryAsString(OsVersionOid2, "MikroTik RouterOS Version String #2");
            }

            // Example: "RouterOS 6.45.3 (stable) on RB711-5Hn-MMCX"
            Match match = OsVersionExtractionRegex.Match(osVersionString);

            SemanticVersion osVersion = match.Success ? match.Groups[1].Value.ToSemanticVersion() : null;

            var model = lowerLayer.SystemData.Description.Replace(RouterOsDetectionString, string.Empty).Trim();

            log.Info($"Detected device '{lowerLayer.Address}' as MikroTik '{model}' v '{osVersion}'");

            DeviceVersion deviceVersion;
            IDeviceSpecificOidLookup oidTable = this.ObtainOidTable(model.Trim(), osVersion, out deviceVersion, lowerLayer.Address);
            if (string.IsNullOrWhiteSpace(deviceVersion.HandlerClassName))
            {
                try
                {
                    return new MikrotikSnmpDeviceHandler(lowerLayer, oidTable, osVersion, model, options);
                }
                catch(Exception ex)
                {
                    this.CollectException("MtikSnmp: OID table lookup", ex);
                    
                    // we want to catch and nest the exception here as the APIs involved are not able to append the infomration for which
                    // device (i.e. IP address) the exception is for
                    throw new HamnetSnmpException($"Failed to create MikroTik handler for device '{lowerLayer.Address}': {ex.Message}", ex, lowerLayer.Address?.ToString());
                }
            }
            else
            {
                return this.GetHandlerViaReflection(deviceVersion.HandlerClassName, lowerLayer, oidTable, osVersion, model, options);
            }
        }
    }
}
