//#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HamnetDbAbstraction;
using HamnetDbRest;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SnmpAbstraction;

namespace RestService.DataFetchingService
{
    /// <summary>
    /// Implementation of an <see cref="IAquiredDataHandler" /> that records failures and allows filtering
    /// based on the failures and the time and how often they have last been seen.
    /// </summary>
    public class FailureRetryFilteringDataHandler : IFailureRetryFilteringDataHandler
    {
        private static readonly log4net.ILog log = Program.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string MinimumFailureRetryIntervalKey = "MinimumFailureRetryInterval";

        private static readonly string MaximumFailureRetryIntervalKey = "MaximumFailureRetryInterval";

        private readonly Random randomizer = new Random();

        private readonly IConfiguration configuration;

        private readonly FailureRecordStore recordStore;

        private bool disposedValue = false;

        /// <summary>
        /// Construct for the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration to construct for.</param>
        public FailureRetryFilteringDataHandler(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "configuration is null");

            var penaltyConfiguration = this.configuration.GetSection(Program.PenaltySystemSectionKey);

            var configTimestampString = penaltyConfiguration.GetValue<string>(MinimumFailureRetryIntervalKey);
            if (!TimeSpan.TryParse(configTimestampString, out TimeSpan minimumRetryInterval))
            {
                minimumRetryInterval = TimeSpan.FromMinutes(1);
            }

            configTimestampString = penaltyConfiguration.GetValue<string>(MaximumFailureRetryIntervalKey);
            if (!TimeSpan.TryParse(configTimestampString, out TimeSpan maximumRetryInterval))
            {
                maximumRetryInterval = TimeSpan.FromMinutes(60);
            }

            log.Info($"Initialized FailureRetryFilteringDataHandler with minimum retry interval = {minimumRetryInterval}, maximum retry interval = {maximumRetryInterval}");

            this.recordStore = new FailureRecordStore(minimumRetryInterval, maximumRetryInterval);
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~InfluxDatabaseDataHandler()
        // {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        /// <inheritdoc />
        public string Name { get; } = "01 - Failure Retry Filtering";

        /// <inheritdoc />
        public void InitializeData(QueryType queryType, IEnumerable<ISingleFailureInfoWithEntity> singleFailureInfos)
        {
            this.recordStore.InitializeData(queryType, singleFailureInfos);
        }

        /// <inheritdoc />
        public bool? IsRetryFeasible(QueryType source, IPAddress address, IPNetwork network)
        {
            var feasible = this.recordStore.IsRetryFeasible(source, network, address);
            var randomizedFeasible = this.RandomizeFeasibility(feasible);

            if (randomizedFeasible != null)
            {
                log.Info($"IsRetryFeasible({source}, {address}, {network}): {feasible} (with randomness: {randomizedFeasible})");
            }

            return randomizedFeasible;
        }

        /// <inheritdoc />
        public bool? IsRetryFeasible(QueryType source, IEnumerable<IPAddress> addresses, IPNetwork network)
        {
            bool? feasible = null;
            foreach (IPAddress address in addresses)
            {
                bool? singleAddressFeasible = this.recordStore.IsRetryFeasible(source, network, address);
                if (singleAddressFeasible.HasValue)
                {
                    if (feasible.HasValue)
                    {
                        feasible = feasible.Value && singleAddressFeasible.Value;
                    }
                    else
                    {
                        feasible = singleAddressFeasible.Value;
                    }
                }
            }

            if (feasible == null)
            {
                return null;
            }

            var randomizedFeasible = this.RandomizeFeasibility(feasible);

            log.Info($"IsRetryFeasible({source}, {string.Join(", ", addresses)}, {network}): {feasible} (with randomness: {randomizedFeasible})");

            return randomizedFeasible;
        }

        /// <inheritdoc />
        public bool? IsRetryFeasible(QueryType source, IPAddress address)
        {
            var feasible = this.recordStore.IsRetryFeasible(source, address);
            var randomizedFeasible = this.RandomizeFeasibility(feasible);

            if (randomizedFeasible != null)
            {
                log.Info($"IsRetryFeasible({source}, {address}): {feasible} (with randomness: {randomizedFeasible})");
            }

            return randomizedFeasible;
        }

        /// <inheritdoc />
        public bool? IsRetryFeasible(QueryType source, IPNetwork network)
        {
            var feasible = this.recordStore.IsRetryFeasible(source, network);
            var randomizedFeasible = this.RandomizeFeasibility(feasible);

            if (randomizedFeasible != null)
            {
                log.Info($"IsRetryFeasible({source}, {network}): {feasible} (with randomness: {randomizedFeasible})");
            }

            return randomizedFeasible;
        }

