using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Container for the status reply.
    /// </summary>
    internal class ServerStatusReply : IServerStatusReply
    {
        private readonly Dictionary<string, IDatabasestatistic> dbStats = new Dictionary<string, IDatabasestatistic>();

        private readonly Dictionary<string, IConfigurationInfo> configs = new Dictionary<string, IConfigurationInfo>();

        /// <inheritdoc />
        public string ServerVersion { get; set; }

        /// <inheritdoc />
        public int MaximumSupportedApiVersion { get; set; }

        /// <inheritdoc />
        public DateTime ProcessStartTime { get; set; }

        /// <inheritdoc />
        public TimeSpan ProcessUptime { get; set; }

        /// <inheritdoc />
        public TimeSpan ProcessCpuTime { get; set; }

        /// <inheritdoc />
        public long ProcessWorkingSet { get; set; }

        /// <inheritdoc />
        public long ProcessPrivateSet { get; set; }

        /// <inheritdoc />
        public int ProcessThreads { get; set; }

        /// <summary>
        /// Adds the given database statistics for a database with the given key.
        /// </summary>
        /// <param name="databaseId">The database key.</param>
        /// <param name="dbStats">The statistics associated with the key.</param>
        public void Add(string databaseId, IDatabasestatistic dbStats)
        {
            this.dbStats.Add(databaseId, dbStats);
        }

        /// <summary>
        /// Adds the given database statistics for a database with the given key.
        /// </summary>
        /// <param name="configurationId">The configuration key.</param>
        /// <param name="configurationInfo">The configuration associated with the key.</param>
        public void Add(string configurationId, IConfigurationInfo configurationInfo)
        {
            this.configs.Add(configurationId, configurationInfo);
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IDatabasestatistic> DatabaseStatistic => this.dbStats;

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IConfigurationInfo>  Configurations => this.configs;
    }
}
