using System;
using System.IO;
using System.Linq;
using HamnetDbRest;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RestService.Model;

namespace RestService.Database
{
    /// <summary>
    /// Database context class for the device database storing the results of SNMP query operations.
    /// </summary>
    public class QueryResultDatabaseContext : DbContext
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = Program.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Gets access to the RSSI values table.
        /// </summary>
        public DbSet<Rssi> RssiValues { get; set; }

        /// <summary>
        /// Gets access to the RSSI failing queries table.
        /// </summary>
        public DbSet<RssiFailingQuery> RssiFailingQueries { get; set; }

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
                        LastQueryEnd = DateTime.MinValue,
                        LastQueryStart = DateTime.MinValue
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
        /// <param name="databasePathAndFile">The database file location.</param>
        public QueryResultDatabaseContext(string databasePathAndFile)
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

            this.DatabasePathAndFile = databasePathAndFile;

            log.Info($"Initialized for Result Database '{this.DatabasePathAndFile}'");
        }

        /// <summary>
        /// Gets the path and/or file name of the file that contains the device database.
        /// </summary>
        public string DatabasePathAndFile { get; }

        /// <inheritdoc />
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }
            
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = this.DatabasePathAndFile };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);

            optionsBuilder.UseSqlite(connection);
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rssi>()
                .HasIndex(r => r.ForeignId).IsUnique();

            //// Conversions of Minimum Device Version to SemanticVersion
            //modelBuilder
            //    .Entity<Rssi>()
            //    .Property(e => e.ForeignId)
            //    .HasConversion(
            //        v => v.ToString(),
            //        v => IPNetwork.Parse(v));
        }
    }
}
