using System;
using System.Collections.Generic;
using System.IO;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Class providing central access 
    /// </summary>
    public class HamnetDbProvider
    {
        /// <summary>
        /// The singleton lookup to avoid re-instantiation of connections.
        /// </summary>
        private Dictionary<string, IHamnetDbAccess> accessSingletonLookup = new Dictionary<string, IHamnetDbAccess>();

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
            string connectionString = this.ReadAndValidateConnectionStringFromFile(connectionStringFile);

            IHamnetDbAccess accessor = null;
            if (!this.accessSingletonLookup.TryGetValue(connectionString, out accessor))
            {
                // Different database engine shall be distinguished here, instantiating the matching accessor.
                // As of now we only support MySQL. But from design perspective we're ready to handle arbitrary engines.
                 accessor = new MySqlHamnetDbAccessor(connectionString, new FactoryEntryRemovingDisposer(this.accessSingletonLookup, connectionString));
                 this.accessSingletonLookup.Add(connectionString, accessor);
            }

            return accessor;
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

    /// <summary>
    /// Helper class to remove an accessor entry from the provider list if it gets explicitly disposed off.
    /// </summary>
    internal class FactoryEntryRemovingDisposer : IDisposable
    {
        private Dictionary<string, IHamnetDbAccess> accessSingletonLookup;

        private string connectionString;

        /// <summary>
        /// Constructs taking everything we need to remove the entry from the povider's handler list.
        /// </summary>
        /// <param name="accessSingletonLookup">The lookup to remove the handler from.</param>
        /// <param name="connectionString">The connection string to use in the lookup.</param>
        public FactoryEntryRemovingDisposer(Dictionary<string, IHamnetDbAccess> accessSingletonLookup, string connectionString)
        {
            this.accessSingletonLookup = accessSingletonLookup;
            this.connectionString = connectionString;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // this is why this helper class exists ...
            this.accessSingletonLookup.Remove(this.connectionString);
        }
    }
}