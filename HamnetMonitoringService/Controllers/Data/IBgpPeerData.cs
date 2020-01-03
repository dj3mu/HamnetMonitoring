using System;

namespace RestService.Model
{
    /// <summary>
    /// Interface to a single BGP peer's data.
    /// </summary>
    public interface IBgpPeerData
    {
        /// <summary>
        /// Gets the row ID (key).
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets or sets the IP address of the remote (i.e. the other side) of the BGP peering.
        /// </summary>
        string RemoteAddress { get; }

        /// <summary>
        /// Gets or sets the IP address of the local (i.e. the queried side) of the BGP peering.
        /// </summary>
        string LocalAddress { get; }

        /// <summary>
        /// Gets or sets the callsign of the local (i.e. the queried side) of the BGP peering.
        /// </summary>
        string LocalCallsign { get; }

        /// <summary>
        /// Gets or sets the name of the peering as assigned by Sysop.
        /// </summary>
        string PeeringName { get; }

        /// <summary>
        /// Gets or sets the uptime of the BGP peering.
        /// </summary>
        string Uptime { get; }

        /// <summary>
        /// Gets or sets the uptime of the BGP peering as TimeSpan object.
        /// </summary>
        TimeSpan UptimeSeconds { get; }

        /// <summary>
        /// Gets or sets the number of prefixes routed to the remote side.
        /// </summary>
        long PrefixCount { get; }

        /// <summary>
        /// Gets or sets the state of the peering.
        /// </summary>
        string PeeringState { get; }

        /// <summary>
        /// Gets or sets the unix time stamp.
        /// </summary>
        ulong UnixTimeStamp { get; }

        /// <summary>
        /// Gets or sets the time stamp as human-readable string.
        /// </summary>
        string TimeStampString { get; }
    }
}
