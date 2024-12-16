using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace SnmpAbstraction
{
    /// <summary>
    /// Class performaning maitenance tasks on the cache data.
    /// </summary>
    public class DeviceDatabaseMaintenance
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
#pragma warning disable IDE0052 // for future use
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly bool dryRunMode;
#pragma warning restore

        /// <summary>
        /// Default-Construct
        /// </summary>
        public DeviceDatabaseMaintenance(bool dryRunMode)
        {
            this.dryRunMode = dryRunMode;
        }

        /// <summary>
        /// Sets the database configuration from the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        public static void SetDatabaseConfiguration(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "the configuration to set is null");
            }

            DeviceDatabaseProvider.Instance.Configuration = configuration.GetSection(CacheDatabaseProvider.CacheDatabaseSectionName);
        }

        /// <summary>
        /// Gets some statistics info about that database.
        /// </summary>
        /// <returns>Key-value-pairs with the statistics information.</returns>
#pragma warning disable CA1822 // API
        public IReadOnlyDictionary<string, string> CacheStatistics()
#pragma warning restore
        {
            using var dbContext = DeviceDatabaseProvider.Instance.DeviceDatabase;
            return new Dictionary<string, string>
                {
                    { "NumberOfUniqueDevices", dbContext.Devices.Count().ToString() },
                    { "NumberOfUniqueDevicesVersions", dbContext.DeviceVersions.Count().ToString() },
                };
        }
    }
}
