using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SnmpAbstraction;
using SnmpSharpNet;

namespace RestService.DataFetchingService
{
    internal class BgpPeersStoreOnlyContainer : IBgpPeers
    {
        private List<IBgpPeer> detailsBacking;

        /// <summary>
        /// Copy-construct from an IBgpPeers.
        /// </summary>
        /// <param name="bgpPeers">The source to copy.</param>
        public BgpPeersStoreOnlyContainer(IBgpPeers bgpPeers)
        {
            if (bgpPeers is null)
            {
                throw new ArgumentNullException(nameof(bgpPeers), "bgpPeers is null when copy-construcing a StoreOnly container");
            }

            this.detailsBacking = bgpPeers.Details.Select(p => new BgpPeerStoreOnlyContainer(p) as IBgpPeer).ToList();
            this.DeviceAddress = bgpPeers.DeviceAddress;
            this.DeviceModel = bgpPeers.DeviceModel;
            this.QueryDuration = bgpPeers.QueryDuration;
        }

        /// <inheritdoc />
        public IReadOnlyList<IBgpPeer> Details => this.detailsBacking;

        /// <inheritdoc />
        public IpAddress DeviceAddress { get; }

        /// <inheritdoc />
        public string DeviceModel { get; }

        /// <inheritdoc />
        public TimeSpan QueryDuration { get; }

        /// <inheritdoc />
        public void ForceEvaluateAll()
        {
            // NOP here
        }

        /// <inheritdoc />
        public IEnumerator<IBgpPeer> GetEnumerator()
        {
            return this.detailsBacking.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.detailsBacking.GetEnumerator();
        }
    }
}