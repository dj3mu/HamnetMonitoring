using System;

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
        public DeviceHandlerBase(ISnmpLowerLayer lowerLayer, DeviceSpecificOidLookup oidLookup)
        {
            if (lowerLayer == null)
            {
                throw new ArgumentNullException(nameof(lowerLayer), "lower layer is null when constructing a device handler");
            }

            if (oidLookup == null)
            {
                throw new ArgumentNullException(nameof(oidLookup), "OID lookup table is null when constructing a device handler");
            }

            this.LowerLayer = lowerLayer;
            this.OidLookup = oidLookup;
        }

        /// <summary>
        /// Gets the lower layer to talk to this device.
        /// </summary>
        public ISnmpLowerLayer LowerLayer { get; }

        /// <summary>
        /// Gets the OID lookup table for this device.
        /// </summary>
        public DeviceSpecificOidLookup OidLookup { get; }

        /// <inheritdoc />
        public IDeviceSystemData SystemData => this.LowerLayer.SystemData;
    }
}