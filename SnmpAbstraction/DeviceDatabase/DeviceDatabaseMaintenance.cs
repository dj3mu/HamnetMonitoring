using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SnmpSharpNet;

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
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool dryRunMode;

        /// <summary>
        /// Default-Construct
        /// </summary>
        public DeviceDatabaseMaintenance(bool dryRunMode)
        {
            this.dryRunMode = dryRunMode;
        }

        /// <summary>
        /// Gets some statistics info about that database.
        /// </summary>
        /// <returns>Key-value-pairs with the statistics information.</returns>
        public IReadOnlyDictionary<string, string> CacheStatistics()
        {
            using (var dbContext = DeviceDatabaseProvider.Instance.DeviceDatabase)
            {
                return new Dictionary<string, string>
                {
                    { "NumberOfUniqueDevices", dbContext.Devices.Count().ToString() },
                    { "NumberOfUniqueDevicesVersions", dbContext.DeviceVersions.Count().ToString() },
                };
            }
        }
    }
}
