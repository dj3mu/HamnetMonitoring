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
        
        private readonly Mutex mutex = new Mutex(false, Program.ProgramWideMutexName);

        private bool disposedValue = false;

        private Timer timer;

        private QuerierOptions snmpQuerierOptions = QuerierOptions.Default;

        private object multiTimerLockingObject = new object();

        private object databaseLockingObject = new object();

        private TimeSpan refreshInterval;

        private bool timerReAdjustmentNeeded = false;

        private QueryResultDatabaseContext resultDatabaseContext;

        private int maxParallelQueries;

        /// <summary>
        /// Constructs taking the logger.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The service configuration.</param>
        public RssiAquisitionService(ILogger<RssiAquisitionService> logger, IConfiguration configuration)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger), "The logger is null when creating a DataAquisitionService");
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "The configuration is null when creating a DataAquisitionService");
            }

            this.logger = logger;
            this.configuration = configuration;

            this.dataHandlers.Add(new ResultDatabaseDataHandler(configuration));

            IConfigurationSection influxSection = configuration.GetSection(Program.InfluxSectionKey);
            if ((influxSection != null) && influxSection.GetChildren().Any())
            {
                this.dataHandlers.Add(new InfluxDatabaseDataHandler(configuration));
            }
            else
            {
                this.logger.LogInformation($"Influx database disabled: No or empty '{Program.InfluxSectionKey}' section in configuration");
            }
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
            IConfigurationSection aquisisionServiceSection = this.configuration.GetSection(Program.AquisitionServiceSectionKey);

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

            var snmpTimeoutConfig = this.configuration.GetSection(Program.AquisitionServiceSectionKey).GetValue<int>("SnmpTimeoutSeconds");
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
            if (hamnetDbConfig.GetValue<string>(HamnetDbProvider.DatabaseTypeKey).ToUpperInvariant() != "MYSQL")
            {
                throw new InvalidOperationException("Only MySQL / MariaDB is currently supported for the Hament database");
            }

            // by default waiting a couple of secs before first Hamnet scan
            TimeSpan timeToFirstAquisition = TimeSpan.FromSeconds(11);

            this.NewDatabaseContext();

            var status = this.resultDatabaseContext.Status;
            var nowItIs = DateTime.UtcNow;
            var timeSinceLastAquisitionStart = (nowItIs - status.LastQueryStart);
            if (status.LastQueryStart > status.LastQueryEnd)
            {
                this.logger.LogInformation($"STARTING first aquisition immediately: Last aquisition started {status.LastQueryStart} seems not to have ended successfully (last end time {status.LastQueryEnd})");
            }
            else if (timeSinceLastAquisitionStart < this.refreshInterval)
            {
                // no aquisition required yet (e.g. service restart)
                timeToFirstAquisition = this.refreshInterval - timeSinceLastAquisitionStart;
                this.timerReAdjustmentNeeded = true;
            }

            this.logger.LogInformation($"STARTING first aquisition after restart in {timeToFirstAquisition}: Last aquisition started {status.LastQueryStart}, configured interval {this.refreshInterval}");

            this.timer = new Timer(DoFetchData, null, timeToFirstAquisition, this.refreshInterval);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Timed data fetching service is stopping.");

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
                        if (item != null)
                        {
                            item.Dispose();
                        }
                    }

                    this.dataHandlers.Clear();

                    this.DisposeDatabaseContext();
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
                    this.mutex.WaitOne();

                    // make sure to change back the due time of the timer
                    if (this.timerReAdjustmentNeeded)
                    {
                        this.logger.LogInformation($"Re-adjusting timer with due time and interval to {this.refreshInterval}");
                        this.timer.Change(this.refreshInterval, this.refreshInterval);
                        this.timerReAdjustmentNeeded = false;
                    }

                    this.PerformDataAquisition();
                }
                catch(Exception ex)
                {
                    this.logger.LogError($"Excpetion caught and ignored in timer-called data aquisition thread: {ex.ToString()}");
                }
                finally
                {
                    this.mutex.ReleaseMutex();

                    Monitor.Exit(this.multiTimerLockingObject);

                    GC.Collect(); // free as much memory as we can
                }
            }
            else
            {
                this.logger.LogError("SKIPPING data aquisition: Previous aquisition still ongoing. Please adjust interval.");
            }
        }

        /// <summary>
        /// Performs the actual data aquisition.
        /// </summary>
        private void PerformDataAquisition()
        {
            IConfigurationSection hamnetDbConfig = this.configuration.GetSection(Program.AquisitionServiceSectionKey);

            // detect if we're due to run and, if we are, record the start of the run
            using (var transaction = this.resultDatabaseContext.Database.BeginTransaction())
            {
                var status = resultDatabaseContext.Status;
                var nowItIs = DateTime.UtcNow;
                var sinceLastScan = nowItIs - status.LastQueryStart;
                if ((sinceLastScan < this.refreshInterval - Hysteresis) && (status.LastQueryStart <= status.LastQueryEnd))
                {
                    this.logger.LogInformation($"SKIPPING: Aquisition not yet due: Last aquisition started {status.LastQueryStart} ({sinceLastScan} ago, hysteresis {Hysteresis}), configured interval {this.refreshInterval}");
                    return;
                }
        
                this.logger.LogInformation($"STARTING: Retrieving monitoring data as configured in HamnetDB - last run: Started {status.LastQueryStart} ({sinceLastScan} ago)");

                status.LastQueryStart = DateTime.UtcNow;

                resultDatabaseContext.SaveChanges();
                transaction.Commit();
            }

            HamnetDbPoller hamnetDbPoller = new HamnetDbPoller(this.configuration);
            Dictionary<IHamnetDbSubnet, IHamnetDbHosts> pairsSlicedAccordingToConfiguration = hamnetDbPoller.FetchSubnetsWithHostsFromHamnetDb();

            this.logger.LogDebug($"SNMP querying {pairsSlicedAccordingToConfiguration.Count} entries");

            this.SendPrepareToDataHandlers();

            NetworkExcludeFile excludes = new NetworkExcludeFile(hamnetDbConfig);
            var excludeNets = excludes?.ParsedNetworks?.ToList();

            this.logger.LogDebug($"Launching {this.maxParallelQueries} parallel aquisition threads");

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
                    this.logger.LogError($"Exception caught and ignored in parallel data aquisition thread: {ex.ToString()}");
                }
            });

            this.SendFinishedToDataHandlers();

            // record the regular end of the run
            using (var transaction = this.resultDatabaseContext.Database.BeginTransaction())
            {
                var status = resultDatabaseContext.Status;

                status.LastQueryEnd = DateTime.UtcNow;

                this.logger.LogInformation($"COMPLETED: Retrieving monitoring data as configured in HamnetDB at {status.LastQueryEnd}, duration {status.LastQueryEnd - status.LastQueryStart}");

                resultDatabaseContext.SaveChanges();
                transaction.Commit();
            }
        }

        /// <summary>
        /// Creates a new database context for the result database.
        /// </summary>
        private void NewDatabaseContext()
        {
            this.DisposeDatabaseContext();

            this.resultDatabaseContext = QueryResultDatabaseProvider.Instance.CreateContext();
        }

        /// <summary>
        /// Disposes off the result database context.
        /// </summary>
        private void DisposeDatabaseContext()
        {
            if (this.resultDatabaseContext != null)
            {
                this.resultDatabaseContext.Dispose();
                this.resultDatabaseContext = null;
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

            this.logger.LogInformation($"Querying link details for pair {address1} <-> {address2} of subnet {pair.Key.Subnet}");

            Exception hitException = null;
            try
            {
                using(var querier = SnmpQuerierFactory.Instance.Create(address1, this.snmpQuerierOptions))
                {
                    // NOTE: Do not Dispose the querier until ALL data has been copied to other containers!
                    //       Else the lazy-loading containers might fail to lazy-query the required values.

                    var linkDetails = querier.FetchLinkDetails(address2.ToString());

                    var storeOnlyDetailsClone = new LinkDetailsStoreOnlyContainer(linkDetails);

                    this.SendResultsToDataHandlers(pair, storeOnlyDetailsClone, DateTime.UtcNow);
                }
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
        /// Calls the <see cref="IAquiredDataHandler.RecordFailingQueryAsync" /> for all configured handlers.
        /// </summary>
        private void SendFailToDataHandlers(Exception hitException, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> pair)
        {
            foreach (IAquiredDataHandler handler in this.dataHandlers)
            {
                try
                {
                    handler.RecordFailingQuery(hitException, pair);
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
        private void SendResultsToDataHandlers(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> pair, ILinkDetails linkDetails, DateTime queryTime)
        {
            foreach (IAquiredDataHandler handler in this.dataHandlers)
            {
                try
                {
                    handler.RecordDetailsInDatabaseAsync(pair, linkDetails, queryTime);
                }
                catch(Exception ex)
                {
                    this.logger.LogError($"Excpetion recording failed query at handler {handler.Name}: {ex.Message}");
                }
            }
        }
    }
}
