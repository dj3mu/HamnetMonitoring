using System;
using System.Linq;
using HamnetDbRest;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Newtonsoft.Json;
using RestService.DataFetchingService;
using RestService.Model;
using SnmpAbstraction;

namespace RestService.Database
{
    /// <summary>
    /// Database context class for the device database storing the results of SNMP query operations.
    /// </summary>
    public class QueryResultDatabaseContext : DbContext
    {
        /// <summary>
        /// JSON serialization / deserialization settings for all data.
        /// </summary>
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            CheckAdditionalContent = false,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateParseHandling = DateParseHandling.DateTimeOffset,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DefaultValueHandling = DefaultValueHandling.Include,
            FloatFormatHandling = FloatFormatHandling.String,
            FloatParseHandling = FloatParseHandling.Double,
#if DEBUG
            Formatting = Formatting.Indented,
#else
            Formatting = Formatting.None,
#endif
            MissingMemberHandling = MissingMemberHandling.Error,
            NullValueHandling = NullValueHandling.Include,
            ObjectCreationHandling = ObjectCreationHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            StringEscapeHandling = StringEscapeHandling.Default,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            TypeNameHandling = TypeNameHandling.None,

            Converters = new JsonConverter[]
            {
                // full converters
                new IpAddressJsonConverter(),
                new SemanticVersionJsonConverter()
            }
        };

#pragma warning disable IDE0052 // for future use
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = Program.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#pragma warning restore

        /// <summary>
        /// Gets access to the RSSI values table.
        /// </summary>
        public DbSet<Rssi> RssiValues { get; set; }

        /// <summary>
        /// Gets access to the RSSI values table.
        /// </summary>
        public DbSet<BgpPeerData> BgpPeers { get; set; }

        /// <summary>
        /// Gets access to the RSSI failing queries table.
        /// </summary>
        public DbSet<RssiFailingQuery> RssiFailingQueries { get; set; }

        /// <summary>
        /// Gets access to the BGP failing queries table.
        /// </summary>
        public DbSet<BgpFailingQuery> BgpFailingQueries { get; set; }

        /// <summary>
        /// Gets access to the monitoring service persistence data.
        /// </summary>
        public DbSet<MonitoringPerstistence> MonitoringStatus { get; set; }

        /// <summary>
        /// Gets the one and only status dataset (a single row in the MonitoringStatus dataset).
        /// </summary>
        public MonitoringPerstistence Status
        {
            get
            {
                var status = this.MonitoringStatus.FirstOrDefault();
                if (status == null)
                {
                    status = new MonitoringPerstistence
                    {
                        LastMaintenanceEnd = DateTime.MinValue,
                        LastMaintenanceStart = DateTime.MinValue,
                        LastRssiQueryEnd = DateTime.MinValue,
                        LastRssiQueryStart = DateTime.MinValue
                    };

                    this.MonitoringStatus.Add(status);
                    this.SaveChanges();
                }

                return status;
            }
        }

        /// <summary>
        /// Construct from DbContextOptions.
        /// </summary>
        /// <param name="options">The options to construct from.</param>
        public QueryResultDatabaseContext(DbContextOptions<QueryResultDatabaseContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Construct for a specific database file location.
        /// </summary>
        /// <param name="configuration">The configuration settings.</param>
        public QueryResultDatabaseContext(IConfigurationSection configuration)
        {
            this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "The specified database configuration data is null");
            this.ConnectionString = configuration.GetValue<string>(QueryResultDatabaseProvider.ConnectionStringKey);
        }

        /// <summary>
        /// Gets the configuration section for the database.
        /// </summary>
        public IConfigurationSection Configuration { get; }

        /// <summary>
        /// Gets the connection string of this database context.
        /// May contain sensitive data !
        /// </summary>
        protected string ConnectionString { get; private set; }

        /// <inheritdoc />
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            var databaseType = this.Configuration.GetValue<string>(QueryResultDatabaseProvider.DatabaseTypeKey)?.ToUpperInvariant();

            switch(databaseType)
            {
                case "SQLITE":
                    {
                        var connection = new SqliteConnection(this.ConnectionString);
                        optionsBuilder.UseSqlite(connection);
                    }

                    break;

                case "MYSQL":
                    {
                        optionsBuilder.UseMySql(this.ConnectionString, ServerVersion.AutoDetect(this.ConnectionString));
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException($"The configured database type '{databaseType}' is not supported for the query result database");
            }

        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rssi>()
                .HasIndex(r => r.ForeignId).IsUnique();

            modelBuilder.Entity<RssiFailingQuery>()
                .Property(e => e.AffectedHosts)
                .HasConversion(
                    affectedHostsObject => string.Join(',', affectedHostsObject),
                    affectedHostsString => affectedHostsString.Split(',', StringSplitOptions.RemoveEmptyEntries));

            modelBuilder.Entity<RssiFailingQuery>()
                .Property(e => e.PenaltyInfo)
                .HasConversion(
                    penaltyInfoObject => JsonConvert.SerializeObject(penaltyInfoObject, SerializerSettings),
                    penaltyInfoString => JsonConvert.DeserializeObject<SingleFailureInfoContainer>(penaltyInfoString, SerializerSettings));

            modelBuilder.Entity<MonitoringPerstistence>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<BgpPeerData>()
                .HasIndex(p => new { p.RemoteAddress, p.LocalAddress }).IsUnique();

            modelBuilder.Entity<BgpFailingQuery>()
                .Property(e => e.PenaltyInfo)
                .HasConversion(
                    penaltyInfoObject => JsonConvert.SerializeObject(penaltyInfoObject, SerializerSettings),
                    penaltyInfoString => JsonConvert.DeserializeObject<SingleFailureInfoContainer>(penaltyInfoString, SerializerSettings));
        }

        /// <summary>
        /// Simple container for deserialization of an <see cref="ISingleFailureInfo" />.
        /// </summary>
        private class SingleFailureInfoContainer : ISingleFailureInfo
        {
            /// <inheritdoc />
            [JsonProperty(Order = 1)]
            public uint OccuranceCount { get; set; }

            /// <inheritdoc />
            [JsonProperty(Order = 3)]
            public DateTime LastOccurance { get; set; }

            /// <inheritdoc />
            [JsonProperty(Order = 2)]
            public DateTime FirsOccurance { get; set; }

            /// <inheritdoc />
            [JsonProperty(Order = 4)]
            public TimeSpan CurrentPenalty { get; set; }

            /// <inheritdoc />
            [JsonProperty(Order = 5)]
            public bool IsRetryFeasible
            {
                get
                {
                    var retryFeasible = ((DateTime.UtcNow - this.LastOccurance) > this.CurrentPenalty);
                    return retryFeasible;
                }

                set
                {
                    // NOP - we ignore the stored value and always recompute in getter
                }
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return $"occurance count = {this.OccuranceCount}, retry interval = {this.CurrentPenalty}, first occurance = {this.FirsOccurance}, last occurance = {this.LastOccurance}";
            }
        }
    }
}
