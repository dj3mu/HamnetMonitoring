using SemVersion;

namespace SnmpAbstraction
{
    /// <summary>
    /// Device Handler class for UNT devices of AirOs vesions below 5.6.
    /// </summary>
    internal class UbiquitiAirOsUpTo56DeviceHandler : GenericDeviceHandler
    {
        /// <summary>
        /// Constructs for the given lower layer.
        /// </summary>
        /// <param name="lowerLayer">The lower layer for talking to this device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="osVersion">The SW version of the device.</param>
        /// <param name="model">The device's model name. Shall be the same name as used for the device name during OID database lookups.</param>
        public UbiquitiAirOsUpTo56DeviceHandler(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup, SemanticVersion osVersion, string model)
            : base(lowerLayer, oidLookup, osVersion, model)
        {
        }

        /// <inheritdoc />
        protected override IInterfaceDetails RetrieveDeviceInterfaceDetails(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup)
        {
            return new LazyLoadingUbiquitiInterfaceDetails(this.LowerLayer, this.OidLookup);
        }

        /// <inheritdoc />
        protected override IWirelessPeerInfos RetrieveWirelessPeerInfos(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup)
        {
            return new LazyLoadingUbiquitiAirOsUpTo56WirelessPeerInfos(this.LowerLayer, this.OidLookup) as IWirelessPeerInfos;
        }
    }
}
