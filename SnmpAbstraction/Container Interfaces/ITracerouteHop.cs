using System.Net;

namespace SnmpAbstraction
{
    /// <summary>
    /// The hop status as enum.
    /// </summary>
    public enum HopStatus
    {
        /// <summary>
        /// All ok with the hop
        /// </summary>
        Ok,

        /// <summary>
        /// The hop timed out
        /// </summary>
        Timeout,
        
        /// <summary>
        /// The hop's packet has been lost
        /// </summary>
        Lost,
        
        /// <summary>
        /// The hop contains replies but also losses of packets
        /// </summary>
        Unstable,
        
        /// <summary>
        /// The hop includes a statement that the address is unreachable
        /// </summary>
        Unreachable
    }

    /// <summary>
    /// Interface to the data of a single traceroute hop.
    /// </summary>
    public interface ITracerouteHop
    {
        /// <summary>
        /// Gets the address of this hop's router.
        /// </summary>
        IPAddress Address { get; }

        /// <summary>
        /// Gets the packet loss to this hop.
        /// </summary>
        double LossPercent { get; }

        /// <summary>
        /// Gets the number of packets sent to this hop.
        /// </summary>
        int SentCount { get; }

        /// <summary>
        /// Gets some info about the hop (a more verbose status meant for human reading)
        /// </summary>
        string Info { get; }

        /// <summary>
        /// Gets the hop's status as enum.
        /// </summary>
        HopStatus Status { get; }

        /// <summary>
        /// Gets the last measure Round-trip-time in milliseconds.
        /// </summary>
        double LastRtt { get; }

        /// <summary>
        /// Gets the average measure Round-trip-time in milliseconds.
        /// </summary>
        double AverageRtt { get; }

        /// <summary>
        /// Gets the best measure Round-trip-time in milliseconds.
        /// </summary>
        double BestRtt { get; }

        /// <summary>
        /// Gets the worst measure Round-trip-time in milliseconds.
        /// </summary>
        double WorstRtt { get; }
    }
}