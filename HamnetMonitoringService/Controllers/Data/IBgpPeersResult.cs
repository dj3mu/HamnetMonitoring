using System.Collections.Generic;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Interface to the data of a BGP peers query.
    /// </summary>
    internal interface IBgpPeersResult : IStatusReply
    {
        /// <summary>
        /// Gets the list of BGP peers.
        /// </summary>
        IReadOnlyList<IBgpPeerResult> BgpPeers { get; }
    }

    /// <summary>
    /// Interface to the data of a single BGP peer.
    /// </summary>
    internal interface IBgpPeerResult
    {
        /// <summary>
        /// Gets the IP address of the remote (i.e. the other side) of the BGP peering.
        /// </summary>
        string RemoteAddress { get; }

        /// <summary>
        /// Gets the name of the peering as assigned by Sysop.
        /// </summary>
        string PeeringName { get; }

        /// <summary>
        /// Gets the IP address of the local (i.e. the queried side) of the BGP peering.
        /// </summary>
        string LocalAddress { get; }

        /// <summary>
        /// Gets the uptime of the BGP peering.
        /// </summary>
        string Uptime { get; }

        /// <summary>
        /// Gets the number of prefixes routed to the remote side.
        /// </summary>
        long PrefixCount { get; }

        /// <summary>
        /// Gets the state of the peering.
        /// </summary>
        string PeeringState { get; }
    }
}