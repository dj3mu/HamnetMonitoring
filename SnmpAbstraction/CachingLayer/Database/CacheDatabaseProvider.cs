using System.IO;
using System.Reflection;
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
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private object lockObject = new object();

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
                    
                    context.Database.EnsureCreated();

                    return context;
                }
            }
        }
    }
}
