using System;
using System.IO;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Class providing central access 
    /// </summary>
    public class HamnetDbProvider
    {
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

            return new MySqlHamnetDbAccessor(connectionString, null);
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