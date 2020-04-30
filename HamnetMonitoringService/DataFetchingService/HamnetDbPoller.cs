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
    internal class HamnetDbPoller : IDisposable
    {
        private static readonly ILog log = Program.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int maximumSubnetCount = int.MaxValue;

        private readonly int subnetStartOffset = 0;
        
        private readonly int maximumHostCount = int.MaxValue;

        private readonly int hostStartOffset = 0;

        private readonly TimeSpan cacheRefreshInterval = TimeSpan.Zero;

        private readonly IConfiguration configuration;
        
        private readonly IConfigurationSection hamnetDbConfig;

        private bool disposedValue = false;

        private Dictionary<IPAddress, IHamnetDbSubnet> subnetCache = new Dictionary<IPAddress, IHamnetDbSubnet>();

        private DateTime lastRefresh = DateTime.MinValue;

        private IHamnetDbAccess hamnetDbAccess;

        /// <summary>
        /// Constructs a poller that uses the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="hamnetDbAccess">The HamnetDB access object to use. If null, a new instance will be requested from HamnetDB provider.</param>
        public HamnetDbPoller(IConfiguration configuration, IHamnetDbAccess hamnetDbAccess)
        {
            this.configuration = configuration;

            this.hamnetDbConfig = this.configuration.GetSection(HamnetDbProvider.HamnetDbSectionName);
            var rssiAquisitionConfig = this.configuration.GetSection(Program.RssiAquisitionServiceSectionKey);

            this.maximumSubnetCount = rssiAquisitionConfig.GetValue<int>("MaximumSubnetCount");
            if (this.maximumSubnetCount == 0)
            {
                // config returns 0 if not defined --> turn it to the reasonable "maximum" value
                this.maximumSubnetCount = int.MaxValue;
            }

            this.subnetStartOffset = rssiAquisitionConfig.GetValue<int>("SubnetStartOffset"); // will implicitly return 0 if not defined

            var bgpAquisitionConfig = this.configuration.GetSection(Program.BgpAquisitionServiceSectionKey);
            this.maximumHostCount = bgpAquisitionConfig.GetValue<int>("MaximumHostCount");
            if (this.maximumHostCount == 0)
            {
                // config returns 0 if not defined --> turn it to the reasonable "maximum" value
                this.maximumHostCount = int.MaxValue;
            }
            
            this.hostStartOffset = bgpAquisitionConfig.GetValue<int>("HostStartOffset"); // will implicitly return 0 if not defined

            this.cacheRefreshInterval = this.hamnetDbConfig.GetValue<TimeSpan>("CacheRefreshInterval");

            this.hamnetDbAccess = hamnetDbAccess ?? HamnetDbProvider.Instance.GetHamnetDbFromConfiguration(this.hamnetDbConfig);
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~HamnetDbPoller()
        // {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   this.Dispose(false);
        // }

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            this.Dispose(true);
            // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            // GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Retrieves the list of hosts/IPs which are considered to be BGP routers from HamnetDB.
        /// </summary>
        /// <returns>The list of hosts/IPs which are considered to be BGP routers from HamnetDB.</returns>
        public List<IHamnetDbHost> FetchBgpRoutersFromHamnetDb()
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(HamnetDbPoller));
            }

            log.Debug($"Getting BGP routers from HamnetDB. Please stand by ...");

            log.Debug($"Fetching BGP routers from HamnetDb");

            var bgpRouters = this.hamnetDbAccess.QueryBgpRouters();

            log.Debug($"... found {bgpRouters.Count} routers");

            var bpgRoutersSlicedForOptions = bgpRouters.Skip(this.subnetStartOffset).Take(this.maximumHostCount).ToList();

            return bpgRoutersSlicedForOptions;
        }

        /// <summary>
        /// Retrieves the list of subnets with their hosts to monitor from HamnetDB.
        /// </summary>
        /// <returns>The list of subnets with their hosts to monitor from HamnetDB.</returns>
        public Dictionary<IHamnetDbSubnet, IHamnetDbHosts> FetchSubnetsWithHostsFromHamnetDb()
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(HamnetDbPoller));
            }

            log.Debug($"Getting unique host pairs to be monitored from HamnetDB. Please stand by ...");

            this.InvalidateCacheIfNeeded();

            log.Debug($"Fetching unique pairs from HamnetDb");

            var uniquePairsCache = this.hamnetDbAccess.UniqueMonitoredHostPairsInSameSubnet();

            log.Debug($"... found {uniquePairsCache.Count} unique pairs");

            var pairsSlicedForOptions = uniquePairsCache.Skip(this.subnetStartOffset).Take(this.maximumSubnetCount).ToDictionary(k => k.Key, v => v.Value);

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
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(HamnetDbPoller));
            }

            this.InvalidateCacheIfNeeded(false);

            if (this.subnetCache.TryGetValue(host, out subnet))
            {
                return true;
            }

            if (!this.hamnetDbAccess.QuerySubnets().TryFindDirectSubnetOfAddress(host, out subnet))
            {
                log.Warn($"Subnet for host {host} not found");
                return false;
            }

            this.subnetCache.Add(host, subnet);
            
            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
                    this.hamnetDbAccess?.Dispose();
                    this.hamnetDbAccess = null;
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                this.disposedValue = true;
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

            this.lastRefresh = DateTime.UtcNow;
        }
    }
}