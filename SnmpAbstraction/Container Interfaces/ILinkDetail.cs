using System;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the details of a single link.
    /// </summary>
    public interface ILinkDetail : IHamnetSnmpQuerierResult
    {
        /// <summary>
        /// Gets the MAC address of the local side (as Hex string like &quot;b8:27:eb:97:b6:39&quot;).
        /// </summary>
        /// <value></value>
        string MacString1 { get; }

        /// <summary>
        /// Gets the MAC address of the remote side (as Hex string like &quot;b8:27:eb:97:b6:39&quot;).
        /// </summary>
        /// <value></value>
        string MacString2 { get; }

        /// <summary>
        /// Gets the RX level which the side having <see cref="MacString1" /> produces at side having <see cref="MacString2" />.
        /// </summary>
        double RxLevel1at2 { get; }

        /// <summary>
        /// Gets the RX level which the side having <see cref="MacString2" /> produces at side having <see cref="MacString1" />.
        /// </summary>
        double RxLevel2at1 { get; }

        /// <summary>
        /// Gets the uptime of the link.
        /// </summary>
        TimeSpan LinkUptime { get; }
    }
}
