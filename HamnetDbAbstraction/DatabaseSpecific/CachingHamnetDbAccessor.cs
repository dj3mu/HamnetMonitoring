using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Implementation of <see cref="IHamnetDbAccess" /> retrieving data via REST / JSON interface of HamnetDB.
    /// </summary>
    internal class CachingHamnetDbAccessor : IDirectSupportOfHamnetDbAccessExtensions
    {
        private static readonly log4net.ILog log = HamnetDbAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Minimum allowed refresh interval for preemtive mode. If configured value is lower than this we'll disable caching.
        /// </summary>
        private static readonly TimeSpan MinimumCacheRefreshIntervalPreemtive = TimeSpan.FromMinutes(2);

        /// <summary>
        /// The underlying IHamnetDbAccess implementation that is used to populate/refresh the cache.
        /// </summary>
        private readonly IHamnetDbAccess underlyingHamnetDbAccess;

        /// <summary>
        /// Locking object to block requests while cache is refreshing (when in preemtive mode).
        /// </summary>
        private readonly object cacheRefreshLock = new object();

        private readonly object multiRefreshPreventionObject = new object();

        private readonly CacheDataStore cacheDataStore;

        /// <summary>
        /// To detect redundant calls.
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// The timer to handle cache refreshing in preemtive mode.
        /// </summary>
        private Timer cacheRefreshTimer = null;

        /// <summary>
        /// Instantiate from connection string and an additional Disposer.
        /// </summary>
        /// <param name="cacheRefreshInterval">The interval at which the cache shall be refreshed.</param>
        /// <param name="underlyingHamnetDbAccess">The underlying IHamnetDbAccess implementation that is used to populate/refresh the cache.</param>
        /// <param name="usePreemtiveCachePopulation">
        /// A value indicating whether to preemptively populate the cache (in background).<br/>
        /// If <c>true</c> the cache will be populated in background at the given interval.<br/>
        /// If <c>false</c> the cache will be populated in foreground and only when needed.
        /// </param>
        public CachingHamnetDbAccessor(TimeSpan cacheRefreshInterval, IHamnetDbAccess underlyingHamnetDbAccess, bool usePreemtiveCachePopulation)
        {
            this.CacheRefreshInterval = (cacheRefreshInterval < TimeSpan.Zero) ? TimeSpan.Zero : cacheRefreshInterval;
            this.underlyingHamnetDbAccess = underlyingHamnetDbAccess ?? throw new ArgumentNullException(nameof(underlyingHamnetDbAccess), "The underlying IHamnetDbAccess instance is null");
            this.UsePreemtiveCachePopulation = usePreemtiveCachePopulation;

            if (this.UsePreemtiveCachePopulation)
            {
                if (this.CacheRefreshInterval >= MinimumCacheRefreshIntervalPreemtive)
                {
                    this.CacheRefreshInterval -= TimeSpan.FromSeconds(3); /* starting a bit before expiry */
                }
                else
                {
                    log.Warn($"Requested cache refresh interval in preemtive mode ({this.CacheRefreshInterval}) < minimum interval for preemtive mode ({MinimumCacheRefreshIntervalPreemtive}): Disabling preemtive mode !");
                    this.UsePreemtiveCachePopulation = false;
                }
            }

            this.cacheDataStore = new CacheDataStore(this.CacheRefreshInterval);

            if (this.UsePreemtiveCachePopulation)
            {
                log.Info($"Starting preemtive cache refresh at interval of {this.CacheRefreshInterval}");
                this.cacheRefreshTimer = new Timer(DoPreemtiveCacheRefresh, null, TimeSpan.Zero, this.CacheRefreshInterval);
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~JsonHamnetDbAccessor()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        public bool UsePreemtiveCachePopulation { get; }

        /// <summary>
        /// The interval at which the cache will be refreshed.
        /// </summary>
        public TimeSpan CacheRefreshInterval { get; }

        /// <inheritdoc />
        public IHamnetDbHosts QueryBgpRouters()
        {
            lock(this.cacheRefreshLock)
            {
                var timeOfQuery = DateTime.UtcNow;
                var dbHostsDataSet = this.cacheDataStore.GetCacheDataSetOrNull<IHamnetDbHosts>(DataTypes.BgpRouters, timeOfQuery);

                return (dbHostsDataSet == null) ? this.RefreshBgpRouters(timeOfQuery) : dbHostsDataSet.Data;
            }
        }

        /// <inheritdoc />
        public IHamnetDbHosts QueryMonitoredHosts()
        {
            lock(this.cacheRefreshLock)
            {
                var timeOfQuery = DateTime.UtcNow;
                var dbHostsDataSet = this.cacheDataStore.GetCacheDataSetOrNull<IHamnetDbHosts>(DataTypes.MonitoredHosts, timeOfQuery);

                return (dbHostsDataSet == null) ? this.RefreshMonitoredHosts(timeOfQuery) : dbHostsDataSet.Data;
            }
        }

        /// <inheritdoc />
        public IHamnetDbSubnets QuerySubnets()
        {
            lock(this.cacheRefreshLock)
            {
                var timeOfQuery = DateTime.UtcNow;
                var dbSubnetsDataSet = this.cacheDataStore.GetCacheDataSetOrNull<IHamnetDbSubnets>(DataTypes.Subnets, timeOfQuery);

                return (dbSubnetsDataSet == null) ? this.RefreshSubnets(timeOfQuery) : dbSubnetsDataSet.Data;
            }
        }

        /// <inheritdoc />
        public IHamnetDbSites QuerySites()
        {
            lock(this.cacheRefreshLock)
            {
                var timeOfQuery = DateTime.UtcNow;
                var dbSiteDataSet = this.cacheDataStore.GetCacheDataSetOrNull<IHamnetDbSites>(DataTypes.Sites, timeOfQuery);

                return (dbSiteDataSet == null) ? this.RefreshSites(timeOfQuery) : dbSiteDataSet.Data;
            }
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<IHamnetDbSubnet, IHamnetDbHosts> UniqueMonitoredHostPairsInSubnet(IPNetwork subnet)
        {
            if (subnet == null)
            {
                throw new ArgumentNullException(nameof(subnet), "subnet search monitored hosts for is null");
            }

            lock(this.cacheRefreshLock)
            {
                var timeOfQuery = DateTime.UtcNow;
                var association =
                    this.cacheDataStore.GetCacheDataSetOrNull<IReadOnlyDictionary<IHamnetDbSubnet, IHamnetDbHosts>>(DataTypes.UniqueMonitoredHostPairsInSubnet, timeOfQuery)?.Data
                    ?? this.RefreshHostAssociations(timeOfQuery);

                var uniquePairs = association
                    .Where(a => subnet.Contains(a.Key.Subnet) || subnet.Equals(a.Key.Subnet))
                    .Where(a => a.Value.Count == 2)
                    .ToDictionary(k => k.Key, v => v.Value);

                return uniquePairs;
            }
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<IHamnetDbSubnet, IHamnetDbHosts> UniqueMonitoredHostPairsInSameSubnet()
        {
            lock(this.cacheRefreshLock)
            {
                var timeOfQuery = DateTime.UtcNow;
                var uniqueMonitoredHostPairsInSameSubnetDataSet = this.cacheDataStore.GetCacheDataSetOrNull<IReadOnlyDictionary<IHamnetDbSubnet, IHamnetDbHosts>>(DataTypes.UniqueMonitoredHostPairsInSameSubnet, timeOfQuery);

                return (uniqueMonitoredHostPairsInSameSubnetDataSet == null) ? this.RefreshUniqueMonitoredHostPairsInSameSubnet(timeOfQuery) : uniqueMonitoredHostPairsInSameSubnetDataSet.Data;
            }
        }

        /// <summary>
        /// Correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes off this instance.
        /// </summary>
        /// <param name="disposing"><c>true</c> when called by <see cref="Dispose()" />.
        /// <c>false</c> when called by finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    // Stop the background refresh timer and dispose off the timer.
                    this.cacheRefreshTimer?.Dispose();
                    this.cacheRefreshTimer = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// The asynchronuous method that is called by the timer to execute refresh of the cache.
        /// </summary>
        /// <param name="state">Required by timer but not used.</param>
        private void DoPreemtiveCacheRefresh(object state)
        {
            // The Monitor handles multiple concurrent refreshes (e.g. when locked for a time > refresh interval)
            if (Monitor.TryEnter(this.multiRefreshPreventionObject))
            {
                try
                {
                    // The lock below prevents running of refresh and query at same time
                    lock (this.cacheRefreshLock)
                    {
                        log.Info("Performing preemtive cache refresh. If you see this too often, check your configuration.");

                        this.RefreshBgpRouters(DateTime.UtcNow);
                        this.RefreshMonitoredHosts(DateTime.UtcNow);
                        this.RefreshSubnets(DateTime.UtcNow);
                        this.RefreshUniqueMonitoredHostPairsInSameSubnet(DateTime.UtcNow);
                        this.RefreshHostAssociations(DateTime.UtcNow);
                    }
                }
                finally
                {
                    Monitor.Exit(this.multiRefreshPreventionObject);
                }
            }
            else
            {
                log.Warn("SKIPPING preemtive cache refresh: Previous refresh still ongoing. Please adjust interval.");
            }
        }

        private IReadOnlyDictionary<IHamnetDbSubnet, IHamnetDbHosts> RefreshUniqueMonitoredHostPairsInSameSubnet(DateTime timeOfQuery)
        {
            var uniqueMonitoredHostPairsInSameSubnet = HamnetDbAccessExtensions.UniqueMonitoredHostPairsInSameSubnet(this, false);
            this.cacheDataStore.AddOrReplaceCacheValue(DataTypes.UniqueMonitoredHostPairsInSameSubnet, uniqueMonitoredHostPairsInSameSubnet, timeOfQuery);

            return uniqueMonitoredHostPairsInSameSubnet;
        }

        private IReadOnlyDictionary<IHamnetDbSubnet, IHamnetDbHosts> RefreshHostAssociations(DateTime timeOfQuery)
        {
            var hosts = this.QueryMonitoredHosts();
            var allSubnets = this.QuerySubnets();

            // filter out parents for which we have nested subnets
            var subnets = allSubnets.Where(s => !allSubnets.Any(a => !object.ReferenceEquals(s.Subnet, a.Subnet) && s.Subnet.Contains(a.Subnet)));

            var association = subnets.AssociateHosts(hosts);

            this.cacheDataStore.AddOrReplaceCacheValue(DataTypes.UniqueMonitoredHostPairsInSubnet, association, timeOfQuery);

            return association;
        }

        private IHamnetDbHosts RefreshMonitoredHosts(DateTime timeOfQuery)
        {
            var hamnetDbHosts = this.underlyingHamnetDbAccess.QueryMonitoredHosts();
            this.cacheDataStore.AddOrReplaceCacheValue(DataTypes.MonitoredHosts, hamnetDbHosts, timeOfQuery);

            return hamnetDbHosts;
        }

        private IHamnetDbHosts RefreshBgpRouters(DateTime timeOfQuery)
        {
            var hamnetDbHosts = this.underlyingHamnetDbAccess.QueryBgpRouters();
            this.cacheDataStore.AddOrReplaceCacheValue(DataTypes.BgpRouters, hamnetDbHosts, timeOfQuery);

            return hamnetDbHosts;
        }

        private IHamnetDbSubnets RefreshSubnets(DateTime timeOfQuery)
        {
            var hamnetDbSubnets = this.underlyingHamnetDbAccess.QuerySubnets();
            this.cacheDataStore.AddOrReplaceCacheValue(DataTypes.Subnets, hamnetDbSubnets, timeOfQuery);

            return hamnetDbSubnets;
        }

        private IHamnetDbSites RefreshSites(DateTime timeOfQuery)
        {
            var hamnetDbSites = this.underlyingHamnetDbAccess.QuerySites();
            this.cacheDataStore.AddOrReplaceCacheValue(DataTypes.Sites, hamnetDbSites, timeOfQuery);

            return hamnetDbSites;
        }

        /// <summary>
        /// Enumeration of data types stored in the cache store.
        /// </summary>
        private enum DataTypes
        {
            BgpRouters,

            MonitoredHosts,

            Subnets,

            UniqueMonitoredHostPairsInSubnet,

            UniqueMonitoredHostPairsInSameSubnet,

            Sites
        }

        /// <summary>
        /// The datastore for the cache.
        /// </summary>
        private class CacheDataStore
        {
            private readonly Dictionary<DataTypes, object> cacheData = new Dictionary<DataTypes, object>(3);

            private readonly TimeSpan cacheRefreshInterval;

            public CacheDataStore(TimeSpan cacheRefreshInterval)
            {
                this.cacheRefreshInterval = cacheRefreshInterval;
            }

            /// <summary>
            /// Gets cache value of the given type or null.
            /// </summary>
            /// <typeparam name="TCacheValue">The type of the cache value.</typeparam>
            public CacheDataSet<TCacheValue> GetCacheDataSetOrNull<TCacheValue>(DataTypes dataType, DateTime timeOfQuery) where TCacheValue : class
            {
                if (this.cacheData.TryGetValue(dataType, out object valueAsObject))
                {
                    var returnObject = valueAsObject as CacheDataSet<TCacheValue>;
                    if (returnObject != null)
                    {
                        // object found but need to check if expired
                        var timediff = (timeOfQuery - returnObject.LastRefreshTime);
                        if (timediff > this.cacheRefreshInterval)
                        {
                            // cached value is outdated --> behave as if it doesn't exist at all
                            log.Info($"Cache data for {dataType} is outdated (timediff {timediff} > cache refresh interval {this.cacheRefreshInterval})");
                            return null;
                        }
                    }

                    return returnObject; // can be null if not in cache
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Adds or replaces the cache value of the implicitly given type.
            /// </summary>
            /// <param name="dataType">The type of the cache entry.</param>
            /// <param name="value">The actual data.</param>
            /// <param name="timeOfAquisition">The time at which the data has been aquired.</param>
            /// <typeparam name="TCacheValue">The type of the cache value.</typeparam>
            public void AddOrReplaceCacheValue<TCacheValue>(DataTypes dataType, TCacheValue value, DateTime timeOfAquisition) where TCacheValue : class
            {
                this.cacheData[dataType] = new CacheDataSet<TCacheValue>(value, timeOfAquisition);
            }

            /// <summary>
            /// /// Container for a single cache data object and its last refresh time.
            /// </summary>
            /// <typeparam name="TCacheValue">The type of the cache value.</typeparam>
            public class CacheDataSet<TCacheValue> where TCacheValue : class
            {
                /// <summary>
                /// Creates an instance of a cache data set.
                /// </summary>
                /// <param name="value">The actual data.</param>
                /// <param name="lastRefreshTime">The time at which the data has been aquired.</param>
                public CacheDataSet(TCacheValue value, DateTime lastRefreshTime)
                {
                    this.Data = value;
                    this.LastRefreshTime = lastRefreshTime;
                }

                /// <summary>
                /// Gets the actual data.
                /// </summary>
                public TCacheValue Data { get; }

                /// <summary>
                /// Gets the time at which the data has been aquired.
                /// </summary>
                public DateTime LastRefreshTime { get; }
            }
        }
    }
}