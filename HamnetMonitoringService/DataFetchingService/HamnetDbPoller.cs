using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        private readonly int maximumSubnetCount = int.MaxValue;

        private readonly int subnetStartOffset = 0;
        
        private readonly int maximumHostCount = int.MaxValue;

        private readonly int hostStartOffset = 0;

        private readonly TimeSpan cacheRefreshInterval = TimeSpan.Zero;

        private readonly IConfiguration configuration;
        
        private readonly IConfigurationSection hamnetDbConfig;

        private Dictionary<IPAddress, IHamnetDbSubnet> subnetCache = new Dictionary<IPAddress, IHamnetDbSubnet>();

        private IHamnetDbSubnets subnets = null;

        private DateTime lastRefresh = DateTime.MinValue;

        private IHamnetDbHosts hamnetDbHostsCache = null;

        private IReadOnlyDictionary<IHamnetDbSubnet, IHamnetDbHosts> uniquePairsCache = null;

        /// <summary>
        /// Constructs a poller that uses the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        public HamnetDbPoller(IConfiguration configuration)
        {
            this.configuration = configuration;

            this.hamnetDbConfig = this.configuration.GetSection(HamnetDbProvider.HamnetDbSectionName);
            var aquisitionConfig = this.configuration.GetSection(Program.RssiAquisitionServiceSectionKey);

            this.maximumSubnetCount = aquisitionConfig.GetValue<int>("MaximumSubnetCount");
            if (this.maximumSubnetCount == 0)
            {
                // config returns 0 if not defined --> turn it to the reasonable "maximum" value
                this.maximumSubnetCount = int.MaxValue;
            }

            this.subnetStartOffset = aquisitionConfig.GetValue<int>("SubnetStartOffset"); // will implicitly return 0 if not defined

            this.maximumHostCount = aquisitionConfig.GetValue<int>("MaximumHostCount");
            if (this.maximumHostCount == 0)
            {
                // config returns 0 if not defined --> turn it to the reasonable "maximum" value
                this.maximumHostCount = int.MaxValue;
            }
            
            this.hostStartOffset = aquisitionConfig.GetValue<int>("HostStartOffset"); // will implicitly return 0 if not defined

            this.cacheRefreshInterval = this.hamnetDbConfig.GetValue<TimeSpan>("CacheRefreshInterval");
        }

        /// <summary>
        /// Retrieves the list of hosts/IPs which are considered to be BGP routers from HamnetDB.
        /// </summary>
        /// <returns>The list of hosts/IPs which are considered to be BGP routers from HamnetDB.</returns>
        public List<IHamnetDbHost> FetchBgpRoutersFromHamnetDb()
        {
            log.Debug($"Getting BGP routers from HamnetDB. Please stand by ...");

            this.InvalidateCacheIfNeeded();

            if (this.hamnetDbHostsCache == null)
            {
                log.Info($"Fetching hosts from HamnetDb");

                using(var accessor = HamnetDbProvider.Instance.GetHamnetDbFromConfiguration(hamnetDbConfig))
                {
                    this.hamnetDbHostsCache = accessor.QueryBgpRouters();

                    log.Debug($"... found {this.hamnetDbHostsCache.Count} routers");
                }
            }

            var hostsSlicedForOptions = this.hamnetDbHostsCache.Skip(this.subnetStartOffset).Take(this.maximumHostCount).ToList();

            return hostsSlicedForOptions;
        }

        /// <summary>
        /// Retrieves the list of subnets with their hosts to monitor from HamnetDB.
        /// </summary>
        /// <returns>The list of subnets with their hosts to monitor from HamnetDB.</returns>
        public Dictionary<IHamnetDbSubnet, IHamnetDbHosts> FetchSubnetsWithHostsFromHamnetDb()
        {
            log.Debug($"Getting unique host pairs to be monitored from HamnetDB. Please stand by ...");

            this.InvalidateCacheIfNeeded();

            if (this.uniquePairsCache == null)
            {
                log.Info($"Fetching unique pairs from HamnetDb");

                using(var accessor = HamnetDbProvider.Instance.GetHamnetDbFromConfiguration(hamnetDbConfig))
                {
                    this.uniquePairsCache = accessor.UniqueMonitoredHostPairsInSameSubnet();

                    log.Debug($"... found {uniquePairsCache.Count} unique pairs");
                }
            }

            var pairsSlicedForOptions = this.uniquePairsCache.Skip(this.subnetStartOffset).Take(this.maximumSubnetCount).ToDictionary(k => k.Key, v => v.Value);

            return pairsSlicedForOptions;
        }

        /// <summary>
        /// Gets the HamnetDB subnet of the given host.
        /// </summary>
        /// <param name="host">The IP address of the host.</param>
        /// <param name="subnet">The subnet of the host.</param>
        /// <returns><c>true</c> if a subnet was found for the given host; otherwise <c>false</c>.</returns>
        public bool TryGetSubnetOfHost(IPAddress host, out IHamnetDbSubnet subnet)
        {
            this.InvalidateCacheIfNeeded(false);

            this.RefreshSubnetsIfNeeded();

            if (this.subnetCache.TryGetValue(host, out subnet))
            {
                return true;
            }

            if (!this.subnets.TryFindDirectSubnetOfAddress(host, out subnet))
            {
                log.Warn($"Subnet for host {host} not found");
                return false;
            }

            this.subnetCache.Add(host, subnet);
            
            return true;
        }

        private void RefreshSubnetsIfNeeded()
        {
            if (this.subnets != null)
            {
                return;
            }

            log.Info($"Fetching subnets from HamnetDB");

            var hamnetDbConfig = this.configuration.GetSection(HamnetDbProvider.HamnetDbSectionName);

            using(var accessor = HamnetDbProvider.Instance.GetHamnetDbFromConfiguration(hamnetDbConfig))
            {
                this.subnets = accessor.QuerySubnets();
            }
        }
 
        private void InvalidateCacheIfNeeded(bool logInfo = true)
        {
            var sinceLastRefresh = (DateTime.UtcNow - this.lastRefresh);
            if (sinceLastRefresh <= this.cacheRefreshInterval)
            {
                if (logInfo)
                {
                    log.Info($"Cache still valid: {sinceLastRefresh} passed since last refresh <= configured interval {this.cacheRefreshInterval}");
                }

                return;
            }

            if (logInfo)
            {
                log.Info($"Cache outdated: {sinceLastRefresh} passed since last refresh > configured interval {this.cacheRefreshInterval}");
            }

            this.subnetCache?.Clear();

            this.hamnetDbHostsCache = null;
            this.uniquePairsCache = null;

            this.lastRefresh = DateTime.UtcNow;
        }
    }
}