using System;
using System.Linq;
using HamnetDbRest;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
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
        /// <param name="configuration">The configuration settings.</param>
        public QueryResultDatabaseContext(IConfigurationSection configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "The specified database configuration data is null");
            }

            this.Configuration = configuration;
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
                        var connection = new MySqlConnection(this.ConnectionString);
                        optionsBuilder.UseMySql(connection);
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

            modelBuilder.Entity<MonitoringPerstistence>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
