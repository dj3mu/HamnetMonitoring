namespace SnmpAbstraction
{
    internal class MikrotikDeviceHandler : DeviceHandlerBase
    {
        /// <summary>
        /// Constructs for the given lower layer.
        /// </summary>
        /// <param name="lowerLayer">The lower layer for talking to this device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        public MikrotikDeviceHandler(ISnmpLowerLayer lowerLayer, DeviceSpecificOidLookup oidLookup)
            : base(lowerLayer, oidLookup)
        {
        }
    }
}