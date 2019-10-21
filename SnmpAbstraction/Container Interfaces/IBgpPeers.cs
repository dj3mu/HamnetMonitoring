using System.Collections.Generic;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the list of the BGP peers of a device.
    /// </summary>
    public interface IBgpPeers : IHamnetSnmpQuerierResult, ILazyEvaluated, IEnumerable<IBgpPeer>
    {
        /// <summary>
        /// Gets the list of BGP peer info.
        /// </summary>
        IReadOnlyList<IBgpPeer> Details { get; }
    }
}