        /// <inheritdoc />
        public ISingleFailureInfo QueryPenaltyDetails(QueryType source, IPAddress address)
        {
            return this.recordStore.QueryDetails(source, address);
        }

        /// <inheritdoc />
        public ISingleFailureInfo QueryPenaltyDetails(QueryType source, IPNetwork network)
        {
            return this.recordStore.QueryDetails(source, network);
        }

        /// <inheritdoc />
        public void AquisitionFinished()
        {
            // NOP for now but printing some debug info
            log.Info($"Record store after aquisition:{Environment.NewLine}{this.recordStore}");
        }

        /// <inheritdoc />
        public void PrepareForNewAquisition()
        {
            // NOP for now
        }

        /// <inheritdoc />
        public void RecordRssiDetailsInDatabase(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData, ILinkDetails linkDetails, DateTime queryTime)
        {
            this.recordStore.RecordSuccess(QueryType.RssiQuery, inputData.Value.Select(v => v.Address), inputData.Key.Subnet);
        }

        /// <inheritdoc />
        public Task RecordRssiDetailsInDatabaseAsync(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData, ILinkDetails linkDetails, DateTime queryTime)
        {
            return Task.Run(() =>
            {
                try
                {
                    this.RecordRssiDetailsInDatabase(inputData, linkDetails, queryTime);
                }
                catch (Exception ex)
                {
                    // we don not want to throw from an async task
                    log.Error($"Caught and ignored exception in async recording of RSSI details for {inputData.Key} @ {queryTime}: {ex.Message}", ex);
                }
            });
        }

        /// <inheritdoc />
        public void RecordFailingRssiQuery(Exception exception, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData)
        {
            this.recordStore.RecordFailure(QueryType.RssiQuery, inputData.Value.Select(v => v.Address), inputData.Key.Subnet);
        }

        /// <inheritdoc />
        public Task RecordFailingRssiQueryAsync(Exception exception, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData)
        {
            // NOP here - as of now, no failing queries recorded in InfluxDB
            return Task.Run(() =>
            {
                try
                {
                    this.RecordFailingRssiQuery(exception, inputData);
                }
                catch (Exception ex)
                {
                    // we don not want to throw from an async task
                    log.Error($"Caught and ignored exception in async recording of RSSI failure for {inputData.Key}: {ex.Message}", ex);
                }
            });
        }

        /// <inheritdoc />
        public void RecordFailingBgpQuery(Exception exception, IHamnetDbHost host)
        {
            this.recordStore.RecordFailure(QueryType.BgpQuery, host.Address, null);
        }

        /// <inheritdoc />
        public Task RecordFailingBgpQueryAsync(Exception exception, IHamnetDbHost host)
        {
            // NOP here - as of now, no failing queries recorded in InfluxDB
            return Task.Run(() =>
            {
                try
                {
                    this.RecordFailingBgpQuery(exception, host);
                }
                catch (Exception ex)
                {
                    // we don not want to throw from an async task
                    log.Error($"Caught and ignored exception in async recording of BGP details for {host}: {ex.Message}", ex);
                }
            });
        }

        /// <inheritdoc />
        public void RecordDetailsInDatabase(IHamnetDbHost host, IBgpPeers peers, DateTime queryTime)
        {
            this.recordStore.RecordSuccess(QueryType.BgpQuery, host.Address, null);
        }

        /// <inheritdoc />
        public Task RecordDetailsInDatabaseAsync(IHamnetDbHost host, IBgpPeers peers, DateTime queryTime)
        {
            return Task.Run(() =>
            {
                try
                {
                    this.RecordDetailsInDatabase(host, peers, queryTime);
                }
                catch (Exception ex)
                {
                    // we don not want to throw from an async task
                    log.Error($"Caught and ignored exception in async recording of BGP details for {host.Address} @ {queryTime}: {ex.Message}", ex);
                }
            });
        }

        /// <inheritdoc />
        public void RecordUptimesInDatabase(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData, ILinkDetails linkDetails, IEnumerable<IDeviceSystemData> systemDatas, DateTime queryTime)
        {
            this.recordStore.RecordSuccess(QueryType.RssiQuery, inputData.Value.Select(v => v.Address), inputData.Key.Subnet);
        }

        /// <inheritdoc />
        public Task RecordUptimesInDatabaseAsync(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData, ILinkDetails linkDetails, IEnumerable<IDeviceSystemData> systemDatas, DateTime queryTime)
        {
            return Task.Run(() =>
            {
                try
                {
                    this.RecordUptimesInDatabase(inputData, linkDetails, systemDatas, queryTime);
                }
                catch (Exception ex)
                {
                    // we don not want to throw from an async task
                    log.Error($"Caught and ignored exception in async recording of details for {inputData.Key} @ {queryTime}: {ex.Message}", ex);
                }
            });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            this.Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes off this instance.
        /// </summary>
        /// <param name="disposing"><c>true</c> if called from Dispose method. <c>false</c> if called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                this.disposedValue = true;
            }
        }

