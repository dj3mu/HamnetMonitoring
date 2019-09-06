using System;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Base class for all device handlers
    /// </summary>
    internal abstract class DeviceHandlerBase : IDeviceHandler
    {
        /// <summary>
        /// Prevent default construction
        /// </summary>
        private DeviceHandlerBase()
        {
        }

        /// <summary>
        /// Constructs for the given lower layer.
        /// </summary>
        /// <param name="lowerLayer">The lower layer for talking to this device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="osVersion">The SW version of the device.</param>
        public DeviceHandlerBase(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup, SemVersion.SemanticVersion osVersion)
        {
            if (lowerLayer == null)
            {
                throw new ArgumentNullException(nameof(lowerLayer), "lower layer is null when constructing a device handler");
            }

            if (oidLookup == null)
            {
                throw new ArgumentNullException(nameof(oidLookup), "OID lookup table is null when constructing a device handler");
            }

            if (osVersion == null)
            {
                throw new ArgumentNullException(nameof(oidLookup), "OS version info is null when constructing a device handler");
            }

            this.LowerLayer = lowerLayer;
            this.OidLookup = oidLookup;
            this.OsVersion = osVersion;
        }

        /// <summary>
        /// Gets the lower layer to talk to this device.
        /// </summary>
        public ISnmpLowerLayer LowerLayer { get; }

        /// <summary>
        /// Gets the OID lookup table for this device.
        /// </summary>
        public IDeviceSpecificOidLookup OidLookup { get; }

        /// <inheritdoc />
        public IDeviceSystemData SystemData => this.LowerLayer.SystemData;

        /// <inheritdoc />
        public abstract IInterfaceDetails NetworkInterfaceDetails { get; }

        /// <inheritdoc />
        public abstract IWirelessPeerInfos WirelessPeerInfos { get; }

        /// <inheritdoc />
        public IpAddress Address => this.LowerLayer.Address;

        /// <inheritdoc />
        public SemanticVersion OsVersion { get; }
    }
}