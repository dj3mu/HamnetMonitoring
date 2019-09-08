using System;

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
        int? InterfaceId { get; }

        /// <summary>
        /// Get the strength of our signal at the remote side (as reported by the remote side) in dBm.
        /// </summary>
        double TxSignalStrength { get; }

        /// <summary>
        /// Get the strength (combined of all streams) of the remote's signal at our side in dBm.
        /// </summary>
        double RxSignalStrength { get; }

        /// <summary>
        /// Gets the uptime of the link.
        /// </summary>
        TimeSpan LinkUptime { get; }

        /// <summary>
        /// Gets a value indicating whether this is peer is in Access Point mode.
        /// </summary>
        bool? IsAccessPoint { get; }
    }
}
