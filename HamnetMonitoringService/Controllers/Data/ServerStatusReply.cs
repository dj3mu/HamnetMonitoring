using System;
using System.Collections.Generic;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Container for the status reply.
    /// </summary>
    internal class ServerStatusReply : IServerStatusReply
    {
        private readonly Dictionary<string, IDatabasestatistic> dbStats = new Dictionary<string, IDatabasestatistic>();

        /// <inheritdoc />
        public string ServerVersion { get; set; }

        /// <inheritdoc />
        public TimeSpan ProcessUptime { get; set; }

        /// <inheritdoc />
        public int MaximumSupportedApiVersion { get; set; }

        /// <summary>
        /// Adds the given database statistics for a database with the given key.
        /// </summary>
        /// <param name="databaseId">The database key.</param>
        /// <param name="dbStats">The statistics associated with the key.</param>
        public void Add(string databaseId, IDatabasestatistic dbStats)
        {
            this.dbStats.Add(databaseId, dbStats);
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IDatabasestatistic> DatabaseStatistic => this.dbStats;
    }
}
