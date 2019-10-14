using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.Routing.Bgp;

namespace SnmpAbstraction
{
    /// <summary>
    /// Container for a lazy-loading (on first request) list of BGP peer infos.
    /// </summary>
    internal class LazyLoadingMikrotikBgpPeerInfos : LazyHamnetSnmpQuerierResultBase, IBgpPeers
    {
        private IDeviceSpecificOidLookup oidLookup;

        private IReadOnlyList<IBgpPeer> detailsBacking = null;

        private TimeSpan queryDurationBacking = TimeSpan.Zero;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="tikConnection">The tik4Net connection to use for talking to the device.</param>
        /// <param name="remotePeerIp">The remote peer to get details for. If null or empty, fetching all peer's info.</param>
        public LazyLoadingMikrotikBgpPeerInfos(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup, ITikConnection tikConnection, string remotePeerIp)
            : base(lowerLayer)
        {
            this.oidLookup = oidLookup ?? throw new ArgumentNullException(nameof(oidLookup), "The OID lookup structure is null when constructing LazyLoadingMikrotikBgpPeerInfos");
            this.TikConnection = tikConnection ?? throw new ArgumentNullException(nameof(tikConnection), "This tik4Net connection is null when constructing LazyLoadingMikrotikBgpPeerInfos");
            this.RemotePeerIp = remotePeerIp;
        }

        /// <inheritdoc />
        public IReadOnlyList<IBgpPeer> Details
        {
            get
            {
                this.FetchPeers();
                return this.detailsBacking;
            }
        }

        /// <inheritdoc />
        /// public override TimeSpan QueryDuration => this.queryDurationBacking;

        /// <summary>
        /// Gets the connection to the MikroTik API
        /// </summary>
        protected ITikConnection TikConnection { get; }
        
        /// <summary>
        /// Gets the remote peer IP to get the BGP info for.
        /// </summary>
        protected string RemotePeerIp { get; }

        /// <inheritdoc />
        public override void ForceEvaluateAll()
        {
            this.FetchPeers();
            foreach (IBgpPeer item in this.detailsBacking)
            {
                item.ForceEvaluateAll();
            }
        }

        /// <inheritdoc />
        public IEnumerator<IBgpPeer> GetEnumerator()
        {
            this.FetchPeers();
            return this.detailsBacking.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            this.FetchPeers();
            return this.detailsBacking.GetEnumerator();
        }

        /// <summary>
        /// Actually fetches the peers.
        /// </summary>
        private void FetchPeers()
        {
            if (this.detailsBacking != null)
            {
                return;
            }

            Stopwatch stopper = Stopwatch.StartNew();

            IEnumerable<BgpPeer> peers = null;
            if (!string.IsNullOrWhiteSpace(this.RemotePeerIp))
            {
                peers = this.TikConnection.LoadList<BgpPeer>(this.TikConnection.CreateParameter("remote-address", this.RemotePeerIp));
            }
            else
            {
                peers = this.TikConnection.LoadList<BgpPeer>();
            }

            stopper.Stop();

            this.queryDurationBacking = stopper.Elapsed;

            this.detailsBacking = peers.Select(p => new LazyLoadingMikrotikBgpPeerInfo(this.LowerSnmpLayer, this.TikConnection, p)).ToList();
        }
    }
}