        private bool? RandomizeFeasibility(bool? feasible)
        {
            if (!feasible.HasValue)
            {
                return null;
            }

            if (!feasible.Value)
            {
                // no randomness stuff it retry is anyway not feasible
                return false;
            }

            var fiftyPercent = this.randomizer.Next(0, 2) > 0; // 50% probability to really retry

            return feasible.Value && fiftyPercent;
        }

        /// <summary>
        /// The data store for recording the failures.
        /// </summary>
        private class FailureRecordStore
        {
            private static readonly Regex IndentationRegex = new Regex(@"^", RegexOptions.Compiled | RegexOptions.Multiline);

            private readonly Dictionary<QueryType, PerFailureSourceStore> failureSourceLookup = new Dictionary<QueryType, PerFailureSourceStore>();

            private readonly object lockingObject = new object();

            /// <summary>
            /// Initializes the failure record store.
            /// </summary>
            /// <param name="mininumRetryInterval">
            /// The minimum time interval for retries. If less than this after the last try has passed, no retry will be made.
            /// Upon every consecutively failed retry, this interval will be doubled up to <see paramref="maximumRetryInterval" />.
            /// </param>
            /// <param name="maximumRetryInterval">The maximum time interval for retries.</param>
            public FailureRecordStore(TimeSpan mininumRetryInterval, TimeSpan maximumRetryInterval)
            {
                this.MaximumRetryInterval = maximumRetryInterval;
                this.MininumRetryInterval = mininumRetryInterval;
            }

            /// <summary>
            /// Gets the minimum time interval for retries. If less than this after the last try has passed, no retry will be made.
            /// Upon every consecutively failed retry, this interval will be doubled up to <see paramref="maximumRetryInterval" />.
            /// </summary>
            public TimeSpan MininumRetryInterval { get; }

            /// <summary>
            /// Gets the maximum time interval for retries.
            /// </summary>
            public TimeSpan MaximumRetryInterval { get; }

            /// <summary>
            /// Records a failure for the given source and affecting the given address and/or network.
            /// </summary>
            /// <param name="source">The source causing the failure.</param>
            /// <param name="address">The host address affected by the failure. Set to null to only consider network.</param>
            /// <param name="network">The IP network affected by the failure. Set to null to only consider address.</param>
            public void RecordFailure(QueryType source, IPAddress address, IPNetwork network)
            {
                log.Debug($"Recording failure for {source}, IP {address}, net {network}");

                var perFailureSourceStore = this.GetOrCreateStoreForSource(source);
                perFailureSourceStore.RecordFailure(address, network);
            }

            /// <summary>
            /// Records a failure for the given source and affecting the given address and/or network.
            /// </summary>
            /// <param name="source">The source causing the failure.</param>
            /// <param name="addresses">The hosts address affected by the failure. Set to null to only consider network.</param>
            /// <param name="network">The IP network affected by the failure. Set to null to only consider address.</param>
            public void RecordFailure(QueryType source, IEnumerable<IPAddress> addresses, IPNetwork network)
            {
                log.Debug($"Recording failure for {source}, IP {string.Join(", ", addresses)}, net {network}");

                var perFailureSourceStore = this.GetOrCreateStoreForSource(source);

                perFailureSourceStore.RecordFailure(addresses, network);
            }

            /// <summary>
            /// Records a success for the given source and affecting the given address and/or network.
            /// </summary>
            /// <param name="source">The source causing the failure.</param>
            /// <param name="address">The host address affected by the failure. Set to null to only consider network.</param>
            /// <param name="network">The IP network affected by the failure. Set to null to only consider address.</param>
            public void RecordSuccess(QueryType source, IPAddress address, IPNetwork network)
            {
                log.Debug($"Recording success for {source}, IP {address}, net {network}");

                var perFailureSourceStore = this.GetOrCreateStoreForSource(source);
                perFailureSourceStore.RecordSuccess(address, network);
            }

            /// <summary>
            /// Records a success for the given source and affecting the given address and/or network.
            /// </summary>
            /// <param name="source">The source causing the failure.</param>
            /// <param name="addresses">The hosts address affected by the failure. Set to null to only consider network.</param>
            /// <param name="network">The IP network affected by the failure. Set to null to only consider address.</param>
            public void RecordSuccess(QueryType source, IEnumerable<IPAddress> addresses, IPNetwork network)
            {
                log.Debug($"Recording success for {source}, IP {string.Join(", ", addresses)}, net {network}");

                var perFailureSourceStore = this.GetOrCreateStoreForSource(source);

                perFailureSourceStore.RecordSuccess(addresses, network);
            }

