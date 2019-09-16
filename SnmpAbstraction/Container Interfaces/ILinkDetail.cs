using System;
using System.Net;

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
        string MacString1 { get; }

        /// <summary>
        /// Gets the MAC address of the remote side (as Hex string like &quot;b8:27:eb:97:b6:39&quot;).
        /// </summary>
        string MacString2 { get; }

        /// <summary>
        /// Gets the IP address of the local side.
        /// </summary>
        IPAddress Address1 { get; }

        /// <summary>
        /// Gets the IP address of the remote side.
        /// </summary>
        IPAddress Address2 { get; }

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

        /// <summary>
        /// Gets the number of the side (i.e. 1 or 2) that is the access point of this connection.<br/>
        /// Null if side of access point cannot be determined.
        /// </summary>
        int? SideOfAccessPoint { get; }
    }
}
