using System.IO;
using System.Reflection;

namespace SnmpAbstraction
{
    /// <summary>
    /// Provider class for databases.
    /// </summary>
    internal class DatabaseProvider
    {
        /// <summary>
        /// The device database path and file name.
        /// </summary>
        private static readonly string DeviceDatabasePathAndFile = Path.Combine("Config", "DeviceDatabase.sqlite");

        /// <summary>
        /// Prevent construction from outside the singleton getter.
        /// </summary>
        private DatabaseProvider()
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.DeviceDatabase = new DeviceDatabaseContext(Path.Combine(assemblyFolder, DeviceDatabasePathAndFile));
        }

        /// <summary>
        /// Gets the singleton instance of the database provider class.
        /// </summary>
        public static DatabaseProvider Instance { get; } = new DatabaseProvider();

        /// <summary>
        /// Gets the device database handle.
        /// </summary>
        public DeviceDatabaseContext DeviceDatabase { get; }
    }
}