using SemVersion;
using tik4net;

namespace SnmpAbstraction
{
    /// <summary>
    /// Device Handler class for Mikrotik devices.
    /// </summary>
    internal class MikrotikDeviceHandler : GenericDeviceHandler
    {
        /// <summary>
        /// The minimum version for support of MTik API v2.
        /// </summary>
        private static readonly SemanticVersion ApiV2MinimumVersion = new SemanticVersion(6, 45, 0, string.Empty, string.Empty);

        /// <summary>
        /// To avoid redundant calls to Dispose.
        /// </summary>
        private bool disposedValue = false;

        private ITikConnection tikConnectionBacking = null;

        /// <summary>
        /// Constructs for the given lower layer.
        /// </summary>
        /// <param name="lowerLayer">The lower layer for talking to this device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="osVersion">The SW version of the device.</param>
        /// <param name="model">The device's model name. Shall be the same name as used for the device name during OID database lookups.</param>
        /// <param name="options">The options to use.</param>
        public MikrotikDeviceHandler(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup, SemanticVersion osVersion, string model, IQuerierOptions options)
            : base(lowerLayer, oidLookup, osVersion, model, options)
        {
        }

        /// <summary>
        /// Gets the tik4Net connection handle to use for talking to this device.
        /// </summary>
        protected ITikConnection TikConnection
        {
            get
            {
                if (this.tikConnectionBacking == null)
                {
                    this.tikConnectionBacking = ConnectionFactory.CreateConnection((this.OsVersion < ApiV2MinimumVersion) ? TikConnectionType.Api : TikConnectionType.Api_v2);
                }

                if (!this.tikConnectionBacking.IsOpened)
                {
                    this.tikConnectionBacking.Open(this.Address.ToString(), this.Options.LoginUser ?? string.Empty, this.Options.LoginPassword ?? string.Empty);
                }

                return this.tikConnectionBacking;
            }
        }

        /// <inheritdoc />
        protected override IInterfaceDetails RetrieveDeviceInterfaceDetails(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup)
        {
            return new LazyLoadingGenericInterfaceDetails(this.LowerLayer, this.OidLookup);
        }

        /// <inheritdoc />
        protected override IWirelessPeerInfos RetrieveWirelessPeerInfos(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup)
        {
            return new LazyLoadingMikroTikWirelessPeerInfos(this.LowerLayer, this.OidLookup);
        }

        /// <inheritdoc />
        public override IBgpPeers FetchBgpPeers(string remotePeerIp)
        {
            return new LazyLoadingMikrotikBgpPeerInfos(this.LowerLayer, this.OidLookup, this.TikConnection, remotePeerIp);
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.tikConnectionBacking != null)
                    {
                        this.tikConnectionBacking.Dispose();
                        this.tikConnectionBacking = null;
                    }
                }

                this.disposedValue = true;
            }
        }
    }
}
