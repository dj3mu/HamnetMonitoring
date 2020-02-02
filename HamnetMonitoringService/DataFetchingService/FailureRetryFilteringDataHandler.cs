#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HamnetDbAbstraction;
using HamnetDbRest;
using Microsoft.Extensions.Configuration;
using SnmpAbstraction;

namespace RestService.DataFetchingService
{
    /// <summary>
    /// Implementation of an <see cref="IAquiredDataHandler" /> that records failures and allows filtering
    /// based on the failures and the time and how often they have last been seen.
    /// </summary>
    internal class FailureRetryFilteringDataHandler : IAquiredDataHandler
    {
        private static readonly log4net.ILog log = Program.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            this.recordStore = new FailureRecordStore(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(120));
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~InfluxDatabaseDataHandler()
        // {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        /// <inheritdoc />
        public string Name { get; } = "Failure Retry Filtering";

        /// <inheritdoc />
        public void AquisitionFinished()
        {
            // NOP for now
        }

        /// <inheritdoc />
        public void PrepareForNewAquisition()
        {
            // NOP for now
        }

        /// <inheritdoc />
        public void RecordRssiDetailsInDatabase(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData, ILinkDetails linkDetails, DateTime queryTime)
        {
            this.recordStore.RecordSuccess(FailureSource.RssiQuery, inputData.Value.Select(v => v.Address), inputData.Key.Subnet);
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
            this.recordStore.RecordFailure(FailureSource.RssiQuery, inputData.Value.Select(v => v.Address), inputData.Key.Subnet);
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
            this.recordStore.RecordFailure(FailureSource.BgpQuery, host.Address, null);
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
            this.recordStore.RecordSuccess(FailureSource.BgpQuery, host.Address, null);
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
            this.recordStore.RecordSuccess(FailureSource.RssiQuery, inputData.Value.Select(v => v.Address), inputData.Key.Subnet);
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

        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
            // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            // GC.SuppressFinalize(this);
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

        /// <summary>
        /// Enumeration of failure sources (entry point for data store).
        /// </summary>
        private enum FailureSource
        {
            RssiQuery,

            BgpQuery
        }

        /// <summary>
        /// The data store for recording the failures.
        /// </summary>
        private class FailureRecordStore
        {
            private Dictionary<FailureSource, PerFailureSourceStore> failureSourceLookup = new Dictionary<FailureSource, PerFailureSourceStore>();

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
            public void RecordFailure(FailureSource source, IPAddress address, IPNetwork network)
            {
                var perFailureSourceStore = this.GetOrCreateStoreForSource(source);
                perFailureSourceStore.RecordFailure(address, network);
            }

            /// <summary>
            /// Records a failure for the given source and affecting the given address and/or network.
            /// </summary>
            /// <param name="source">The source causing the failure.</param>
            /// <param name="addresses">The hosts address affected by the failure. Set to null to only consider network.</param>
            /// <param name="network">The IP network affected by the failure. Set to null to only consider address.</param>
            public void RecordFailure(FailureSource source, IEnumerable<IPAddress> addresses, IPNetwork network)
            {
                var perFailureSourceStore = this.GetOrCreateStoreForSource(source);

                foreach (var address in addresses)
                {
                    perFailureSourceStore.RecordFailure(address, network);
                }
            }

            /// <summary>
            /// Records a success for the given source and affecting the given address and/or network.
            /// </summary>
            /// <param name="source">The source causing the failure.</param>
            /// <param name="address">The host address affected by the failure. Set to null to only consider network.</param>
            /// <param name="network">The IP network affected by the failure. Set to null to only consider address.</param>
            public void RecordSuccess(FailureSource source, IPAddress address, IPNetwork network)
            {
                var perFailureSourceStore = this.GetOrCreateStoreForSource(source);
                perFailureSourceStore.RecordSuccess(address, network);
            }

            /// <summary>
            /// Records a success for the given source and affecting the given address and/or network.
            /// </summary>
            /// <param name="source">The source causing the failure.</param>
            /// <param name="addresses">The hosts address affected by the failure. Set to null to only consider network.</param>
            /// <param name="network">The IP network affected by the failure. Set to null to only consider address.</param>
            public void RecordSuccess(FailureSource source, IEnumerable<IPAddress> addresses, IPNetwork network)
            {
                var perFailureSourceStore = this.GetOrCreateStoreForSource(source);

                foreach (var address in addresses)
                {
                    perFailureSourceStore.RecordSuccess(address, network);
                }
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
            public bool? IsRetryFeasible(FailureSource source, IPNetwork network)
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
            public bool? IsRetryFeasible(FailureSource source, IPAddress address)
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
            public bool? IsRetryFeasible(FailureSource source, IPNetwork network, IPAddress address)
            {
                var perFailureSourceStore = this.GetOrCreateStoreForSource(source);
                return perFailureSourceStore.IsRetryFeasible(network, address);
            }

            /// <summary>
            /// Get and (if not yet available) create the per-failure-source store for the given failure source.
            /// </summary>
            /// <param name="source">The source causing the failure.</param>
            /// <returns>The store for the given failure source.</returns>
            private PerFailureSourceStore GetOrCreateStoreForSource(FailureSource source)
            {
                if (!this.failureSourceLookup.TryGetValue(source, out PerFailureSourceStore perFailureSourceStore))
                {
                    perFailureSourceStore = new PerFailureSourceStore(this.MininumRetryInterval, this.MaximumRetryInterval);
                    this.failureSourceLookup.Add(source, perFailureSourceStore);
                }

                return perFailureSourceStore;
            }

            /// <summary>
            /// Container for a failure store for a single source.
            /// </summary>
            private class PerFailureSourceStore
            {
                private TimeSpan mininumRetryInterval;

                private TimeSpan maximumRetryInterval;

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
                    throw new NotImplementedException();
                }

                /// <summary>
                /// Records a failure affecting the given address and/or network.
                /// </summary>
                /// <param name="addresses">The hosts address affected by the failure. Set to null to only consider network.</param>
                /// <param name="network">The IP network affected by the failure. Set to null to only consider address.</param>
                public void RecordFailure(IEnumerable<IPAddress> addresses, IPNetwork network)
                {
                    foreach (var address in addresses)
                    {
                        this.RecordFailure(address, network);
                    }
                }

                /// <summary>
                /// Records a success affecting the given address and/or network.
                /// </summary>
                /// <param name="address">The host address affected by the failure. Set to null to only consider network.</param>
                /// <param name="network">The IP network affected by the failure. Set to null to only consider address.</param>
                public void RecordSuccess(IPAddress address, IPNetwork network)
                {
                    throw new NotImplementedException();
                }

                /// <summary>
                /// Records a success affecting the given address and/or network.
                /// </summary>
                /// <param name="addresses">The hosts address affected by the failure. Set to null to only consider network.</param>
                /// <param name="network">The IP network affected by the failure. Set to null to only consider address.</param>
                public void RecordSuccess(IEnumerable<IPAddress> addresses, IPNetwork network)
                {
                    foreach (var address in addresses)
                    {
                        this.RecordSuccess(address, network);
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
                    throw new NotImplementedException();
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
                    throw new NotImplementedException();
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
                    throw new NotImplementedException();
                }
            }
        }
    }
}