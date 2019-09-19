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
        /// Gets the device handler of the given handler class name via reflection.
        /// </summary>
        /// <param name="handlerClassName">The class name of the device handler to get.</param>
        /// <param name="lowerLayer">The lower layer for talking to this device.</param>
        /// <param name="oidTable">The OID lookup table for the device.</param>
        /// <param name="osVersion">The SW version of the device.</param>
        /// <param name="model">The device's model name. Shall be the same name as used for the device name during OID database lookups.</param>
        /// <returns>The generated device handler.</returns>
        protected IDeviceHandler GetHandlerViaReflection(string handlerClassName, ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidTable, SemanticVersion osVersion, string model)
        {
            var type = Type.GetType($"SnmpAbstraction.{handlerClassName}");
            if (type == null)
            {
                throw new HamnetSnmpException($"{lowerLayer.Address} ({model} v {osVersion}): Cannot find a DeviceHandler implementation of name '{handlerClassName}'");
            }

            object myObject = null;
            try
            {
                myObject = Activator.CreateInstance(type, lowerLayer, oidTable, osVersion, model);
            }
            catch(Exception ex)
            {
                throw new HamnetSnmpException($"{lowerLayer.Address} ({model} v {osVersion}): Exception while instantiating DeviceHandler of name '{handlerClassName}': {ex.Message}", ex);
            }

            IDeviceHandler castedHandler = myObject as IDeviceHandler;
            if (castedHandler == null)
            {
                throw new HamnetSnmpException($"{lowerLayer.Address} ({model} v {osVersion}): Instantiating DeviceHandler of name '{handlerClassName}' is NOT an IDeviceHandler");
            }

            return castedHandler;
        }

        /// <summary>
        /// Gets the OID lookup table for the specified device name and version.
        /// </summary>
        /// <param name="deviceName">The device name to look up.</param>
        /// <param name="version">The current version of the device.</param>
        /// <param name="deviceVersion">Returns the device version container matching this device.</param>
        /// <returns>The OID lookup table for the specified device name and version.</returns>
        protected IDeviceSpecificOidLookup ObtainOidTable(string deviceName, SemanticVersion version, out DeviceVersion deviceVersion)
        {
            using(var database = DatabaseProvider.Instance.DeviceDatabase)
            {
                int foundDeviceId = -1;
                if (!database.TryFindDeviceId(deviceName, out foundDeviceId))
                {
                    var exception = new HamnetSnmpException($"Device name '{deviceName}' cannot be found in device database");
                    log.Error(exception.Message);
                    throw exception;
                }

                deviceVersion = null;
                if (!database.TryFindDeviceVersionId(foundDeviceId, version, out deviceVersion))
                {
                    var exception = new HamnetSnmpException($"Version '{version}' of device named '{deviceName}' (ID {foundDeviceId}) cannot be matched to any version range of device database");
                    log.Error(exception.Message);
                    throw exception;
                }

                string foundOidMappingIds = string.Empty;
                if (!database.TryFindOidLookupId(deviceVersion.Id, out foundOidMappingIds))
                {
                    var exception = new HamnetSnmpException($"Version '{version}' of device named '{deviceName}' (ID {foundDeviceId}, version ID {deviceVersion.Id}) cannot be matched to any OID mapping ID of device database");
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
                        log.Warn($"OID mapping table ID '{sid}' as found for device '{deviceName}' v '{version}' (version ID {deviceVersion.Id}, mapping IDs '{foundOidMappingIds}') is not an integer value and will be ignored");
                        continue;
                    }

                    IDeviceSpecificOidLookup foundLookup = null;
                    if (!database.TryFindDeviceSpecificOidLookup(intId, deviceVersion.MaximumSupportedSnmpVersion.ToSnmpVersion(), out foundLookup))
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
}
