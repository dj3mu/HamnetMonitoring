namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the details of a single wireless peer.
    /// </summary>
    public interface IWirelessPeerInfo : IHamnetSnmpQuerierResult, ILazyEvaluated
    {
        /// <summary>
        /// Gets the MAC address of the remote side (as Hex string like &quot;b8:27:eb:97:b6:39&quot;).
        /// </summary>
        /// <value></value>
        string RemoteMacString { get; }

        /// <summary>
        /// Gets the numeric ID of the interface that this peer is connected to.
        /// </summary>
        int InterfaceId { get; }
    }
}
