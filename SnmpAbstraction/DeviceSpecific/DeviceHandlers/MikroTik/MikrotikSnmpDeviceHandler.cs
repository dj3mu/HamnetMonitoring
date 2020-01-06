using System;
using SemVersion;
using SnmpSharpNet;
using tik4net;

namespace SnmpAbstraction
{
    /// <summary>
    /// Device Handler class for Mikrotik devices using SNMP communication.
    /// </summary>
    internal class MikrotikSnmpDeviceHandler : GenericDeviceHandler
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
        public MikrotikSnmpDeviceHandler(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup, SemanticVersion osVersion, string model, IQuerierOptions options)
            : base(lowerLayer, oidLookup, osVersion, model, options)
        {
            if ((options.AllowedApis & this.SupportedApi) == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options), $"This device handler doesn't support any of the APIs allowed by the IQuerierOptions (allowed: {options.AllowedApis}, supported {this.SupportedApi}).");
            }

            LazyLoadingDeviceSystemData llsd = lowerLayer.SystemData as LazyLoadingDeviceSystemData;
            if (llsd != null)
            {
                if ((oidLookup.TryGetValue(RetrievableValuesEnum.RxSignalStrengthImmediateOid, out DeviceSpecificOid oid) && !oid.Oid.IsNull)
                   || ((oidLookup.TryGetValue(RetrievableValuesEnum.RxSignalStrengthCh0AppendMacAndInterfaceId, out DeviceSpecificOid oid0) && !oid0.Oid.IsNull)
                   || (oidLookup.TryGetValue(RetrievableValuesEnum.RxSignalStrengthCh1AppendMacAndInterfaceId, out DeviceSpecificOid oid1) && !oid1.Oid.IsNull)
                   || (oidLookup.TryGetValue(RetrievableValuesEnum.RxSignalStrengthCh2AppendMacAndInterfaceId, out DeviceSpecificOid oid2) && !oid2.Oid.IsNull)))
                {
                    // for SNMP-based MikroTik devices we currently only support RSSI querying
                    llsd.SupportedFeatures = DeviceSupportedFeatures.Rssi;
                }
            }
        }

        /// <summary>
        /// Gets the tik4Net connection handle to use for talking to this device.
        /// </summary>
        public ITikConnection TikConnection
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
        public override QueryApis SupportedApi { get; } = QueryApis.Snmp;

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
        public override ITracerouteResult Traceroute(IpAddress remoteIp, uint count, TimeSpan timeout, int maxHops)
        {
            return new MikrotikApiTracerouteOperation(this.LowerLayer.Address, this.TikConnection, remoteIp, count, timeout, maxHops).Execute(); 
        }

        /// <inheritdoc />
        public override IBgpPeers FetchBgpPeers(string remotePeerIp)
        {
            return new LazyLoadingMikrotikBgpPeerInfos(this.LowerLayer.Address, this.TikConnection, remotePeerIp);
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
