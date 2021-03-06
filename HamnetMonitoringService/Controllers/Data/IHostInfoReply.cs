using System;
using System.Collections.Generic;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Interface to the host info reply of a single host.
    /// </summary>
    public interface IHostInfoReply : IStatusReply
    {
        /// <summary>
        /// Gets the IP address of that this host info is valid for.
        /// </summary>
        string Address { get; }

        /// <summary>
        /// Gets the system's description or null if not provided.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the system's admin name or null if not available.
        /// </summary>
        string Contact { get; }

        /// <summary>
        /// Gets the system's location or null if not available.
        /// </summary>
        string Location { get; }

        /// <summary>
        /// Gets the system's name or null if not available.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the system's uptime (at the time of the query that filled the container).
        /// </summary>
        TimeSpan? Uptime { get; }

        /// <summary>
        /// Gets the device model name.
        /// </summary>
        /// <value></value>
        string Model { get; }

        /// <summary>
        /// Gets the device's software version.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Gets the device's maximum supported SNMP version.
        /// </summary>
        string MaximumSnmpVersion { get; }

        /// <summary>
        /// Gets the list of supported features.
        /// </summary>
        IEnumerable<string> SupportedFeatures { get; }

        /// <summary>
        /// Gets the API that is, by default, use for talking to this device.
        /// </summary>
        string DefaultApi { get; }

        /// <summary>
        /// Gets the date and time when this data has last been updated.
        /// </summary>
        DateTime? LastDataUpdate { get; }

        /// <summary>
        /// Gets the date and time when this data has last been updated as Unix timestamp.
        /// </summary>
        ulong? UnixTimeStamp { get; }
    }
}
