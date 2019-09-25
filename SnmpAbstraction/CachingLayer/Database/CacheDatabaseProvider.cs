using System.IO;
using System.Reflection;

namespace SnmpAbstraction
{
    /// <summary>
    /// Provider class for databases.
    /// </summary>
    internal class CacheDatabaseProvider
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The device database path and file name.
        /// </summary>
        private static readonly string DeviceDatabasePathAndFile = Path.Combine("Config", "CacheDatabase.sqlite");

        private string dataBaseFile;

        /// <summary>
        /// Prevent construction from outside the singleton getter.
        /// </summary>
        private CacheDatabaseProvider()
        {
            this.dataBaseFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DeviceDatabasePathAndFile);

            log.Info($"Initialized for Device Database @ '{this.dataBaseFile}'");
        }

        /// <summary>
        /// Gets the singleton instance of the database provider class.
        /// </summary>
        public static CacheDatabaseProvider Instance { get; } = new CacheDatabaseProvider();

        /// <summary>
        /// Gets the cache database handle.
        /// </summary>
        public CacheDatabaseContext CacheDatabase
        {
            get
            {
                var context = new CacheDatabaseContext(this.dataBaseFile);
                
                context.Database.EnsureCreated();

                return context;
            }
        }
    }
}
