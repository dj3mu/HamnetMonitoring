using System.Collections.Generic;
using System.Linq;
using HamnetDbAbstraction;
using HamnetDbRest;
using log4net;
using Microsoft.Extensions.Configuration;

namespace RestService.DataFetchingService
{
    /// <summary>
    /// Helper class for polling the data from HamnetDB.
    /// </summary>
    internal class HamnetDbPoller
    {
        private static readonly ILog log = Program.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IConfiguration configuration;

        /// <summary>
        /// Constructs a poller that uses the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        public HamnetDbPoller(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Retrieves the list of hosts/IPs which are considered to be BGP routers from HamnetDB.
        /// </summary>
        /// <returns>The list of hosts/IPs which are considered to be BGP routers from HamnetDB.</returns>
        public List<IHamnetDbHost> FetchBgpRoutersFromHamnetDb()
        {
            log.Debug($"Getting BGP routers from HamnetDB. Please stand by ...");

            var hamnetDbConfig = this.configuration.GetSection(HamnetDbProvider.HamnetDbSectionName);
            var aquisitionConfig = this.configuration.GetSection(Program.RssiAquisitionServiceSectionKey);

            using(var accessor = HamnetDbProvider.Instance.GetHamnetDbFromConfiguration(hamnetDbConfig))
            {
                var routers = accessor.QueryBgpRouters();

                log.Debug($"... found {routers.Count} routers");

                int maximumHostCount = aquisitionConfig.GetValue<int>("MaximumHostCount");
                if (maximumHostCount == 0)
                {
                    // config returns 0 if not defined --> turn it to the reasonable "maximum" value
                    maximumHostCount = int.MaxValue;
                }

                int startOffset = aquisitionConfig.GetValue<int>("HostStartOffset"); // will implicitly return 0 if not defined

                var hostsSlicedForOptions = routers.Skip(startOffset).Take(maximumHostCount).ToList();

                return hostsSlicedForOptions;
            }
        }

        /// <summary>
        /// Retrieves the list of subnets with their hosts to monitor from HamnetDB.
        /// </summary>
        /// <returns>The list of subnets with their hosts to monitor from HamnetDB.</returns>
        public Dictionary<IHamnetDbSubnet, IHamnetDbHosts> FetchSubnetsWithHostsFromHamnetDb()
        {
            log.Debug($"Getting unique host pairs to be monitored from HamnetDB. Please stand by ...");

            var hamnetDbConfig = this.configuration.GetSection(HamnetDbProvider.HamnetDbSectionName);
            var aquisitionConfig = this.configuration.GetSection(Program.RssiAquisitionServiceSectionKey);

            using(var accessor = HamnetDbProvider.Instance.GetHamnetDbFromConfiguration(hamnetDbConfig))
            {
                var uniquePairs = accessor.UniqueMonitoredHostPairsInSameSubnet();

                log.Debug($"... found {uniquePairs.Count} unique pairs");

                int maximumSubnetCount = aquisitionConfig.GetValue<int>("MaximumSubnetCount");
                if (maximumSubnetCount == 0)
                {
                    // config returns 0 if not defined --> turn it to the reasonable "maximum" value
                    maximumSubnetCount = int.MaxValue;
                }

                int startOffset = aquisitionConfig.GetValue<int>("SubnetStartOffset"); // will implicitly return 0 if not defined

                var pairsSlicedForOptions = uniquePairs.Skip(startOffset).Take(maximumSubnetCount).ToDictionary(k => k.Key, v => v.Value);

                return pairsSlicedForOptions;
            }
        }
    }
}