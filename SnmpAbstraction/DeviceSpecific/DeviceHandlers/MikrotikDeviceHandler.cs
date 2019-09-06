using SemVersion;

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
        /// Backing field for wireless peer information.
        /// </summary>
        private IWirelessPeerInfos wirelessPeerInfos = null;

        /// <summary>
        /// Constructs for the given lower layer.
        /// </summary>
        /// <param name="lowerLayer">The lower layer for talking to this device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="osVersion">The SW version of the device.</param>
        public MikrotikDeviceHandler(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup, SemanticVersion osVersion)
            : base(lowerLayer, oidLookup, osVersion)
        {
        }

        /// <inheritdoc />
        public override IInterfaceDetails NetworkInterfaceDetails
        {
            get
            {
                if (this.interfaceDetails == null)
                {
                    this.interfaceDetails = new LazyLoadingMikroTikDeviceInterfaceDetails(this.LowerLayer, this.OidLookup);
                }

                return this.interfaceDetails;
            }
        }

        /// <inheritdoc />
        public override IWirelessPeerInfos WirelessPeerInfos
        {
            get
            {
                if (this.wirelessPeerInfos == null)
                {
                    this.wirelessPeerInfos = new LazyLoadingMikroTikWirelessPeerInfos(this.LowerLayer, this.OidLookup);
                }

                return this.wirelessPeerInfos;
            }
        }
    }
}
