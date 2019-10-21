namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Interface to the data of a single traceroute hop.
    /// </summary>
    public interface ITracerouteWebHop
    {
        /// <summary>
        /// Gets the address of this hop's router.
        /// </summary>
        string Address { get; }

        /// <summary>
        /// Gets the packet loss to this hop.
        /// </summary>
        double LossPercent { get; }

        /// <summary>
        /// Gets the number of packets sent to this hop.
        /// </summary>
        int SentCount { get; }

        /// <summary>
        /// Gets the hop's status.
        /// </summary>
        string Status { get; }

        /// <summary>
        /// Gets the last measure Round-trip-time in milliseconds.
        /// </summary>
        double LastRttMs { get; }

        /// <summary>
        /// Gets the average measure Round-trip-time in milliseconds.
        /// </summary>
        double AverageRttMs { get; }

        /// <summary>
        /// Gets the best measure Round-trip-time in milliseconds.
        /// </summary>
        double BestRttMs { get; }

        /// <summary>
        /// Gets the worst measure Round-trip-time in milliseconds.
        /// </summary>
        double WorstRttMs { get; }
    }
}