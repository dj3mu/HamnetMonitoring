using SemVersion;

namespace SnmpAbstraction
{
    /// <summary>
    /// Device Handler class for Mikrotik devices.
    /// </summary>
    internal class UbiquitiAirOsDeviceHandler : GenericDeviceHandler
    {
        /// <summary>
        /// Constructs for the given lower layer.
        /// </summary>
        /// <param name="lowerLayer">The lower layer for talking to this device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="osVersion">The SW version of the device.</param>
        public UbiquitiAirOsDeviceHandler(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup, SemanticVersion osVersion)
            : base(lowerLayer, oidLookup, osVersion)
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
            return this.OsVersion >= new SemanticVersion(6, 0, 0)
                ? new LazyLoadingUbiquitiAirOs6plusWirelessPeerInfos(this.LowerLayer, this.OidLookup) as IWirelessPeerInfos
                : new LazyLoadingUbiquitiAirOs4WirelessPeerInfos(this.LowerLayer, this.OidLookup) as IWirelessPeerInfos;
        }
    }
}
