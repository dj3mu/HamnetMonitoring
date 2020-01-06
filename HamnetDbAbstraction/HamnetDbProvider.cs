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
        /// The configuration key for getting the API URL for obtaining the hosts of HamnetDB.
        /// </summary>
        public static readonly string HostsUrlKey = "Hosts";

        /// <summary>
        /// The configuration key for getting the API URL for obtaining the subnets of HamnetDB.
        /// </summary>
        public static readonly string SubnetsUrlKey = "Subnets";

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
        public IHamnetDbAccess GetHamnetDbFromConfiguration(IConfigurationSection configurationSection)
        {
            if (configurationSection == null)
            {
                throw new ArgumentNullException(nameof(configurationSection), "The configuration section is null");
            }

            if (configurationSection.GetValue<string>(HamnetDbProvider.DatabaseTypeKey).ToUpperInvariant() == "MYSQL")
            {
                return this.InstantiateMySqlAccessor(configurationSection.GetValue<string>(HamnetDbProvider.ConnectionStringKey));
            }
            
            if (configurationSection.GetValue<string>(HamnetDbProvider.DatabaseTypeKey).ToUpperInvariant() == "JSONURL")
            {
                return this.InstantiateJsonUrlAccessor(configurationSection.GetSection(HamnetDbProvider.DatabaseUrlsKey));
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
                this.connectionString = this.ReadAndValidateConnectionStringFromFile(connectionStringFile);
            }

            return this.InstantiateMySqlAccessor(this.connectionString);
        }

        /// <summary>
        /// Instantiates the DB accessor.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The accessor instance.</returns>
        private IHamnetDbAccess InstantiateMySqlAccessor(string connectionString)
        {
            return new MySqlHamnetDbAccessor(connectionString, null);
        }

        /// <summary>
        /// Instantiates the DB accessor.
        /// </summary>
        /// <param name="configurationSection">The configuration section containing the API URLs.</param>
        /// <returns>The accessor instance.</returns>
        private IHamnetDbAccess InstantiateJsonUrlAccessor(IConfigurationSection configurationSection)
        {
            return new JsonHamnetDbAccessor(
                configurationSection.GetValue<string>(HamnetDbProvider.HostsUrlKey),
                configurationSection.GetValue<string>(HamnetDbProvider.SubnetsUrlKey),
                null
                );
        }

        /// <summary>
        /// Reads the connection string from the given file.
        /// </summary>
        /// <param name="connectionStringFile">A path to and name of a file containing the database connection string.</param>
        /// <returns>The connection string read from the file.</returns>
        private string ReadAndValidateConnectionStringFromFile(string connectionStringFile)
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