using System;
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
        protected DeviceSpecificOidLookup ObtainOidTable(string deviceName, SemanticVersion version)
        {
            var database = DatabaseProvider.Instance.DeviceDatabase;

            int foundDeviceId = -1;
            if (!database.TryFindDeviceId(deviceName, out foundDeviceId))
            {
                var exception = new HamnetSnmpException($"Device name '{deviceName}' cannot be found in device database");
                log.Error(exception.Message);
                throw exception;
            }

            int foundDeviceVersionId = -1;
            if (!database.TryFindDeviceVersionId(foundDeviceId, version, out foundDeviceVersionId))
            {
                var exception = new HamnetSnmpException($"Version '{version}' of device named '{deviceName}' (ID {foundDeviceId}) cannot be matched to any version range of device database");
                log.Error(exception.Message);
                throw exception;
            }

            int foundOidMappingId = -1;
            if (!database.TryFindOidLookupId(foundDeviceVersionId, out foundOidMappingId))
            {
                var exception = new HamnetSnmpException($"Version '{version}' of device named '{deviceName}' (ID {foundDeviceVersionId}) cannot be matched to any OID mapping ID of device database");
                log.Error(exception.Message);
                throw exception;
            }

            DeviceSpecificOidLookup foundLookup = null;
            if (!database.TryFindDeviceSpecificOidLookup(foundOidMappingId, out foundLookup))
            {
                var exception = new HamnetSnmpException($"Version '{version}' of device named '{deviceName}': Cannot find OID mapping ID table of ID {foundOidMappingId} in device database");
                log.Error(exception.Message);
                throw exception;
            }

            return foundLookup;
        }
    }
}
