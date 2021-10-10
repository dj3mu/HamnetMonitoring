using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HamnetDbAbstraction;
using HamnetDbRest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestService.Database;
using SnmpAbstraction;
using SnmpSharpNet;

namespace RestService.DataFetchingService
{
    /// <summary>
    /// Hosted service to regularly retrieve the RSSI data to be reported via REST api.
    /// </summary>
    public class RssiAquisitionService : IHostedService, IDisposable
    {
        private static readonly TimeSpan Hysteresis = TimeSpan.FromSeconds(10);

        /// <summary>
        /// The list of receivers for the data that we aquired.
        /// </summary>
        private readonly List<IAquiredDataHandler> dataHandlers = new List<IAquiredDataHandler>();

        private readonly ILogger<RssiAquisitionService> logger;

        private readonly IConfiguration configuration;

        private readonly IFailureRetryFilteringDataHandler retryFeasibleHandler;

        private readonly Mutex rssiMutex = new Mutex(false, Program.RssiRunningMutexName);

        private readonly object multiTimerLockingObject = new object();

        private readonly object databaseLockingObject = new object();

        private HamnetDbPoller hamnetDbPoller;

        private bool disposedValue = false;

        private Timer timer;

        private QuerierOptions snmpQuerierOptions = QuerierOptions.Default;

        private TimeSpan refreshInterval;

        private bool usePenaltySystem = false;

