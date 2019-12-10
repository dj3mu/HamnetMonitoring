using System;
using System.Net;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the details of a single link.
    /// </summary>
    public interface ILinkDetail : IHamnetSnmpQuerierResult, ILazyEvaluated
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
        /// Gets the model and version name of the local side.
        /// </summary>
        string ModelAndVersion1 { get; }

        /// <summary>
        /// Gets the model and version name of the remote side.
        /// </summary>
        string ModelAndVersion2 { get; }

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

        /// <summary>
        /// Gets the client connection quality value if the side which delivers it. Perferring side #1 if both sides provide the value.<br/>
        /// Null if CCQ could not be obtained from an of the devices.
        /// </summary>
        double? Ccq { get; }

        /// <summary>
        /// Gets the client connection quality value reported by side #1.<br/>
        /// Null if CCQ could not be obtained from the device on side #1.
        /// </summary>
        double? Ccq1 { get; }

        /// <summary>
        /// Gets the client connection quality value reported by side #2.<br/>
        /// Null if CCQ could not be obtained from the device on side #2.
        /// </summary>
        double? Ccq2 { get; }
    }
}
