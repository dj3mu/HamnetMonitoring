using System;
using System.Linq;
using System.Text.RegularExpressions;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Detectable device implementation for MikroTik devices
    /// </summary>
    internal class AlixDetectableDevice : DetectableDeviceBase
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// String in system description to detect Alix devices
        /// </summary>
        private static readonly string[] AlixDetectionStrings = { "H4L HAMNET", "ALIX", "adult playground" };

        private static readonly Regex OsVersionExtractionRegex = new Regex(@"\s+([0-9.]+)\s+");

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

            if (!AlixDetectionStrings.Any(ds => description.Contains(ds, StringComparison.InvariantCultureIgnoreCase)))
            {
                log.Info($"Description in system data of device '{snmpLowerLayer.Address}' doesn't contain any of the strings string '{string.Join(", ", AlixDetectionStrings)}': Assuming the device is not an ALIX device");
                return false;
            }

            log.Info($"Device '{snmpLowerLayer.Address}' seems to be an ALIX device");
            
            return true;
        }

        /// <inheritdoc />
        public override IDeviceHandler CreateHandler(ISnmpLowerLayer lowerLayer)
        {
            string osVersionString = "0.0.0";

            //// try #1: In IEEE SNMP tree
            //try
            //{
            //    osVersionString = lowerLayer.QueryAsString(OsVersionOid, "ALIX Version String #1");
            //}
            //catch(SnmpException)
            //{
            //    osVersionString = null;
            //}
            //catch(HamnetSnmpException)
            //{
            //    osVersionString = null;
            //}

            // try #2: Withing MikroTik enterprise tree
            //if (string.IsNullOrWhiteSpace(osVersionString))
            //{
            //    osVersionString = lowerLayer.QueryAsString(OsVersionOid2, "ALIX Version String #2");
            //}

            // Example: "..."
            Match match = OsVersionExtractionRegex.Match(osVersionString);

            SemanticVersion osVersion = osVersionString.ToSemanticVersion(); // match.Success ? match.Groups[1].Value.ToSemanticVersion() : null;

            var model = lowerLayer.SystemData.Description; //lowerLayer.SystemData.Description.Replace(RouterOsDetectionString, string.Empty).Trim();

            log.Info($"Detected device '{lowerLayer.Address}' as ALIX '{model}' v '{osVersion}'");

            DeviceVersion deviceVersion;
            IDeviceSpecificOidLookup oidTable = this.ObtainOidTable(model.Trim(), osVersion, out deviceVersion, lowerLayer.Address);
            if (string.IsNullOrWhiteSpace(deviceVersion.HandlerClassName))
            {
                try
                {
                    return new AlixDeviceHandler(lowerLayer, oidTable, osVersion, model);
                }
                catch(Exception ex)
                {
                    // we want to catch and nest the exception here as the APIs involved are not able to append the infomration for which
                    // device (i.e. IP address) the exception is for
                    throw new HamnetSnmpException($"Failed to create ALIX handler for device '{lowerLayer.Address}': {ex.Message}", ex, lowerLayer?.Address?.ToString());
                }
            }
            else
            {
                return this.GetHandlerViaReflection(deviceVersion.HandlerClassName, lowerLayer, oidTable, osVersion, model);
            }
        }
    }
}
