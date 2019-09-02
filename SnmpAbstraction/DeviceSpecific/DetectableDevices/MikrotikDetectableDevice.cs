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

        /// <inheritdoc />
        public override bool IsApplicable(ISnmpLowerLayer snmpLowerLayer)
        {
            if (!snmpLowerLayer.SystemData.Description.Contains(RouterOsDetectionString))
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public override IDeviceHandler CreateHandler(ISnmpLowerLayer lowerLayer)
        {
            string osVersionString = lowerLayer.QueryAsString(OsVersionOid, "MikroTik RouterOS Version String");

            // Example: "RouterOS 6.45.3 (stable) on RB711-5Hn-MMCX"
            Match match = OsVersionExtractionRegex.Match(osVersionString);

            SemanticVersion osVersion = match.Success ? SemanticVersion.Parse(match.Groups[1].Value) : null;

            return new MikrotikDeviceHandler(lowerLayer, this.ObtainOidTable(lowerLayer.SystemData.Description.Replace(RouterOsDetectionString, string.Empty).Trim(), osVersion));
        }
    }
}
