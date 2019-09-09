using SemVersion;

namespace SnmpAbstraction
{
    /// <summary>
    /// Generic device Handler base class all devices.
    /// </summary>
    internal abstract class GenericDeviceHandler : DeviceHandlerBase
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
        /// <param name="model">The device's model name. Shall be the same name as used for the device name during OID database lookups.</param>
        public GenericDeviceHandler(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup, SemanticVersion osVersion, string model)
            : base(lowerLayer, oidLookup, osVersion, model)
        {
        }

        /// <inheritdoc />
        public override IInterfaceDetails NetworkInterfaceDetails
        {
            get
            {
                if (this.interfaceDetails == null)
                {
                    this.interfaceDetails = this.RetrieveDeviceInterfaceDetails(this.LowerLayer, this.OidLookup);
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
                    this.wirelessPeerInfos = this.RetrieveWirelessPeerInfos(this.LowerLayer, this.OidLookup);
                }

                return this.wirelessPeerInfos;
            }
        }

        /// <summary>
        /// Allows deriving class to retrieve and return their implementation of <see cref="IInterfaceDetails" />.
        /// </summary>
        /// <param name="lowerLayer">The lower layer for talking to this device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <returns>The retrieved interface details.</returns>
        protected abstract IInterfaceDetails RetrieveDeviceInterfaceDetails(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup);

        /// <summary>
        /// Allows deriving class to retrieve and return their implementation of <see cref="IWirelessPeerInfos" />.
        /// </summary>
        /// <param name="lowerLayer">The lower layer for talking to this device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <returns>The retrieved interface details.</returns>
        protected abstract IWirelessPeerInfos RetrieveWirelessPeerInfos(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup);
    }
}
