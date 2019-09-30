using System;

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
    }
}