            /// <summary>
            /// Checks whether a retry is feasible for the given source and network combination.
            /// </summary>
            /// <param name="source">The source that shall be retried.</param>
            /// <param name="network">The affected network that is being retried.</param>
            /// <returns>
            /// <c>true</c> if a retry is feasible according to the store's settings.
            /// <c>null</c> if no information about the source/network combination is available. It's up to the caller do consider this as retry or no retry.
            /// <c>false</c> if a retry is not yet due according to the store's settings.
            /// </returns>
            public bool? IsRetryFeasible(QueryType source, IPNetwork network)
            {
                var perFailureSourceStore = this.GetOrCreateStoreForSource(source);
                return perFailureSourceStore.IsRetryFeasible(network);
            }

            /// <summary>
            /// Checks whether a retry is feasible for the given source and host address combination.
            /// </summary>
            /// <param name="source">The source that shall be retried.</param>
            /// <param name="address">The affected host address that is being retried.</param>
            /// <returns>
            /// <c>true</c> if a retry is feasible according to the store's settings.
            /// <c>null</c> if no information about the source/network combination is available. It's up to the caller do consider this as retry or no retry.
            /// <c>false</c> if a retry is not yet due according to the store's settings.
            /// </returns>
            public bool? IsRetryFeasible(QueryType source, IPAddress address)
            {
                var perFailureSourceStore = this.GetOrCreateStoreForSource(source);
                return perFailureSourceStore.IsRetryFeasible(address);
            }

            /// <summary>
            /// Checks whether a retry is feasible for the given source and host address _and_ network combination.
            /// </summary>
            /// <param name="source">The source that shall be retried.</param>
            /// <param name="network">The affected network that is being retried.</param>
            /// <param name="address">The affected host address that is being retried.</param>
            /// <returns>
            /// <c>true</c> if a retry is feasible according to the store's settings.
            /// <c>null</c> if no information about the source/network combination is available. It's up to the caller do consider this as retry or no retry.
            /// <c>false</c> if a retry is not yet due according to the store's settings.
            /// </returns>
            public bool? IsRetryFeasible(QueryType source, IPNetwork network, IPAddress address)
            {
                var perFailureSourceStore = this.GetOrCreateStoreForSource(source);
                return perFailureSourceStore.IsRetryFeasible(network, address);
            }

            public override string ToString()
            {
                StringBuilder returnBuilder = new StringBuilder(512);

                if (this.failureSourceLookup.TryGetValue(QueryType.RssiQuery, out PerFailureSourceStore rssiStore) && ((rssiStore?.Count ?? 0) > 0))
                {
                    returnBuilder.AppendLine("Failing RSSI:");
                    returnBuilder.AppendLine(IndentationRegex.Replace(rssiStore.ToString(), "  "));
                }

                if (this.failureSourceLookup.TryGetValue(QueryType.BgpQuery, out PerFailureSourceStore bpgStore) && ((bpgStore?.Count ?? 0) > 0))
                {
                    returnBuilder.AppendLine("Failing BGP:");
                    returnBuilder.AppendLine(IndentationRegex.Replace(bpgStore.ToString(), "  "));
                }

                return returnBuilder.ToString();
            }

            /// <summary>
            /// Queries the details of the given combination of query type, address and network.
            /// </summary>
            /// <param name="source">The source that shall be retried.</param>
            /// <param name="address">The affected host address that is being retried.</param>
            /// <returns>
            /// <c>true</c> if a retry is feasible according to the store's settings.
            /// <c>null</c> if no information about the source/address/network combination is available.
            /// <c>false</c> if a retry is not yet due according to the store's settings.
            /// </returns>
            internal ISingleFailureInfo QueryDetails(QueryType source, IPAddress address)
            {
                var perFailureSourceStore = this.GetOrCreateStoreForSource(source);
                return perFailureSourceStore.QueryDetails(address);
            }

            /// <summary>
            /// Queries the details of the given combination of query type, address and network.
            /// </summary>
            /// <param name="source">The source that shall be retried.</param>
            /// <param name="network">The affected network that is being retried.</param>
            /// <returns>
            /// <c>true</c> if a retry is feasible according to the store's settings.
            /// <c>null</c> if no information about the source/address/network combination is available.
            /// <c>false</c> if a retry is not yet due according to the store's settings.
            /// </returns>
            internal ISingleFailureInfo QueryDetails(QueryType source, IPNetwork network)
            {
                var perFailureSourceStore = this.GetOrCreateStoreForSource(source);
                return perFailureSourceStore.QueryDetails(network);
            }

