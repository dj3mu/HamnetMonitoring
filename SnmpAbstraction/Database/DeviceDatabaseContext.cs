using System;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SnmpAbstraction
{
    /// <summary>
    /// Database context class for the device database providing OIDs for abstract data retrieval.
    /// </summary>
    internal class DeviceDatabaseContext : DbContext
    {
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
        /// Construct for a specific database file location.
        /// </summary>
        /// <param name="databasePathAndFile">The database file location.</param>
        public DeviceDatabaseContext(string databasePathAndFile)
        {
            if (string.IsNullOrWhiteSpace(databasePathAndFile))
            {
                throw new ArgumentNullException(nameof(databasePathAndFile), "The specified database is null, empty or white-space-only");
            }
            
            this.DatabasePathAndFile = databasePathAndFile;
            if (!Path.IsPathRooted(this.DatabasePathAndFile))
            {
                this.DatabasePathAndFile = Path.Combine(Environment.CurrentDirectory, this.DatabasePathAndFile);
            }

            if (!File.Exists(this.DatabasePathAndFile))
            {
                throw new FileNotFoundException($"Cannot find OID database file '{this.DatabasePathAndFile}'", databasePathAndFile);
            }

            this.DatabasePathAndFile = databasePathAndFile;
        }

        /// <summary>
        /// Gets the path and/or file name of the file that contains the device database.
        /// </summary>
        public string DatabasePathAndFile { get; }

        /// <inheritdoc />
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = this.DatabasePathAndFile };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            optionsBuilder.UseSqlite(connection);
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>()
                .HasOne(d => d.Vendor)
                .WithMany(v => v.Devices)
                .HasForeignKey(d => d.VendorId);
        }
    }
}
