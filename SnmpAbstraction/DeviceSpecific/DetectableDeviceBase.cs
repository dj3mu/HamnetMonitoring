using System;
using System.Collections.Generic;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Base class for all detectable devices.
    /// </summary>
    internal abstract class DetectableDeviceBase : IDetectableDevice
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private int circularCreateHandler = 0;

        private List<InfoAndException> collectedExceptions = new List<InfoAndException>();

        /// <inheritdoc />
        public abstract QueryApis SupportedApi { get; }

        /// <inheritdoc />
        public int Priority { get; protected set; } = 100;

        /// <inheritdoc />
        public IReadOnlyCollection<IInfoAndException> CollectedExceptions => this.collectedExceptions;

        /// <inheritdoc />
        public virtual IDeviceHandler CreateHandler(ISnmpLowerLayer lowerLayer, IQuerierOptions options)
        {
            if(this.circularCreateHandler++ > 1)
            {
                var ex = new InvalidOperationException($"Internal Error: DetectableDevice {this.GetType().Name} seems to neither implement CreateHandler(ISnmpLowerLayer lowerLayer, IQuerierOptions options) nor CreateHandler(IpAddress address, IQuerierOptions options)");
                this.CollectException("CreateHandler(ISnmpLowerLayer, IQuerierOptions) circular call", ex);
                throw ex;
            }

            return this.CreateHandler(lowerLayer.Address, options);
        }

        /// <inheritdoc />
        public virtual IDeviceHandler CreateHandler(IpAddress address, IQuerierOptions options)
        {
            if(this.circularCreateHandler++ > 1)
            {
                var ex = new InvalidOperationException($"Internal Error: DetectableDevice {this.GetType().Name} seems to neither implement CreateHandler(ISnmpLowerLayer lowerLayer, IQuerierOptions options) nor CreateHandler(IpAddress address, IQuerierOptions options)");
                this.CollectException("CreateHandler(IpAddress, IQuerierOptions) circular call", ex);
                throw ex;
            }

            var lowerLayer = new SnmpLowerLayer(address, options);
            return this.CreateHandler(lowerLayer, options);
        }

        /// <inheritdoc />
        public abstract bool IsApplicableSnmp(ISnmpLowerLayer snmpLowerLayer, IQuerierOptions options);

        /// <inheritdoc />
        public abstract bool IsApplicableVendorSpecific(IpAddress address, IQuerierOptions options);

        /// <summary>
        /// Collectes the given exception.
        /// </summary>
        /// <param name="info">The addtional information.</param>
        /// <param name="ex">The exception to collect.</param>
        protected void CollectException(string info, Exception ex)
        {
            this.collectedExceptions.Add(new InfoAndException { Info = string.IsNullOrWhiteSpace(info) ? string.Empty : info, Exception = ex });
        }

        /// <summary>
        /// Gets the device handler of the given handler class name via reflection.
        /// </summary>
        /// <param name="handlerClassName">The class name of the device handler to get.</param>
        /// <param name="lowerLayer">The lower layer for talking to this device.</param>
        /// <param name="oidTable">The OID lookup table for the device.</param>
        /// <param name="osVersion">The SW version of the device.</param>
        /// <param name="model">The device's model name. Shall be the same name as used for the device name during OID database lookups.</param>
        /// <param name="options">The options to use.</param>
        /// <returns>The generated device handler.</returns>
        protected IDeviceHandler GetHandlerViaReflection(string handlerClassName, ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidTable, SemanticVersion osVersion, string model, IQuerierOptions options)
        {
            var type = Type.GetType($"SnmpAbstraction.{handlerClassName}");
            if (type == null)
            {
                var ex = new HamnetSnmpException($"{lowerLayer.Address} ({model} v {osVersion}): Cannot find a DeviceHandler implementation of name '{handlerClassName}'", lowerLayer.Address?.ToString());
                this.CollectException("Missing handler class name", ex);
                throw ex;
            }

            object myObject = null;
            try
            {
                myObject = Activator.CreateInstance(type, lowerLayer, oidTable, osVersion, model, options);
            }
            catch(Exception ex)
            {
                this.CollectException($"Instantiate handler class '{type.FullName}'", ex);

                throw new HamnetSnmpException($"{lowerLayer.Address} ({model} v {osVersion}): Exception while instantiating DeviceHandler of name '{handlerClassName}': {ex.Message}", ex, lowerLayer.Address?.ToString());
            }

            IDeviceHandler castedHandler = myObject as IDeviceHandler;
            if (castedHandler == null)
            {
                var ex = new HamnetSnmpException($"{lowerLayer.Address} ({model} v {osVersion}): Instantiating DeviceHandler of name '{handlerClassName}' is NOT an IDeviceHandler", lowerLayer.Address?.ToString());
                this.CollectException($"Cast handler class '{type.FullName}' to IDeviceHandler", ex);
                throw ex;
            }

            return castedHandler;
        }

        /// <summary>
        /// Gets the OID lookup table for the specified device name and version.
        /// </summary>
        /// <param name="deviceName">The device name to look up.</param>
        /// <param name="version">The current version of the device.</param>
        /// <param name="deviceVersion">Returns the device version container matching this device.</param>
        /// <param name="deviceAddress">The IP address of the device (only used to include it with possible exceptions).</param>
        /// <returns>The OID lookup table for the specified device name and version.</returns>
        protected IDeviceSpecificOidLookup ObtainOidTable(string deviceName, SemanticVersion version, out DeviceVersion deviceVersion, IpAddress deviceAddress)
        {
            using(var database = DeviceDatabaseProvider.Instance.DeviceDatabase)
            {
                int foundDeviceId = -1;
                if (!database.TryFindDeviceId(deviceName, out foundDeviceId))
                {
                    var exception = new HamnetSnmpException($"Device name '{deviceName}' cannot be found in device database", deviceAddress?.ToString());
                    log.Error(exception.Message);
                    this.CollectException("No OID lookup for device name", exception);
                    throw exception;
                }

                deviceVersion = null;
                if (!database.TryFindDeviceVersionId(foundDeviceId, version, out deviceVersion))
                {
                    var exception = new HamnetSnmpException($"Version '{version}' of device named '{deviceName}' (ID {foundDeviceId}) cannot be matched to any version range of device database", deviceAddress?.ToString());
                    log.Error(exception.Message);
                    this.CollectException("No OID lookup for device version", exception);
                    throw exception;
                }

                string foundOidMappingIds = string.Empty;
                if (!database.TryFindOidLookupId(deviceVersion.Id, out foundOidMappingIds))
                {
                    var exception = new HamnetSnmpException($"Version '{version}' of device named '{deviceName}' (ID {foundDeviceId}, version ID {deviceVersion.Id}) cannot be matched to any OID mapping ID of device database", deviceAddress?.ToString());
                    log.Error(exception.Message);
                    this.CollectException("No OID mapping for device version", exception);
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
                    if (!database.TryFindDeviceSpecificOidLookup(intId, deviceVersion.MinimumSupportedSnmpVersion?.ToSnmpVersion() ?? SnmpVersion.Ver1, deviceVersion.MaximumSupportedSnmpVersion.ToSnmpVersion(), out foundLookup))
                    {
                        var exception = new HamnetSnmpException($"Version '{version}' of device named '{deviceName}': Cannot find OID mapping ID table of ID {intId} in device database", deviceAddress?.ToString());
                        log.Error(exception.Message);
                        this.CollectException("No OID mapping for mapping ID", exception);
                        throw exception;
                    }

                    orderedOidLookupsList.Add(foundLookup);
                }

                return new DeviceSpecificMultiLayerOidLookupProxy(orderedOidLookupsList);
            }
        }
    }
}