            /// <summary>
            /// Loads the data for the given query type from the given enumeration of failure infos
            /// </summary>
            /// <param name="queryType">The type of the query that produce the failures.</param>
            /// <param name="singleFailureInfos">The list of failures.</param>
            /// <remarks>
            /// Calling this method will discard all data that might already be stored in the handler.<br/>
            /// Usually it should be called only and exactly once after initializing the object.
            /// </remarks>
            internal void InitializeData(QueryType queryType, IEnumerable<ISingleFailureInfoWithEntity> singleFailureInfos)
            {
                log.Info($"Called for query {queryType}");

                var perFailureSourceStore = this.GetOrCreateStoreForSource(queryType);
                perFailureSourceStore.InitializeData(singleFailureInfos);
            }

            /// <summary>
            /// Get and (if not yet available) create the per-failure-source store for the given failure source.
            /// </summary>
            /// <param name="source">The source causing the failure.</param>
            /// <returns>The store for the given failure source.</returns>
            private PerFailureSourceStore GetOrCreateStoreForSource(QueryType source)
            {
                lock (this.lockingObject)
                {
                    if (!this.failureSourceLookup.TryGetValue(source, out PerFailureSourceStore perFailureSourceStore))
                    {
                        perFailureSourceStore = new PerFailureSourceStore(this.MininumRetryInterval, this.MaximumRetryInterval);
                        this.failureSourceLookup.Add(source, perFailureSourceStore);
                    }

                    return perFailureSourceStore;
                }
            }

            /// <summary>
            /// Container for a failure store for a single source.
            /// </summary>
            private class PerFailureSourceStore
            {
                private readonly TimeSpan mininumRetryInterval;

                private readonly TimeSpan maximumRetryInterval;

                private readonly Dictionary<IPAddress, SingleFailureInfo> hostFailureInfos = new Dictionary<IPAddress, SingleFailureInfo>();

                private readonly Dictionary<IPNetwork, SingleFailureInfo> networkFailureInfos = new Dictionary<IPNetwork, SingleFailureInfo>();

                private readonly object lockingObject = new object();

                /// <summary>
                /// Gets the total number of entries (host + net) in this store.
                /// </summary>
                public int Count => this.hostFailureInfos.Count + this.networkFailureInfos.Count;

                /// <summary>
                /// Initializes the per-failure record store.
                /// </summary>
                /// <param name="mininumRetryInterval">
                /// The minimum time interval for retries. If less than this after the last try has passed, no retry will be made.
                /// Upon every consecutively failed retry, this interval will be doubled up to <see paramref="maximumRetryInterval" />.
                /// </param>
                /// <param name="maximumRetryInterval">The maximum time interval for retries.</param>
                public PerFailureSourceStore(TimeSpan mininumRetryInterval, TimeSpan maximumRetryInterval)
                {
                    this.mininumRetryInterval = mininumRetryInterval;
                    this.maximumRetryInterval = maximumRetryInterval;
                }

                /// <summary>
                /// Records a failure affecting the given address and/or network.
                /// </summary>
                /// <param name="address">The host address affected by the failure. Set to null to only consider network.</param>
                /// <param name="network">The IP network affected by the failure. Set to null to only consider address.</param>
                public void RecordFailure(IPAddress address, IPNetwork network)
                {
                    lock (this.lockingObject)
                    {
                        this.RecordFailingAddress(address);
                        this.RecordFailingNetwork(network);
                    }
                }

                /// <summary>
                /// Records a failure affecting the given address and/or network.
                /// </summary>
                /// <param name="addresses">The hosts address affected by the failure. Set to null to only consider network.</param>
                /// <param name="network">The IP network affected by the failure. Set to null to only consider address.</param>
                public void RecordFailure(IEnumerable<IPAddress> addresses, IPNetwork network)
                {
                    lock (this.lockingObject)
                    {
                        this.RecordFailingNetwork(network);
                        foreach (var address in addresses)
                        {
                            this.RecordFailingAddress(address);
                        }
                    }
                }

                /// <summary>
                /// Records a success affecting the given address and/or network.
                /// </summary>
                /// <param name="address">The host address affected by the failure. Set to null to only consider network.</param>
                /// <param name="network">The IP network affected by the failure. Set to null to only consider address.</param>
                public void RecordSuccess(IPAddress address, IPNetwork network)
                {
                    lock (this.lockingObject)
                    {
                        this.DeleteHostFailure(address);
                        this.DeleteNetworkFailure(network);
                    }
                }

                /// <summary>
                /// Records a success affecting the given address and/or network.
                /// </summary>
                /// <param name="addresses">The hosts address affected by the failure. Set to null to only consider network.</param>
                /// <param name="network">The IP network affected by the failure. Set to null to only consider address.</param>
                public void RecordSuccess(IEnumerable<IPAddress> addresses, IPNetwork network)
                {
                    lock (this.lockingObject)
                    {
                        this.DeleteNetworkFailure(network);
                        foreach (var address in addresses)
                        {
                            this.DeleteHostFailure(address);
                        }
                    }
                }

