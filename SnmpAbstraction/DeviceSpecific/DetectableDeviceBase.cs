using System;
using System.Collections.Generic;
using SemVersion;

namespace SnmpAbstraction
{
    /// <summary>
    /// Base class for all detectable devices.
    /// </summary>
    internal abstract class DetectableDeviceBase : IDetectableDevice
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <inheritdoc />
        public abstract IDeviceHandler CreateHandler(ISnmpLowerLayer lowerLayer);

        /// <inheritdoc />
        public abstract bool IsApplicable(ISnmpLowerLayer snmpLowerLayer);

        /// <summary>
        /// Gets the OID lookup table for the specified device name and version.
        /// </summary>
        /// <param name="deviceName">The device name to look up.</param>
        /// <param name="version">The current version of the device.</param>
        /// <returns>The OID lookup table for the specified device name and version.</returns>
        protected IDeviceSpecificOidLookup ObtainOidTable(string deviceName, SemanticVersion version)
        {
            var database = DatabaseProvider.Instance.DeviceDatabase;

            int foundDeviceId = -1;
            if (!database.TryFindDeviceId(deviceName, out foundDeviceId))
            {
                var exception = new HamnetSnmpException($"Device name '{deviceName}' cannot be found in device database");
                log.Error(exception.Message);
                throw exception;
            }

            DeviceVersion foundDeviceVersion = null;
            if (!database.TryFindDeviceVersionId(foundDeviceId, version, out foundDeviceVersion))
            {
                var exception = new HamnetSnmpException($"Version '{version}' of device named '{deviceName}' (ID {foundDeviceId}) cannot be matched to any version range of device database");
                log.Error(exception.Message);
                throw exception;
            }

            string foundOidMappingIds = string.Empty;
            if (!database.TryFindOidLookupId(foundDeviceVersion.Id, out foundOidMappingIds))
            {
                var exception = new HamnetSnmpException($"Version '{version}' of device named '{deviceName}' (ID {foundDeviceVersion}) cannot be matched to any OID mapping ID of device database");
                log.Error(exception.Message);
                throw exception;
            }

            // need to convert the string containing a comma-separated list of OID lookup tables IDs into single, integer table IDs
            // Example: There are two lookups given in order "3,1" in the foundOidMappingIds string.
            //          If a RetrievableValuesEnum has a value in lookup of ID 3 that value shall be used. Otherwise the value of lookup #1.
            string[] splitOidMappingIds = foundOidMappingIds.Split(new char[] { ',', ';', ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            List<IDeviceSpecificOidLookup> orderedOidLookupsList = new List<IDeviceSpecificOidLookup>(splitOidMappingIds.Length);
            foreach(var sid in splitOidMappingIds)
            {
                int intId;
                if (!int.TryParse(sid, out intId))
                {
                    log.Warn($"OID mapping table ID '{sid}' as found for device '{deviceName}' v '{version}' (version ID {foundDeviceVersion}, mapping IDs '{foundOidMappingIds}') is not an integer value and will be ignored");
                    continue;
                }

                IDeviceSpecificOidLookup foundLookup = null;
                if (!database.TryFindDeviceSpecificOidLookup(intId, foundDeviceVersion.MaximumSupportedSnmpVersion.ToSnmpVersion(), out foundLookup))
                {
                    var exception = new HamnetSnmpException($"Version '{version}' of device named '{deviceName}': Cannot find OID mapping ID table of ID {intId} in device database");
                    log.Error(exception.Message);
                    throw exception;
                }

                orderedOidLookupsList.Add(foundLookup);
            }

            return new DeviceSpecificMultiLayerOidLookupProxy(orderedOidLookupsList);
        }
    }
}
