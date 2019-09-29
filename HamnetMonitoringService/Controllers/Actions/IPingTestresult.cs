using System;
using System.Net;
using System.Net.NetworkInformation;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Result of a ping test.
    /// </summary>
    public interface IPingTestResult : IStatusReply
    {
        /// <summary>
        /// The IP address that has been pinged.
        /// </summary>
        string Address { get; }

        /// <summary>
        /// Gets the status result of the ping.
        /// </summary>
        string Status { get; }

        /// <summary>
        /// The round-trip time of the ping.
        /// </summary>
        TimeSpan RoundtripTime { get; }

        /// <summary>
        /// The time-to-live value that has been used with the ping.
        /// </summary>
        int? TimeToLive { get; }

        /// <summary>
        /// The &quot;don't fragment&quot; setting used with the ping.
        /// </summary>
        bool? DontFragment { get; }

        /// <summary>
        /// The buffer size (amount of data) used with the ping.
        /// </summary>
        int? BufferSize { get; }
    }
}