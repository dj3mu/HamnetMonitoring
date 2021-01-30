using System;
using System.IO;
using System.Net;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Database context class for the device database providing OIDs for abstract data retrieval.
    /// </summary>
    internal class CacheDatabaseContext : DbContext
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                new OidJsonConverter(),
                new IpAddressJsonConverter(),
                new SemanticVersionJsonConverter(),

                // creation-only converters
                new CachableOidJsonCreationConverter(),
                new WirelessPeerInfoJsonCreationConverter(),
                new WirelessPeerInfosJsonCreationConverter(),
                new InterfaceDetailsJsonCreationConverter(),
                new InterfaceDetailJsonCreationConverter()
            }
        };

        /// <summary>
        /// Gets access to the cache data table.
        /// </summary>
        public DbSet<CacheData> CacheData { get; set; }

        /// <summary>
        /// Construct for a specific configuration.
        /// </summary>
        /// <param name="configuration">The database configuration section.</param>
        public CacheDatabaseContext(IConfigurationSection configuration)
        {
            if (configuration == null)
            {
                SqliteConnectionStringBuilder connStringBuilder = new SqliteConnectionStringBuilder();

                var databaseDefaultPath = Path.Combine(Environment.CurrentDirectory, "Config/CacheDatabase.sqlite");
                connStringBuilder.DataSource = databaseDefaultPath;

                this.DatabaseType = "SQLITE";
                this.ConnectionString = connStringBuilder.ToString();
            }
            else
            {
                this.DatabaseType = configuration.GetValue<string>(CacheDatabaseProvider.DatabaseTypeKey).ToUpperInvariant();
                this.ConnectionString = configuration.GetValue<string>(CacheDatabaseProvider.ConnectionStringKey);
            }
        }

        /// <summary>
        /// Construct for a specific database file location. Only intended for unit testing.
        /// </summary>
        /// <param name="databaseFilePath">The database file path.</param>
        internal CacheDatabaseContext(string databaseFilePath)
        {
            if (string.IsNullOrWhiteSpace(databaseFilePath))
            {
                throw new InvalidOperationException("Only SQLite is currently supported for the cache database");
            }

            SqliteConnectionStringBuilder connStringBuilder = new SqliteConnectionStringBuilder();

            connStringBuilder.DataSource = databaseFilePath;

            this.ConnectionString = connStringBuilder.ToString();
        }

        /// <summary>
        /// Construct from DbContextOptions.
        /// </summary>
        /// <param name="options">The options to construct from.</param>
        public CacheDatabaseContext(DbContextOptions<CacheDatabaseContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets a string reflecting the type of the database (from configuration).
        /// </summary>
        public string DatabaseType { get; }

        /// <summary>
        /// Gets the path and/or file name of the file that contains the device database.
        /// </summary>
        public string ConnectionString { get; }

        /// <inheritdoc />
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            switch(this.DatabaseType)
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
                    throw new InvalidOperationException($"A database of type '{this.DatabaseType}' is currently not supported for the cache database");
            }
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<CacheData>()
                .HasIndex(p => new { p.Address }).IsUnique();

            modelBuilder
                .Entity<CacheData>()
                .Property(e => e.Address)
                .HasConversion(
                    ipaddressObject => ipaddressObject.ToString(),
                    addressString => new IpAddress(IPAddress.Parse(addressString)));

            modelBuilder
                .Entity<CacheData>()
                .Property(e => e.SystemData)
                .HasConversion(
                    systemDataObject => JsonConvert.SerializeObject(systemDataObject, SerializerSettings),
                    systemDataString => JsonConvert.DeserializeObject<SerializableSystemData>(systemDataString, SerializerSettings));

            modelBuilder
                .Entity<CacheData>()
                .Property(e => e.WirelessPeerInfos)
                .HasConversion(
                    wirelessPeerInfoObject => JsonConvert.SerializeObject(wirelessPeerInfoObject, SerializerSettings),
                    wirelessPeerInfoString => JsonConvert.DeserializeObject<IWirelessPeerInfos>(wirelessPeerInfoString, SerializerSettings));

            modelBuilder
                .Entity<CacheData>()
                .Property(e => e.InterfaceDetails)
                .HasConversion(
                    interfaceDetailsObject => JsonConvert.SerializeObject(interfaceDetailsObject, SerializerSettings),
                    interfaceDetailsString => JsonConvert.DeserializeObject<IInterfaceDetails>(interfaceDetailsString, SerializerSettings));
        }
    }
}