        private int maxParallelQueries;
        /// <summary>
        /// Constructs taking the logger.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The service configuration.</param>
        /// <param name="hamnetDbAccess">The singleton instance of the HamnetDB access handler.</param>
        /// <param name="retryFeasibleHandler">The handler to check whether retry is feasible.</param>
        public RssiAquisitionService(ILogger<RssiAquisitionService> logger, IConfiguration configuration, IHamnetDbAccess hamnetDbAccess, IFailureRetryFilteringDataHandler retryFeasibleHandler)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "The logger has not been provided by DI engine");
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "The configuration has not been provided by DI engine");
            this.retryFeasibleHandler = retryFeasibleHandler ?? throw new ArgumentNullException(nameof(retryFeasibleHandler), "The retry feasible handler singleton has not been provided by DI engine");

            this.dataHandlers.Add(this.retryFeasibleHandler);
            this.dataHandlers.Add(new ResultDatabaseDataHandler(configuration, this.retryFeasibleHandler));

            this.hamnetDbPoller = new HamnetDbPoller(this.configuration, hamnetDbAccess ?? throw new ArgumentNullException(nameof(hamnetDbAccess), "The HamnetDB accessor singleton has not been provided by DI engine"));

            IConfigurationSection influxSection = configuration.GetSection(Program.InfluxSectionKey);
            if ((influxSection != null) && influxSection.GetChildren().Any())
            {
                this.dataHandlers.Add(new InfluxDatabaseDataHandler(configuration, this.hamnetDbPoller));
            }
            else
            {
                this.logger.LogInformation($"Influx database disabled: No or empty '{Program.InfluxSectionKey}' section in configuration");
            }

            this.dataHandlers = this.dataHandlers.OrderBy(h => h.Name).ToList();
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~DataFetchingService()
        // {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            IConfigurationSection aquisisionServiceSection = this.configuration.GetSection(Program.RssiAquisitionServiceSectionKey);

            this.usePenaltySystem = aquisisionServiceSection.GetValue<bool>(Program.PenaltySystemEnablingKey);

            this.logger.LogInformation($"Penality system for RSSI aquisition: {(this.usePenaltySystem ? "enabled" : "disabled")}");

            // configure thread pool for number of parallel queries
            this.maxParallelQueries = aquisisionServiceSection.GetValue<int>("MaximumParallelQueries");
            if (this.maxParallelQueries == 0)
            {
                this.maxParallelQueries = Environment.ProcessorCount;
            }

            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minEaThreads);
            ThreadPool.GetMaxThreads(out int maxworkerThreads, out int maxEaThreads);

            this.logger.LogInformation($"Thread pool: Found: Min workers = {minWorkerThreads}, Max workers = {maxworkerThreads}, Min EA {minEaThreads}, Max EA {maxEaThreads}");

            this.refreshInterval = TimeSpan.Parse(aquisisionServiceSection.GetValue<string>("RefreshInterval"));

            var snmpVersion = aquisisionServiceSection.GetValue<int>("SnmpVersion");
            if (snmpVersion != 0)
            {
                // 0 is the default value set by config framework and doesn't make any sense here - so we can use it to identify a missing config
                this.snmpQuerierOptions = this.snmpQuerierOptions.WithProtocolVersion(snmpVersion.ToSnmpVersion());
            }

            var snmpTimeoutConfig = this.configuration.GetSection(Program.RssiAquisitionServiceSectionKey).GetValue<int>("SnmpTimeoutSeconds");
            if (snmpTimeoutConfig != 0)
            {
                // 0 is the default value set by config framework and doesn't make any sense here - so we can use it to identify a missing config
                this.snmpQuerierOptions = this.snmpQuerierOptions.WithTimeout(TimeSpan.FromSeconds(snmpTimeoutConfig));
            }

            var snmpRetriesConfig = aquisisionServiceSection.GetValue<int>("SnmpRetries");
            if (snmpRetriesConfig != 0)
            {
                // 0 is the default value set by config framework and doesn't make any sense here - so we can use it to identify a missing config
                this.snmpQuerierOptions = this.snmpQuerierOptions.WithRetries(snmpRetriesConfig);
            }

            this.snmpQuerierOptions = this.snmpQuerierOptions.WithCaching(aquisisionServiceSection.GetValue<bool>("UseQueryCaching"));

            var hamnetDbConfig = this.configuration.GetSection(HamnetDbProvider.HamnetDbSectionName);

            // by default waiting a couple of secs before first Hamnet scan
            TimeSpan timeToFirstAquisition = TimeSpan.FromSeconds(11);

            using var resultDatabase = QueryResultDatabaseProvider.Instance.CreateContext();

            var status = resultDatabase.Status;
            var nowItIs = DateTime.UtcNow;
            var timeSinceLastAquisitionStart = (nowItIs - status.LastRssiQueryStart);
            if (status.LastRssiQueryStart > status.LastRssiQueryEnd)
            {
                this.logger.LogInformation($"STARTING first RSSI aquisition immediately: Last aquisition started {status.LastRssiQueryStart} seems not to have ended successfully (last end time {status.LastRssiQueryEnd})");
            }
            else if (timeSinceLastAquisitionStart < this.refreshInterval)
            {
                // no aquisition required yet (e.g. service restart)
                timeToFirstAquisition = this.refreshInterval - timeSinceLastAquisitionStart;
            }

            this.logger.LogInformation($"STARTING first RSSI aquisition after restart in {timeToFirstAquisition}: Last aquisition started {status.LastRssiQueryStart}, configured interval {this.refreshInterval}");

            this.timer = new Timer(DoFetchData, null, timeToFirstAquisition, this.refreshInterval);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Timed RSSI data fetching service is stopping.");

            this.timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Disposes off this instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // TODO: Uncomment if finalized is implemented above.
            // GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes off this instance.
        /// </summary>
        /// <param name="disposing">Indication whether called from finalizer or Dispose().</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.timer?.Dispose();

                    foreach (var item in this.dataHandlers)
                    {
                        item?.Dispose();
                    }

                    this.dataHandlers.Clear();

                    this.hamnetDbPoller?.Dispose();
                    this.hamnetDbPoller = null;
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// The asynchronuous method that is called by the timer to execute the periodic data aquisiation.
        /// </summary>
        /// <param name="state">Required by timer but not used. Using field <see cref="configuration" /> instead.</param>
        private void DoFetchData(object state)
        {
            // NOTE: The Monitor handles multiple concurrent runs while the mutex prevents running of aquisition and maintenance at the same time.
            if (Monitor.TryEnter(this.multiTimerLockingObject))
            {
                try
                {
                    this.rssiMutex.WaitOne();
                    Program.ProgramWideAquisitionSemaphore.WaitOne();

                    this.PerformDataAquisition();
                }
                catch(Exception ex)
                {
                    this.logger.LogError($"Excpetion caught and ignored in timer-called data aquisition thread: {ex.ToString()}");
                }
                finally
                {
                    Program.ProgramWideAquisitionSemaphore.Release();
                    this.rssiMutex.ReleaseMutex();

                    Monitor.Exit(this.multiTimerLockingObject);

                    GC.Collect(); // free as much memory as we can
                }
            }
            else
            {
                this.logger.LogError("SKIPPING RSSI data aquisition: Previous aquisition still ongoing. Please adjust interval.");
            }
        }

        /// <summary>
        /// Performs the actual data aquisition.
        /// </summary>
        private void PerformDataAquisition()
        {
            IConfigurationSection hamnetDbConfig = this.configuration.GetSection(Program.RssiAquisitionServiceSectionKey);

            using var resultDatabase = QueryResultDatabaseProvider.Instance.CreateContext();

            // detect if we're due to run and, if we are, record the start of the run
            using (var transaction = resultDatabase.Database.BeginTransaction())
            {
                var status = resultDatabase.Status;
                var nowItIs = DateTime.UtcNow;
                var sinceLastScan = nowItIs - status.LastRssiQueryStart;
                if ((sinceLastScan < this.refreshInterval - Hysteresis) && (status.LastRssiQueryStart <= status.LastRssiQueryEnd))
                {
                    this.logger.LogInformation($"SKIPPING: RSSI aquisition not yet due: Last aquisition started {status.LastRssiQueryStart} ({sinceLastScan} ago, hysteresis {Hysteresis}), configured interval {this.refreshInterval}");
                    return;
                }

                // we restart the timer so that in case we've been blocked by Mutexes etc. the interval really starts from scratch
                this.timer.Change(this.refreshInterval, this.refreshInterval);

                this.logger.LogInformation($"STARTING: Retrieving RSSI monitoring data as configured in HamnetDB - last run: Started {status.LastRssiQueryStart} ({sinceLastScan} ago)");

                status.LastRssiQueryStart = DateTime.UtcNow;

                resultDatabase.SaveChanges();
                transaction.Commit();
            }

            Program.RequestStatistics.RssiPollings++;

            Dictionary<IHamnetDbSubnet, IHamnetDbHosts> pairsSlicedAccordingToConfiguration = this.hamnetDbPoller.FetchSubnetsWithHostsFromHamnetDb();

            this.logger.LogDebug($"SNMP querying {pairsSlicedAccordingToConfiguration.Count} entries");

            this.SendPrepareToDataHandlers();

            NetworkExcludeFile excludes = new NetworkExcludeFile(hamnetDbConfig);
            var excludeNets = excludes?.ParsedNetworks?.ToList();

            this.logger.LogDebug($"Launching {this.maxParallelQueries} parallel RSSI aquisition threads");

            Parallel.ForEach(pairsSlicedAccordingToConfiguration, new ParallelOptions { MaxDegreeOfParallelism = this.maxParallelQueries },
            pair =>
            {
                if ((excludeNets != null) && (excludeNets.Any(exclude => exclude.Equals(pair.Key.Subnet) || exclude.Contains(pair.Key.Subnet))))
                {
                    this.logger.LogInformation($"Skipping subnet {pair.Key.Subnet} due to exclude list");
                    return;
                }

                try
                {
                    this.QueryLinkOfSingleSubnet(pair);
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Exception caught and ignored in RSSI arallel data aquisition thread: {ex.ToString()}");
                }
            });

            this.SendFinishedToDataHandlers();

            // record the regular end of the run
            using (var transaction = resultDatabase.Database.BeginTransaction())
            {
                var status = resultDatabase.Status;

                status.LastRssiQueryEnd = DateTime.UtcNow;

                this.logger.LogInformation($"COMPLETED: Retrieving RSSI monitoring data as configured in HamnetDB at {status.LastRssiQueryEnd}, duration {status.LastRssiQueryEnd - status.LastRssiQueryStart}");

                resultDatabase.SaveChanges();
                transaction.Commit();
            }
        }

        /// <summary>
        /// Queries the link for a single subnet.
        /// </summary>
        /// <param name="pair">The pair of hosts inside the subnet to query.</param>
        private void QueryLinkOfSingleSubnet(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> pair)
        {
            IPAddress address1 = pair.Value.First().Address;
            IPAddress address2 = pair.Value.Last().Address;

            if (this.usePenaltySystem && !this.retryFeasibleHandler.IsRetryFeasible(QueryType.RssiQuery, pair.Key.Subnet).GetValueOrDefault(true))
            {
                this.logger.LogInformation($"Skipping link details for pair {address1} <-> {address2} of subnet {pair.Key.Subnet}: Retry not yet due.");
                return;
            }

            this.logger.LogInformation($"Querying link details for pair {address1} <-> {address2} of subnet {pair.Key.Subnet}");

            Exception hitException = null;
            try
            {
                using var querier = SnmpQuerierFactory.Instance.Create(address1, this.snmpQuerierOptions);
                using var querier2 = SnmpQuerierFactory.Instance.Create(address2, this.snmpQuerierOptions);
                // NOTE: Do not Dispose the querier until ALL data has been copied to other containers!
                //       Else the lazy-loading containers might fail to lazy-query the required values.

                var linkDetails = querier.FetchLinkDetails(querier2);

                var storeOnlyDetailsClone = new LinkDetailsStoreOnlyContainer(linkDetails);

                this.SendResultsToDataHandlers(pair, storeOnlyDetailsClone, new IDeviceSystemData[] { new SystemDataStoreOnlyContainer(querier.SystemData), new SystemDataStoreOnlyContainer(querier2.SystemData) }, DateTime.UtcNow);
            }
            catch (HamnetSnmpException ex)
            {
                this.logger.LogWarning($"Cannot get link details for pair {address1} <-> {address2} (subnet {pair.Key.Subnet}): Error: {ex.Message}");
                hitException = ex;
            }
            catch (SnmpException ex)
            {
                this.logger.LogWarning($"Cannot get link details for pair {address1} <-> {address2} (subnet {pair.Key.Subnet}): SNMP Error: {ex.Message}");
                hitException = ex;
            }
            catch (Exception ex)
            {
                this.logger.LogWarning($"Cannot get link details for pair {address1} <-> {address2} (subnet {pair.Key.Subnet}): General exception: {ex.Message}");
                hitException = ex;
            }

            if (hitException != null)
            {
                this.SendFailToDataHandlers(hitException, pair);
            }
        }

        /// <summary>
        /// Calls the <see cref="IAquiredDataHandler.PrepareForNewAquisition" /> for all configured handlers.
        /// </summary>
        private void SendPrepareToDataHandlers()
        {
            foreach (IAquiredDataHandler handler in this.dataHandlers)
            {
                try
                {
                    handler.PrepareForNewAquisition();
                }
                catch(Exception ex)
                {
                    this.logger.LogError($"Excpetion recording failed query at handler {handler.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Calls the <see cref="IAquiredDataHandler.AquisitionFinished" /> for all configured handlers.
        /// </summary>
        private void SendFinishedToDataHandlers()
        {
            foreach (IAquiredDataHandler handler in this.dataHandlers)
            {
                try
                {
                    handler.AquisitionFinished();
                }
                catch(Exception ex)
                {
                    this.logger.LogError($"Excpetion recording failed query at handler {handler.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Calls the <see cref="IAquiredDataHandler.RecordFailingRssiQueryAsync" /> for all configured handlers.
        /// </summary>
        private void SendFailToDataHandlers(Exception hitException, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> pair)
        {
            foreach (IAquiredDataHandler handler in this.dataHandlers)
            {
                try
                {
                    handler.RecordFailingRssiQuery(hitException, pair);
                }
                catch(Exception ex)
                {
                    this.logger.LogError($"Excpetion recording failed query at handler {handler.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Calls the <see cref="IAquiredDataHandler.RecordDetailsInDatabaseAsync" /> for all configured handlers.
        /// </summary>
        private void SendResultsToDataHandlers(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> pair, ILinkDetails linkDetails, IEnumerable<IDeviceSystemData> systemDatas, DateTime queryTime)
        {
            foreach (IAquiredDataHandler handler in this.dataHandlers)
            {
                try
                {
                    handler.RecordRssiDetailsInDatabaseAsync(pair, linkDetails, queryTime);
                    handler.RecordUptimesInDatabase(pair, linkDetails, systemDatas, queryTime);
                }
                catch(Exception ex)
                {
                    this.logger.LogError($"Excpetion recording failed query at handler {handler.Name}: {ex.Message}");
                }
            }
        }
    }
}