                /// <summary>
                /// Checks whether a retry is feasible for the given source and network combination.
                /// </summary>
                /// <param name="network">The affected network that is being retried.</param>
                /// <returns>
                /// <c>true</c> if a retry is feasible according to the store's settings.
                /// <c>null</c> if no information about the source/network combination is available. It's up to the caller do consider this as retry or no retry.
                /// <c>false</c> if a retry is not yet due according to the store's settings.
                /// </returns>
                public bool? IsRetryFeasible(IPNetwork network)
                {
                    lock (this.lockingObject)
                    {
                        if (!this.networkFailureInfos.TryGetValue(network, out SingleFailureInfo failureInfo))
                        {
                            return null;
                        }

                        var isFeasible = failureInfo.IsRetryFeasible;
#if DEBUG
                        log.Info($"IsRetryFeasible({network}) = {isFeasible}");
#endif

                        return isFeasible;
                    }
                }

                /// <summary>
                /// Checks whether a retry is feasible for the given host address.
                /// </summary>
                /// <param name="address">The affected host address that is being retried.</param>
                /// <returns>
                /// <c>true</c> if a retry is feasible according to the store's settings.
                /// <c>null</c> if no information about the source/network combination is available. It's up to the caller do consider this as retry or no retry.
                /// <c>false</c> if a retry is not yet due according to the store's settings.
                /// </returns>
                public bool? IsRetryFeasible(IPAddress address)
                {
                    lock (this.lockingObject)
                    {
                        if (!this.hostFailureInfos.TryGetValue(address, out SingleFailureInfo failureInfo))
                        {
                            return null;
                        }

                        var isFeasible = failureInfo.IsRetryFeasible;
#if DEBUG
                        log.Info($"IsRetryFeasible({address}) = {isFeasible}");
#endif

                        return isFeasible;
                    }
                }

                /// <summary>
                /// Checks whether a retry is feasible for the given host address _and_ network combination.
                /// </summary>
                /// <param name="network">The affected network that is being retried.</param>
                /// <param name="address">The affected host address that is being retried.</param>
                /// <returns>
                /// <c>true</c> if a retry is feasible according to the store's settings.
                /// <c>null</c> if no information about the source/network combination is available. It's up to the caller do consider this as retry or no retry.
                /// <c>false</c> if a retry is not yet due according to the store's settings.
                /// </returns>
                public bool? IsRetryFeasible(IPNetwork network, IPAddress address)
                {
                    lock (this.lockingObject)
                    {
                        if (!this.networkFailureInfos.TryGetValue(network, out SingleFailureInfo networkFailureInfo))
                        {
                            return null;
                        }

                        if (!this.hostFailureInfos.TryGetValue(address, out SingleFailureInfo hostFailureInfo))
                        {
                            return null;
                        }

                        var isFeasible = hostFailureInfo.IsRetryFeasible && networkFailureInfo.IsRetryFeasible;

#if DEBUG
                        log.Info($"IsRetryFeasible({network} && {address}) = {isFeasible}");
#endif

                        return isFeasible;
                    }
                }

                public override string ToString()
                {
                    StringBuilder returnBuilder = new StringBuilder(512);

                    if (this.hostFailureInfos.Count > 0)
                    {
                        returnBuilder.AppendLine("Failing hosts:");
                        foreach (var item in this.hostFailureInfos)
                        {
                            returnBuilder.Append(" - ").Append(item.Key).Append(": ").AppendLine(item.Value.ToString());
                        }
                    }

                    if (this.networkFailureInfos.Count > 0)
                    {
                        returnBuilder.AppendLine("Failing networks:");
                        foreach (var item in this.networkFailureInfos)
                        {
                            returnBuilder.Append(" - ").Append(item.Key).Append(": ").AppendLine(item.Value.ToString());
                        }
                    }

                    return returnBuilder.ToString();
                }

                /// <summary>
                /// Queries the details of the given combination of query type, address and network.
                /// </summary>
                /// <param name="address">The affected host address that is being retried.</param>
                /// <returns>
                /// <c>true</c> if a retry is feasible according to the store's settings.
                /// <c>null</c> if no information about the source/address/network combination is available.
                /// <c>false</c> if a retry is not yet due according to the store's settings.
                /// </returns>
                internal ISingleFailureInfo QueryDetails(IPAddress address)
                {
                    if (address == null)
                    {
                        return null;
                    }

                    lock (this.lockingObject)
                    {
                        if (!this.hostFailureInfos.TryGetValue(address, out SingleFailureInfo failureInfo))
                        {
                           return null;
                        }

                        return failureInfo;
                    }
                }

