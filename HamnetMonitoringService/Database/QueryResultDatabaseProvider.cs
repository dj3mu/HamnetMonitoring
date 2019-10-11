using System;
using System.IO;
using HamnetDbRest;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RestService.Database
{
    /// <summary>
    /// Provider class for databases.
    /// </summary>
    internal class QueryResultDatabaseProvider
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = Program.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The configuration section name for the result database configuration.
        /// </summary>
        public static readonly string ResultDatabaseSectionName = "ResultDatabase";

        /// <summary>
        /// The configuration key for getting the database type.
        /// </summary>
        public static readonly string DatabaseTypeKey = "DatabaseType";

        /// <summary>
        /// The configuration key for getting the database connection string.
        /// </summary>
        public static readonly string ConnectionStringKey = "ConnectionString";
        
        /// <summary>
        /// Remember whether we've already called Migrate on the DB.
        /// </summary>
        private bool migrateCalled = false;

        /// <summary>
        /// Prevent construction from outside the singleton getter.
        /// </summary>
        private QueryResultDatabaseProvider()
        {
        }

        /// <summary>
        /// Gets the singleton instance of the database provider class.
        /// </summary>
        public static QueryResultDatabaseProvider Instance { get; } = new QueryResultDatabaseProvider();

        /// <summary>
        /// Gets the configuration settings for the result database.
        /// </summary>
        public IConfiguration Configuration { get; private set; } = null;

        /// <summary>
        /// Gets a device database context.
        /// </summary>
        /// <remarks>The context must be disposed off by the caller.</remarks>
        public QueryResultDatabaseContext CreateContext()
        {
            QueryResultDatabaseContext context = new QueryResultDatabaseContext(this.Configuration.GetSection(QueryResultDatabaseProvider.ResultDatabaseSectionName));
            
            if (!this.migrateCalled)
            {
                context.Database.Migrate();
                this.migrateCalled = true;
            }

            return context;
        }

        /// <summary>
        /// Sets the connection according to the given configuration section.
        /// </summary>
        /// <param name="configuration">The configuration section to configure for.</param>
        public void SetConfiguration(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "configuration to set result database from is null");
            }
            
            this.Configuration = configuration;
        }
    }

    /// <summary>
    /// Factory class for the purpose of Entitiy Framework design-time.
    /// </summary>
    internal class QueryResultDatabaseContextFactory : IDesignTimeDbContextFactory<QueryResultDatabaseContext>
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = Program.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <inheritdoc />
        public QueryResultDatabaseContext CreateDbContext(string[] args)
        {
            var configFile = (args != null) && (args.Length > 0) && !string.IsNullOrWhiteSpace(args[0])
                ? args[0] // first command line options is the config file to use
                : "~/hamnetMonitorSettings.json"; // this is the fallback config file - only good for Linux

            configFile = configFile.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            log.Debug($"Using config file '{configFile}'");

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFile, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            
            QueryResultDatabaseProvider.Instance.SetConfiguration(configuration);

            return QueryResultDatabaseProvider.Instance.CreateContext() as QueryResultDatabaseContext;
        }
    }
}
