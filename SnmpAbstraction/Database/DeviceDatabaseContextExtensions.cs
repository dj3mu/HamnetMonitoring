using System;
using System.Linq;
using SemVersion;

namespace SnmpAbstraction
{
    internal static class DeviceDatabaseContextExtensions
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Tries to obtain the device ID (database key) for the device of the given name.
        /// </summary>
        /// <param name="context">The device database context to extend.</param>
        /// <param name="deviceName">The device name to search for.</param>
        /// <param name="deviceId">Returns the database key, if the device of given name has been found.</param>
        /// <returns><c>true</c> if a device of the given name has been found and the ID returned. Otherwise <c>false</c>.</returns>
        public static bool TryFindDeviceId(this DeviceDatabaseContext context, string deviceName, out int deviceId)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "The context to search device ID on is null");
            }

            if (String.IsNullOrWhiteSpace(deviceName))
            {
                throw new ArgumentNullException(nameof(deviceName), "The device name to search device ID for is null, empty or white-space-only");
            }

            deviceId = -1;

            Device result = context.Devices.FirstOrDefault(d => d.Name == deviceName);

            if (result == null)
            {
                return false;
            }

            deviceId = result.Id;

            return true;
        }

        /// <summary>
        /// Tries to obtain the device ID (database key) for the device of the given name.
        /// </summary>
        /// <param name="context">The device database context to extend.</param>
        /// <param name="deviceId">The device ID to search versions for.</param>
        /// <param name="version">The version number to return the device version ID for.</param>
        /// <param name="deviceVersionId">Returns the database key, if a device version of for given device ID and version range has been found.</param>
        /// <returns><c>true</c> if a device of the given name has been found and the ID returned. Otherwise <c>false</c>.</returns>
        public static bool TryFindDeviceVersionId(this DeviceDatabaseContext context, int deviceId, SemanticVersion version, out int deviceVersionId)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "The context to search device ID on is null");
            }

            deviceVersionId = -1;

            var result = context.DeviceVersions.Where(d => d.DeviceId == deviceId).OrderByDescending(d => d.MinimumVersion).ToList();

            if (result.Count == 0)
            {
                // no such device ID at all
                return false;
            }

            if (version == null)
            {
                // no specific version requested, returns first entry (i.e. the one with the highest MinimumVersion)
                deviceVersionId = result[0].Id;
                return true;
            }

            // need to search for the best match of specified version:
            // we take the entry with the highest minimum version where the requested version is below maximum version
            foreach (DeviceVersion item in result)
            {
                if ((item.MinimumVersion <= version) && (item.MaximumVersion >= version))
                {
                    deviceVersionId = item.Id;
                    return true;
                }
            }

            log.Debug($"TryFindDeviceVersionId: No version range for device ID {deviceId} in database is applicable to requested version '{version}'");

            return false;
        }

        /// <summary>
        /// Tries to obtain the device ID (database key) for the device of the given name.
        /// </summary>
        /// <param name="context">The device database context to extend.</param>
        /// <param name="deviceVersionId">The device version ID to look up the mapping ID for.</param>
        /// <param name="oidMappingId">Returns the mapping ID, if found.</param>
        /// <returns><c>true</c> if a device of the given name has been found and the ID returned. Otherwise <c>false</c>.</returns>
        public static bool TryFindOidLookupId(this DeviceDatabaseContext context, int deviceVersionId, out int oidMappingId)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "The context to search device ID on is null");
            }

            oidMappingId = -1;

            var result = context.DeviceVersionMapping.FirstOrDefault(d => d.DeviceVersionId == deviceVersionId);

            if (result == null)
            {
                return false;
            }

            oidMappingId = result.OidMappingId;

            return true;
        }

        /// <summary>
        /// Tries to obtain the device ID (database key) for the device of the given name.
        /// </summary>
        /// <param name="context">The device database context to extend.</param>
        /// <param name="oidLookupId">The OID mapping lookup ID to get.</param>
        /// <param name="oidLookup">Returns the OID lookup, if found.</param>
        /// <returns><c>true</c> if a lookup of the given lookup ID has been found and the ID returned. Otherwise <c>false</c>.</returns>
        public static bool TryFindDeviceSpecificOidLookup(this DeviceDatabaseContext context, int oidLookupId, out DeviceSpecificOidLookup oidLookup)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context), "The context to search device ID on is null");
            }

            oidLookup = null;

            var result = context.DeviceSpecificOids.Where(d => d.OidMappingId == oidLookupId);

            if (!result.Any())
            {
                return false;
            }

            oidLookup = new DeviceSpecificOidLookup(result);

            return true;
        }
    }
}