                /// <summary>
                /// Queries the details of the given combination of query type, address and network.
                /// </summary>
                /// <param name="network">The affected network that is being retried.</param>
                /// <returns>
                /// <c>true</c> if a retry is feasible according to the store's settings.
                /// <c>null</c> if no information about the source/address/network combination is available.
                /// <c>false</c> if a retry is not yet due according to the store's settings.
                /// </returns>
                internal ISingleFailureInfo QueryDetails(IPNetwork network)
                {
                    if (network == null)
                    {
                        return null;
                    }

                    lock (this.lockingObject)
                    {
                        if (!this.networkFailureInfos.TryGetValue(network, out SingleFailureInfo failureInfo))
                        {
                           return null;
                        }

                        return failureInfo;
                    }
                }


                /// <summary>
                /// Loads the data for the given query type from the given enumeration of failure infos
                /// </summary>
                /// <param name="singleFailureInfos">The list of failures.</param>
                /// <remarks>
                /// Calling this method will discard all data that might already be stored in the handler.<br/>
                /// Usually it should be called only and exactly once after initializing the object.
                /// </remarks>
                internal void InitializeData(IEnumerable<ISingleFailureInfoWithEntity> singleFailureInfos)
                {
                    if ((singleFailureInfos == null) || !singleFailureInfos.Any())
                    {
                        log.Warn("singleFailureInfos is null or an empty enumeration: This is a NOP. Not initializing anything.");
                        return;
                    }

                    lock(this.lockingObject)
                    {
                        foreach (ISingleFailureInfoWithEntity item in singleFailureInfos)
                        {
                            switch (item.EntityType)
                            {
                                case EntityType.Host:
                                    if (!IPAddress.TryParse(item.Entity, out IPAddress ipAddress))
                                    {
                                        log.Error($"Cannot convert entity '{item.Entity}' of type {item.EntityType} into an IPAddress of the host");
                                        break;
                                    }

                                    this.hostFailureInfos[ipAddress] = new SingleFailureInfo(this.mininumRetryInterval, this.maximumRetryInterval, item);

                                    break;

                                case EntityType.Subnet:
                                    if (!IPNetwork.TryParse(item.Entity, out IPNetwork ipNetwork))
                                    {
                                        log.Error($"Cannot convert entity '{item.Entity}' of type {item.EntityType} into an IPNetwork of the subnet");
                                        break;
                                    }

                                    this.networkFailureInfos[ipNetwork] = new SingleFailureInfo(this.mininumRetryInterval, this.maximumRetryInterval, item);

                                    break;

                                default:
                                    log.Error($"Unknown entity type {item.EntityType} (entity string '{item.Entity}') for failure info will be ignored");
                                    break;
                            }
                        }
                    }
                }

                private void DeleteHostFailure(IPAddress address)
                {
                    if (address == null)
                    {
                        return;
                    }

                    lock (this.lockingObject)
                    {
                        var removed = this.hostFailureInfos.Remove(address);

                        if (removed)
                        {
                            log.Info($"DeleteHostFailure({address}): {(removed ? "done" : "not present")}");
                        }
                    }
                }

                private void DeleteNetworkFailure(IPNetwork network)
                {
                    if (network == null)
                    {
                        return;
                    }

                    lock (this.lockingObject)
                    {
                        var removed = this.networkFailureInfos.Remove(network);

                        if (removed)
                        {
                            log.Info($"DeleteNetworkFailure({network}): {(removed ? "done" : "not present")}");
                        }
                    }
                }

                private void RecordFailingNetwork(IPNetwork network)
                {
                    if (network == null)
                    {
                        return;
                    }

                    lock (this.lockingObject)
                    {
                        if (!this.networkFailureInfos.TryGetValue(network, out SingleFailureInfo failureInfo))
                        {
                            failureInfo = new SingleFailureInfo(this.mininumRetryInterval, this.maximumRetryInterval);

                            log.Debug($"Recorded failure for network {network}");

                            this.networkFailureInfos.Add(network, failureInfo);
                        }

                        failureInfo.RecordNewOccurance();
                    }
                }

                private void RecordFailingAddress(IPAddress address)
                {
                    if (address == null)
                    {
                        return;
                    }

                    lock (this.lockingObject)
                    {
                        if (!this.hostFailureInfos.TryGetValue(address, out SingleFailureInfo failureInfo))
                        {
                            failureInfo = new SingleFailureInfo(this.mininumRetryInterval, this.maximumRetryInterval);

                            log.Debug($"Recorded failure for host {address}");

                            this.hostFailureInfos.Add(address, failureInfo);
                        }

                        failureInfo.RecordNewOccurance();
                    }
                }

                /// <summary>
                /// Class for the info about a single failure.
                /// </summary>
                private class SingleFailureInfo : ISingleFailureInfo
                {
                    /// <summary>
                    /// Object for locking against concurrent access to ensure consistent setting of all data.
                    /// </summary>
                    private readonly object lockingObject = new object();

