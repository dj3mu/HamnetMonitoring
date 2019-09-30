using System;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Container for the status reply.
    /// </summary>
    internal class ServerStatusReply : IServerStatusReply
    {
        /// <inheritdoc />
        public string ServerVersion { get; set; }

        /// <inheritdoc />
        public TimeSpan ProcessUptime { get; set; }

        /// <inheritdoc />
        public int MaximumSupportedApiVersion { get; set; }
    }
}
