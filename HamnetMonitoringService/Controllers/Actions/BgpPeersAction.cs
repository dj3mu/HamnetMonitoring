using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SnmpAbstraction;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Class doing a ping to a host
    /// </summary>
    internal class BgpPeersAction
    {
        private string host;

        private string remotePeerAddress;
        
        private readonly FromUrlQueryQuerierOptions querierOptions;

        /// <summary>
        /// Construct for a specific host.
        /// </summary>
        /// <param name="host">The host or IP address to ping.</param>
        /// <param name="remotePeerAddress">The address of the remote peer to get the data for. If null or empty, all peer's data.</param>
        /// <param name="querierOptions">The querier options to use.</param>
        public BgpPeersAction(string host, string remotePeerAddress, FromUrlQueryQuerierOptions querierOptions)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentNullException(nameof(host), "Host is null, empty or white-space-only");
            }

            this.host = host;
            this.remotePeerAddress = remotePeerAddress;
            this.querierOptions = querierOptions ?? new FromUrlQueryQuerierOptions();

            this.querierOptions.AllowedApis = QueryApis.VendorSpecific; // currently we only support vendor-specific API - no matter what user requests
        }

        /// <summary>
        /// Asynchronuously executes the ping task.
        /// </summary>
        /// <returns></returns>
        internal async Task<ActionResult<IStatusReply>> Execute()
        {
            return await Task.Run(this.DoGetBgpInfo);
        }

        private ActionResult<IStatusReply> DoGetBgpInfo()
        {
            try
            {
                using(var querier = SnmpQuerierFactory.Instance.Create(this.host, this.querierOptions))
                {
                    IBgpPeers bgpPeers = querier.FetchBgpPeers(this.remotePeerAddress);

                    return new BgpPeersResult(bgpPeers);
                }
            }
            catch(Exception ex)
            {
                return new ErrorReply(ex);
            }
        }

        /// <summary>
        /// Container for a list of BGP peer results.
        /// </summary>
        private class BgpPeersResult : IBgpPeersResult
        {
            List<IBgpPeerResult> peerResultsBacking = null;

            /// <summary>
            /// Construct from the querier interface container.
            /// </summary>
            /// <param name="bgpPeers">The peers to construct from.</param>
            public BgpPeersResult(IBgpPeers bgpPeers)
            {
                this.peerResultsBacking = (bgpPeers == null) ? new List<IBgpPeerResult>() : bgpPeers.Details.Select(p => new BgpPeerResult(p) as IBgpPeerResult).ToList();
            }

            /// <inheritdoc />
            public IReadOnlyList<IBgpPeerResult> BgpPeers => this.peerResultsBacking;

            /// <inheritdoc />
            public IEnumerable<string> ErrorDetails { get; } = Enumerable.Empty<string>();
        }

        /// <summary>
        /// Container for a single BGP peer result.
        /// </summary>
        private class BgpPeerResult : IBgpPeerResult
        {
            /// <summary>
            /// Construct from a querier BGP peer.
            /// </summary>
            /// <param name="peer">The querier peer data to construct from.</param>
            public BgpPeerResult(IBgpPeer peer)
            {
                this.LocalAddress = peer.LocalAddress?.ToString();
                this.RemoteAddress = peer.RemoteAddress?.ToString();
                this.PeeringName = peer.Name;
                this.Uptime = peer.Uptime.ToString();
                this.PrefixCount = peer.PrefixCount;
                this.PeeringState = peer.State;
            }

            /// <inheritdoc />
            public string RemoteAddress { get; }

            /// <inheritdoc />
            public string PeeringName { get; }

            /// <inheritdoc />
            public string LocalAddress { get; }

            /// <inheritdoc />
            public string Uptime { get; }

            /// <inheritdoc />
            public long PrefixCount { get; }

            /// <inheritdoc />
            public string PeeringState { get; }
        }
    }
}