                    private readonly TimeSpan maximumRetryInterval;

                    private DateTime firsOccurance = DateTime.MinValue;

                    private DateTime lastOccurance = DateTime.MinValue;

                    private uint occuranceCount = 0;

                    private TimeSpan currentRetryInterval;

                    /// <summary>
                    /// Creates an instance for the given retry intervals.
                    /// </summary>
                    /// <param name="mininumRetryInterval">
                    /// The minimum time interval for retries. If less than this after the last try has passed, no retry will be made.
                    /// Upon every consecutively failed retry, this interval will be doubled up to <see paramref="maximumRetryInterval" />.
                    /// </param>
                    /// <param name="maximumRetryInterval">The maximum time interval for retries.</param>
                    /// <param name="item">The item to copy the data from.</param>
                    public SingleFailureInfo(TimeSpan mininumRetryInterval, TimeSpan maximumRetryInterval, ISingleFailureInfo item)
                    {
                        this.currentRetryInterval = mininumRetryInterval;
                        this.maximumRetryInterval = maximumRetryInterval;
                        this.firsOccurance = item.FirsOccurance;
                        this.lastOccurance = item.LastOccurance;
                        this.occuranceCount = item.OccuranceCount;

                        // for retry intefval we need to check the maximum given to us. it could have changed (and hence lowered) if restarted.
                        this.currentRetryInterval = item.CurrentPenalty <= maximumRetryInterval ? item.CurrentPenalty : maximumRetryInterval;
                    }

                    /// <summary>
                    /// Creates an instance for the given retry intervals.
                    /// </summary>
                    /// <param name="mininumRetryInterval">
                    /// The minimum time interval for retries. If less than this after the last try has passed, no retry will be made.
                    /// Upon every consecutively failed retry, this interval will be doubled up to <see paramref="maximumRetryInterval" />.
                    /// </param>
                    /// <param name="maximumRetryInterval">The maximum time interval for retries.</param>
                    public SingleFailureInfo(TimeSpan mininumRetryInterval, TimeSpan maximumRetryInterval)
                    {
                        this.currentRetryInterval = mininumRetryInterval;
                        this.maximumRetryInterval = maximumRetryInterval;
                    }

                    /// <inheritdoc />
                    [JsonProperty(Order = 1)]
                    public uint OccuranceCount
                    {
                        get
                        {
                            lock (this.lockingObject)
                            {
                                return this.occuranceCount;
                            }
                        }
                    }

                    /// <inheritdoc />
                    [JsonProperty(Order = 3)]
                    public DateTime LastOccurance
                    {
                        get
                        {
                            lock (this.lockingObject)
                            {
                                return this.lastOccurance;
                            }
                        }
                    }

                    /// <inheritdoc />
                    [JsonProperty(Order = 2)]
                    public DateTime FirsOccurance
                    {
                        get
                        {
                            lock (this.lockingObject)
                            {
                                return this.firsOccurance;
                            }
                        }
                    }

                    /// <inheritdoc />
                    [JsonProperty(Order = 5)]
                    public bool IsRetryFeasible
                    {
                        get
                        {
                            lock (this.lockingObject)
                            {
                                var retryFeasible = ((DateTime.UtcNow - this.lastOccurance) > this.currentRetryInterval);

#if DEBUG
                                log.Info($"IsRetryFeasible({this}): {retryFeasible}");
#endif

                                return retryFeasible;
                            }
                        }
                    }

                    /// <inheritdoc />
                    [JsonProperty(Order = 4)]
                    public TimeSpan CurrentPenalty => this.currentRetryInterval;

                    /// <summary>
                    /// Records a new occurance of this specific failure.
                    /// </summary>
                    public void RecordNewOccurance()
                    {
                        lock (this.lockingObject)
                        {
                            ++this.occuranceCount;

                            var nowItIs = DateTime.UtcNow;
                            this.lastOccurance = nowItIs;
                            if (this.firsOccurance == DateTime.MinValue)
                            {
                                this.firsOccurance = nowItIs;
                            }

                            // double the retry interval on every occurance
                            // but make sure it's at most maximumRetryInterval
                            this.currentRetryInterval *= 2;
                            if (this.currentRetryInterval > this.maximumRetryInterval)
                            {
                                this.currentRetryInterval = this.maximumRetryInterval;
                            }

                            log.Debug($"Recorded failure: New values: {this}");
                        }
                    }

                    public override string ToString()
                    {
                        return $"occurance count = {this.occuranceCount}, retry interval = {this.currentRetryInterval}, first occurance = {this.firsOccurance}, last occurance = {this.lastOccurance}";
                    }
                }
            }
        }
    }
}