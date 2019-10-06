using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Database context class for the device database providing OIDs for abstract data retrieval.
    /// </summary>
    internal class DeviceDatabaseContext : DbContext
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The version to use for fallback for MinimumVersion fields.
        /// </summary>
        private static readonly SemanticVersion FallBackMinimumVersion = new SemanticVersion(0, 0, 0, string.Empty, string.Empty);

        /// <summary>
        /// The version to use for fallback for MaximumVersion fields.
        /// </summary>
        private static readonly SemanticVersion FallBackMaximumVersion = new SemanticVersion(int.MaxValue, int.MaxValue, int.MaxValue, string.Empty, string.Empty);

        /// <summary>
        /// Gets access to the retrievable values table.
        /// </summary>
        public DbSet<RetrievableValue> RetrievalValues { get; set; }

        /// <summary>
        /// Gets access to the data types table.
        /// </summary>
        public DbSet<DataType> DataTypes { get; set; }

        /// <summary>
        /// Gets access to the device vendors table.
        /// </summary>
        public DbSet<DeviceVendor> Vendors { get; set; }

        /// <summary>
        /// Gets access to the devices table.
        /// </summary>
        public DbSet<Device> Devices { get; set; }

        /// <summary>
        /// Gets access to the schema info table.
        /// </summary>
        public DbSet<SchemaInfo> SchemaInfo { get; set; }

        /// <summary>
        /// Gets access to the device specific OIDs (which is the reason why we do all this).
        /// </summary>
        public DbSet<DeviceSpecificOid> DeviceSpecificOids { get; set; }

        /// <summary>
        /// Gets access to the device version to OID mapping table (the m:n relation between DeviceVersion and DeviceSpecificOid)
        /// </summary>
        public DbSet<DeviceVersionMapping> DeviceVersionMapping { get; set; }

        /// <summary>
        /// Gets access to the device versions table
        /// </summary>
        public DbSet<DeviceVersion> DeviceVersions { get; set; }

        /// <summary>
        /// Construct for a specific database file location.
        /// </summary>
        /// <param name="configurationSection">The configuration section.</param>
        public DeviceDatabaseContext(IConfigurationSection configurationSection)
        {
            if (configurationSection == null)
            {
                SqliteConnectionStringBuilder connStringBuilder = new SqliteConnectionStringBuilder();

                var databaseDefaultPath = Path.Combine(Environment.CurrentDirectory, "Config/DeviceDatabase.sqlite");
                connStringBuilder.DataSource = databaseDefaultPath;
                
                this.ConnectionString = connStringBuilder.ToString();
            }
            else
            {
                if (configurationSection.GetValue<string>(CacheDatabaseProvider.DatabaseTypeKey).ToUpperInvariant() != "SQLITE")
                {
                    throw new InvalidOperationException("Only SQLite is currently supported for the device database");
                }

                this.ConnectionString = configurationSection.GetValue<string>(CacheDatabaseProvider.ConnectionStringKey);
            }
        }

       /// <summary>
        /// Construct for a specific database file location. Only intended for Unit Tests !
        /// </summary>
        /// <param name="databasePathAndFile">The database file location.</param>
        internal DeviceDatabaseContext(string databasePathAndFile)
        {
            if (string.IsNullOrWhiteSpace(databasePathAndFile))
            {
                throw new ArgumentNullException(nameof(databasePathAndFile), "The specified database is null, empty or white-space-only");
            }
            
            SqliteConnectionStringBuilder connStringBuilder = new SqliteConnectionStringBuilder();

            var databaseDefaultPath = Path.Combine(Environment.CurrentDirectory, databasePathAndFile);
            connStringBuilder.DataSource = databaseDefaultPath;
            
            this.ConnectionString = connStringBuilder.ToString();
        }

        public string ConnectionString { get; }

        /// <inheritdoc />
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connection = new SqliteConnection(this.ConnectionString);

            optionsBuilder.UseSqlite(connection);
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Conversions of Minimum Device Version to SemanticVersion
            modelBuilder
                .Entity<DeviceVersion>()
                .Property(e => e.HigherOrEqualVersion)
                .HasConversion(
                    v => v.ToString(),
                    v => v.ParseAsSemanticVersionWithFallback(FallBackMinimumVersion));

            // Conversions of Maximum Device Version to SemanticVersion
            modelBuilder
                .Entity<DeviceVersion>()
                .Property(e => e.LowerThanVersion)
                .HasConversion(
                    v => v.ToString(),
                    v => v.ParseAsSemanticVersionWithFallback(FallBackMaximumVersion));

            // Conversions of OID string to Oid object
            modelBuilder
                .Entity<DeviceSpecificOid>()
                .Property(e => e.Oid)
                .HasConversion(
                    v => v.ToString(),
                    v => new Oid(v));
        }
    }
}
