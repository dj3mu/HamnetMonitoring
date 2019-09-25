using System.IO;
using System.Reflection;

namespace SnmpAbstraction
{
    /// <summary>
    /// Provider class for databases.
    /// </summary>
    internal class DeviceDatabaseProvider
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The device database path and file name.
        /// </summary>
        private static readonly string DeviceDatabasePathAndFile = Path.Combine("Config", "DeviceDatabase.sqlite");

        private string dataBaseFile;

        /// <summary>
        /// Prevent construction from outside the singleton getter.
        /// </summary>
        private DeviceDatabaseProvider()
        {
            this.dataBaseFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DeviceDatabasePathAndFile);

            log.Info($"Initialized for Device Database @ '{this.dataBaseFile}'");
        }

        /// <summary>
        /// Gets the singleton instance of the database provider class.
        /// </summary>
        public static DeviceDatabaseProvider Instance { get; } = new DeviceDatabaseProvider();

        /// <summary>
        /// Gets the device database handle.
        /// </summary>
        public DeviceDatabaseContext DeviceDatabase => new DeviceDatabaseContext(this.dataBaseFile);
    }
}