using System;
using System.Text.RegularExpressions;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Detectable device implementation for MikroTik devices
    /// </summary>
    internal class MikrotikDetectableDevice : DetectableDeviceBase
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// String in system description to detect RouterOS
        /// </summary>
        private const string RouterOsDetectionString = "RouterOS";

        private static readonly Regex OsVersionExtractionRegex = new Regex(RouterOsDetectionString + @"\s+([0-9.]+)\s+");

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
        public override bool IsApplicable(ISnmpLowerLayer snmpLowerLayer)
        {
            var description = snmpLowerLayer?.SystemData?.Description;
            if (string.IsNullOrWhiteSpace(description))
            {
                log.Warn($"Description in system data of device '{snmpLowerLayer.Address}' is null, empty or white-space-only: Assuming the device is not a MikroTik device");
                return false;
            }

            if (!description.Contains(RouterOsDetectionString))
            {
                log.Info($"Description in system data of device '{snmpLowerLayer.Address}' doesn't contain string '{RouterOsDetectionString}': Assuming the device is not a MikroTik device");
                return false;
            }

            log.Info($"Device '{snmpLowerLayer.Address}' seems to be a MikroTik device");
            
            return true;
        }

        /// <inheritdoc />
        public override IDeviceHandler CreateHandler(ISnmpLowerLayer lowerLayer)
        {
            string osVersionString = null;

            // try #1: In IEEE SNMP tree
            try
            {
                osVersionString = lowerLayer.QueryAsString(OsVersionOid, "MikroTik RouterOS Version String #1");
            }
            catch(SnmpException)
            {
                osVersionString = null;
            }
            catch(HamnetSnmpException)
            {
                osVersionString = null;
            }

            // try #2: Withing MikroTik enterprise tree
            if (string.IsNullOrWhiteSpace(osVersionString))
            {
                osVersionString = lowerLayer.QueryAsString(OsVersionOid2, "MikroTik RouterOS Version String #2");
            }

            // Example: "RouterOS 6.45.3 (stable) on RB711-5Hn-MMCX"
            Match match = OsVersionExtractionRegex.Match(osVersionString);

            SemanticVersion osVersion = match.Success ? SemanticVersion.Parse(match.Groups[1].Value) : null;

            var model = lowerLayer.SystemData.Description.Replace(RouterOsDetectionString, string.Empty).Trim();

            log.Info($"Detected device '{lowerLayer.Address}' as MikroTik '{model}' v '{osVersion}'");

            try
            {
                return new MikrotikDeviceHandler(lowerLayer, this.ObtainOidTable(model, osVersion), osVersion, model);
            }
            catch(Exception ex)
            {
                // we want to catch and nest the exception here as the APIs involved are not able to append the infomration for which
                // device (i.e. IP address) the exception is for
                throw new HamnetSnmpException($"Failed to create MikroTik handler for device '{lowerLayer.Address}': {ex.Message}", ex);
            }
        }
    }
}
