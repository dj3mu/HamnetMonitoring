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
                var info = $"Description in system data of device '{snmpLowerLayer.Address}' is null, empty or white-space-only: Assuming the device is not an Alix device";
                this.CollectException("AlixSnmp: No device description", new HamnetSnmpException(info));
                log.Warn(info);
                return false;
            }

            if (!AlixDetectionStrings.Any(ds => description.Contains(ds, StringComparison.InvariantCultureIgnoreCase)))
            {
                var info = $"Description in system data of device '{snmpLowerLayer.Address}' doesn't contain any of the strings string '{string.Join(", ", AlixDetectionStrings)}': Assuming the device is not an ALIX device";
                this.CollectException("AlixSnmp: No ALIX-like string in device description", new HamnetSnmpException(info));
                log.Info(info);
                return false;
            }

            log.Info($"Device '{snmpLowerLayer.Address}' seems to be an ALIX device");

            return true;
        }

        /// <inheritdoc />
        public override IDeviceHandler CreateHandler(ISnmpLowerLayer lowerLayer, IQuerierOptions options)
        {
            string osVersionString = "0.0.0";

            // Example: "..."
            Match match = OsVersionExtractionRegex.Match(osVersionString);

            SemanticVersion osVersion = osVersionString.ToSemanticVersion(); // match.Success ? match.Groups[1].Value.ToSemanticVersion() : null;

            var model = lowerLayer.SystemData.Description; //lowerLayer.SystemData.Description.Replace(RouterOsDetectionString, string.Empty).Trim();

            log.Info($"Detected device '{lowerLayer.Address}' as ALIX '{model}' v '{osVersion}'");

            IDeviceSpecificOidLookup oidTable = this.ObtainOidTable(model.Trim(), osVersion, out DeviceVersion deviceVersion, lowerLayer.Address);
            if (string.IsNullOrWhiteSpace(deviceVersion.HandlerClassName))
            {
                try
                {
                    return new AlixDeviceHandler(lowerLayer, oidTable, osVersion, model, options);
                }
                catch(Exception ex)
                {
                    this.CollectException("AlixSnmp: OID table lookup", ex);

                    // we want to catch and nest the exception here as the APIs involved are not able to append the infomration for which
                    // device (i.e. IP address) the exception is for
                    throw new HamnetSnmpException($"Failed to create ALIX handler for device '{lowerLayer.Address}': {ex.Message}", ex, lowerLayer?.Address?.ToString());
                }
            }
            else
            {
                return this.GetHandlerViaReflection(deviceVersion.HandlerClassName, lowerLayer, oidTable, osVersion, model, options);
            }
        }
    }
}
