using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SnmpAbstraction
{
    /// <summary>
    /// Provider class for databases.
    /// </summary>
    internal class CacheDatabaseProvider
    {
        /// <summary>
        /// The configuration section name for the cache database configuration.
        /// </summary>
        public static readonly string CacheDatabaseSectionName = "CacheDatabase";

        /// <summary>
        /// The configuration key for getting the database type.
        /// </summary>
        public static readonly string DatabaseTypeKey = "DatabaseType";

        /// <summary>
        /// The configuration key for getting the database connection string.
        /// </summary>
        public static readonly string ConnectionStringKey = "ConnectionString";

        /// <summary>
        /// Handle to the logger.
        /// </summary>
#pragma warning disable IDE0052 // for future use
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#pragma warning restore

        private readonly object lockObject = new object();

        private bool migrateCalled = false;

        /// <summary>
        /// Prevent construction from outside the singleton getter.
        /// </summary>
        private CacheDatabaseProvider()
        {
        }

        /// <summary>
        /// Gets the singleton instance of the database provider class.
        /// </summary>
        public static CacheDatabaseProvider Instance { get; } = new CacheDatabaseProvider();

        /// <summary>
        /// Gets the database configuration to use.
        /// </summary>
        public IConfigurationSection Configuration { get; internal set; } = null;

        /// <summary>
        /// Gets the cache database handle.
        /// </summary>
        public CacheDatabaseContext CacheDatabase
        {
            get
            {
                lock(this.lockObject)
                {
                    var context = new CacheDatabaseContext(this.Configuration);

                    if (!this.migrateCalled)
                    {
                        context.Database.Migrate();
                        this.migrateCalled = true;
                    }

                    return context;
                }
            }
        }
    }

    /// <summary>
    /// Factory class for the purpose of Entitiy Framework design-time.
    /// </summary>
    internal class CacheDatabaseContextFactory : IDesignTimeDbContextFactory<CacheDatabaseContext>
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <inheritdoc />
        public CacheDatabaseContext CreateDbContext(string[] args)
        {
            var configFile = (args != null) && (args.Length > 0) && !string.IsNullOrWhiteSpace(args[0])
                ? args[0] // first command line options is the config file to use
                : "~/hamnetMonitorSettings.json"; // this is the fallback config file - only good for Linux

            configFile = configFile.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            log.Debug($"Using config file '{configFile}'");

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFile, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            CacheDatabaseProvider.Instance.Configuration = configuration.GetSection(CacheDatabaseProvider.CacheDatabaseSectionName);

            return CacheDatabaseProvider.Instance.CacheDatabase;
        }
    }
}
