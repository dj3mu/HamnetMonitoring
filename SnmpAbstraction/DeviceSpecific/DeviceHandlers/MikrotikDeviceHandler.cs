namespace SnmpAbstraction
{
    /// <summary>
    /// Device Handler class for Mikrotik devices.
    /// </summary>
    internal class MikrotikDeviceHandler : DeviceHandlerBase
    {
        /// <summary>
        /// Backing field for interface details information.
        /// </summary>
        private IInterfaceDetails interfaceDetails = null;

        /// <summary>
        /// Constructs for the given lower layer.
        /// </summary>
        /// <param name="lowerLayer">The lower layer for talking to this device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        public MikrotikDeviceHandler(ISnmpLowerLayer lowerLayer, DeviceSpecificOidLookup oidLookup)
            : base(lowerLayer, oidLookup)
        {
        }

        /// <inheritdoc />
        public override IInterfaceDetails NetworkInterfaceDetails
        {
            get
            {
                if (this.interfaceDetails == null)
                {
                    this.interfaceDetails = new LazyLoadingDeviceInterfaceDetails(this.LowerLayer, this.OidLookup);
                }

                return this.interfaceDetails;
            }
        }
    }
}
