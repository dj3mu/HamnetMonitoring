using System;
using System.Collections.Generic;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Interface to the information returned by the status REST command.
    /// </summary>
    public interface IServerStatusReply
    {
        /// <summary>
        /// Gets the highes supported API version of this server.
        /// </summary>
        int MaximumSupportedApiVersion { get; }

        /// <summary>
        /// Gets the server version information.
        /// </summary>
        string ServerVersion { get; }

        /// <summary>
        /// Gets the uptime of the current server process.
        /// </summary>
        TimeSpan ProcessUptime { get; }

        /// <summary>
        /// Gets the database statistics
        /// </summary>
        IReadOnlyDictionary<string, IDatabasestatistic> DatabaseStatistic { get; }
    }

    /// <summary>
    /// Interface to a single database statistic.
    /// </summary>
    public interface IDatabasestatistic : IReadOnlyDictionary<string, string>
    {
    }
}
