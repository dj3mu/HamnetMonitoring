using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Class providing central access
    /// </summary>
    public class HamnetDbProvider
    {
        /// <summary>
        /// The configuration section name for the Hamnet database configuration.
        /// </summary>
        public static readonly string HamnetDbSectionName = "HamnetDb";

        /// <summary>
        /// The configuration key for getting the database type.
        /// </summary>
        public static readonly string DatabaseTypeKey = "DatabaseType";

        /// <summary>
        /// The configuration key for getting the database connection string.
        /// </summary>
        public static readonly string ConnectionStringKey = "ConnectionString";

        /// <summary>
        /// The configuration key for getting the cache refresh interval value.
        /// </summary>
        public static readonly string CacheRefreshIntervalKey = "CacheRefreshInterval";

        /// <summary>
        /// The configuration key for getting the the flag whether to refresh cache preemtively.
        /// </summary>
        public static readonly string PreemptiveCacheRefreshKey = "PreemtiveCacheRefresh";

        /// <summary>
        /// The configuration key for getting the API URL for obtaining the hosts of HamnetDB.
        /// </summary>
        public static readonly string HostsUrlKey = "Hosts";

        /// <summary>
        /// The configuration key for getting the API URL for obtaining the subnets of HamnetDB.
        /// </summary>
        public static readonly string SubnetsUrlKey = "Subnets";

        /// <summary>
        /// The configuration key for getting the API URL for obtaining the sites of HamnetDB.
        /// </summary>
        public static readonly string SitesUrlKey = "Sites";

        /// <summary>
        /// The configuration key for getting the database API URLs.
        /// </summary>
        public static readonly string DatabaseUrlsKey = "DatabaseUrls";

        /// <summary>
        /// The connection string to use.
        /// </summary>
        private string connectionString = null;

        /// <summary>
        /// Prevent instantiation from outside the singleton getter.
        /// </summary>
        private HamnetDbProvider()
        {
        }

        /// <summary>
        /// Gets the singleton instance of the HamnetDB provider.
        /// </summary>
        public static HamnetDbProvider Instance { get; } = new HamnetDbProvider();

        /// <summary>
        /// Gets an abstract functionality handle to the HamnetDB.
        /// </summary>
        /// <param name="configurationSection">The configuration section.</param>
        /// <returns>A handle to an abstract database interface.</returns>
#pragma warning disable CA1822 // by design pattern
        public IHamnetDbAccess GetHamnetDbFromConfiguration(IConfigurationSection configurationSection)
#pragma warning restore
        {
            if (configurationSection == null)
            {
                throw new ArgumentNullException(nameof(configurationSection), "The configuration section is null");
            }

            var cacheRefreshIntervalString = configurationSection.GetValue<string>(HamnetDbProvider.CacheRefreshIntervalKey);
            TimeSpan cacheRefreshInterval = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(cacheRefreshIntervalString) || !TimeSpan.TryParse(cacheRefreshIntervalString, out cacheRefreshInterval))
            {
                // by default do not cache
                cacheRefreshInterval = TimeSpan.Zero;
            }

            var cachePreemptiveRefreshString = configurationSection.GetValue<string>(HamnetDbProvider.PreemptiveCacheRefreshKey);
            if (string.IsNullOrWhiteSpace(cachePreemptiveRefreshString) || !Boolean.TryParse(cachePreemptiveRefreshString, out bool cacheRefreshPreemptive))
            {
                // by default use preemtive cache refresh
                cacheRefreshPreemptive = true;
            }

            if (configurationSection.GetValue<string>(HamnetDbProvider.DatabaseTypeKey).ToUpperInvariant() == "MYSQL")
            {
                var accessor = InstantiateMySqlAccessor(configurationSection.GetValue<string>(HamnetDbProvider.ConnectionStringKey));
                return (cacheRefreshInterval != TimeSpan.Zero)
                    ? new CachingHamnetDbAccessor(cacheRefreshInterval, accessor, cacheRefreshPreemptive)
                    : accessor;
            }

            if (configurationSection.GetValue<string>(HamnetDbProvider.DatabaseTypeKey).ToUpperInvariant() == "JSONURL")
            {
                var accessor = InstantiateJsonUrlAccessor(configurationSection.GetSection(HamnetDbProvider.DatabaseUrlsKey));
                return (cacheRefreshInterval != TimeSpan.Zero)
                    ? new CachingHamnetDbAccessor(cacheRefreshInterval, accessor, cacheRefreshPreemptive)
                    : accessor;
            }

            throw new InvalidOperationException($"Only MySQL or JsonUrl is currently supported for the HamentDB interface but found '{configurationSection.GetValue<string>(HamnetDbProvider.DatabaseTypeKey)}'");
        }

        /// <summary>
        /// Gets an abstract functionality handle to the HamnetDB.
        /// </summary>
        /// <param name="connectionStringFile">A path to and name of a file containing the database connection string.</param>
        /// <returns>A handle to an abstract database interface.</returns>
        public IHamnetDbAccess GetHamnetDb(string connectionStringFile)
        {
            if (string.IsNullOrWhiteSpace(connectionStringFile))
            {
                throw new ArgumentNullException(nameof(connectionStringFile), "The connections string file name is null, empty or white-space-only");
            }

            return this.ManufactureHamnetDbAccess(connectionStringFile);
        }

        /// <summary>
        /// Retrieves or creates a new <see cref="IHamnetDbAccess" /> object for the connection string given in the mentioned file.
        /// </summary>
        /// <param name="connectionStringFile">A path to and name of a file containing the database connection string.</param>
        /// <returns>A handle to an abstract database interface.</returns>
        private IHamnetDbAccess ManufactureHamnetDbAccess(string connectionStringFile)
        {
            if (string.IsNullOrWhiteSpace(this.connectionString))
            {
                this.connectionString = ReadAndValidateConnectionStringFromFile(connectionStringFile);
            }

            return InstantiateMySqlAccessor(this.connectionString);
        }

        /// <summary>
        /// Instantiates the DB accessor.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The accessor instance.</returns>
        private static IHamnetDbAccess InstantiateMySqlAccessor(string connectionString)
        {
            return new MySqlHamnetDbAccessor(connectionString, null);
        }

        /// <summary>
        /// Instantiates the DB accessor.
        /// </summary>
        /// <param name="configurationSection">The configuration section containing the API URLs.</param>
        /// <returns>The accessor instance.</returns>
        private static IHamnetDbAccess InstantiateJsonUrlAccessor(IConfigurationSection configurationSection)
        {
            return new JsonHamnetDbAccessor(
                configurationSection.GetValue<string>(HamnetDbProvider.HostsUrlKey),
                configurationSection.GetValue<string>(HamnetDbProvider.SubnetsUrlKey),
                configurationSection.GetValue<string>(HamnetDbProvider.SitesUrlKey),
                null
                );
        }

        /// <summary>
        /// Reads the connection string from the given file.
        /// </summary>
        /// <param name="connectionStringFile">A path to and name of a file containing the database connection string.</param>
        /// <returns>The connection string read from the file.</returns>
        private static string ReadAndValidateConnectionStringFromFile(string connectionStringFile)
        {
            var fileNameToUse = connectionStringFile.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            if (!File.Exists(fileNameToUse))
            {
                throw new FileNotFoundException($"Connection string file '{connectionStringFile}' does not exist");
            }

            var connectionString = File.ReadAllText(fileNameToUse).Trim();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException($"The connection string in file '{fileNameToUse}' is null, empty or white-space-only");
            }

            return connectionString;
        }
    }
}