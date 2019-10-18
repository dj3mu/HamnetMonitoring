using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    /// Hosted service to regularly retrieve the BGP-related data to be reported via REST api.
    /// </summary>
    public class BgpAquisitionService : IHostedService, IDisposable
    {
        private static readonly TimeSpan Hysteresis = TimeSpan.FromSeconds(10);
        
        /// <summary>
        /// The list of receivers for the data that we aquired.
        /// </summary>
        private readonly List<IAquiredDataHandler> dataHandlers = new List<IAquiredDataHandler>();

        private readonly ILogger<BgpAquisitionService> logger;

        private readonly IConfiguration configuration;
        
        private readonly Mutex mutex = new Mutex(false, Program.ProgramWideMutexName);

        private bool disposedValue = false;

        private Timer timer;

        private QuerierOptions snmpQuerierOptions = QuerierOptions.Default;

        private object multiTimerLockingObject = new object();

        private object databaseLockingObject = new object();

        private TimeSpan refreshInterval;

        private QueryResultDatabaseContext resultDatabaseContext;

        private List<Regex> filterRegexList = null;

        private int maxParallelQueries;

        /// <summary>
        /// Constructs taking the logger.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The service configuration.</param>
        public BgpAquisitionService(ILogger<BgpAquisitionService> logger, IConfiguration configuration)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger), "The logger is null when creating a BgpAquisitionService");
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "The configuration is null when creating a BgpAquisitionService");
            }

            this.logger = logger;
            this.configuration = configuration;

            this.dataHandlers.Add(new ResultDatabaseDataHandler(configuration));
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
            IConfigurationSection aquisisionServiceSection = this.configuration.GetSection(Program.BgpAquisitionServiceSectionKey);

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

            var monitoringAccountsSection = this.configuration.GetSection(Program.MonitoringAccountsSectionKey).GetSection(Program.BgpAccountSectionKey);

            // we poll BGP solely using the vendor-specific APIs (SNMP is too fragile and slow for it)
            this.snmpQuerierOptions = this.snmpQuerierOptions.WithAllowedApis(QueryApis.VendorSpecific);

            var loginUserName = monitoringAccountsSection.GetValue<string>("User");
            if (!string.IsNullOrWhiteSpace(loginUserName))
            {
                this.snmpQuerierOptions = this.snmpQuerierOptions.WithUser(loginUserName);
            }

            var loginPassword = monitoringAccountsSection.GetValue<string>("Password");
            if (!string.IsNullOrWhiteSpace(loginPassword))
            {
                this.snmpQuerierOptions = this.snmpQuerierOptions.WithPassword(loginPassword);
            }

            this.snmpQuerierOptions = this.snmpQuerierOptions.WithCaching(aquisisionServiceSection.GetValue<bool>("UseQueryCaching"));

            var hamnetDbConfig = this.configuration.GetSection(HamnetDbProvider.HamnetDbSectionName);
            if (hamnetDbConfig.GetValue<string>(HamnetDbProvider.DatabaseTypeKey).ToUpperInvariant() != "MYSQL")
            {
                throw new InvalidOperationException("Only MySQL / MariaDB is currently supported for the Hament database");
            }

            // get filter regex
            var filterRegexConfig = aquisisionServiceSection.GetSection("WhitelistFilterRegex").GetChildren();
            this.filterRegexList = filterRegexConfig.Select(c => new Regex(c.Value, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)).ToList();
            if (this.filterRegexList.Count == 0)
            {
                this.filterRegexList.Add(new Regex(@".*", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase));
            }

            // by default waiting a couple of secs before first Hamnet scan
            TimeSpan timeToFirstAquisition = TimeSpan.FromSeconds(17);

            this.NewDatabaseContext();

            var status = this.resultDatabaseContext.Status;
            var nowItIs = DateTime.UtcNow;
            var timeSinceLastAquisitionStart = (nowItIs - status.LastBgpQueryStart);
            if (status.LastBgpQueryStart > status.LastBgpQueryEnd)
            {
                this.logger.LogInformation($"STARTING first BGP aquisition immediately: Last aquisition started {status.LastBgpQueryStart} seems not to have ended successfully (last end time {status.LastBgpQueryEnd})");
            }
            else if (timeSinceLastAquisitionStart < this.refreshInterval)
            {
                // no aquisition required yet (e.g. service restart)
                timeToFirstAquisition = this.refreshInterval - timeSinceLastAquisitionStart;
            }

            this.logger.LogInformation($"STARTING first BGP aquisition after restart in {timeToFirstAquisition}: Last aquisition started {status.LastBgpQueryStart}, configured interval {this.refreshInterval}");

            this.timer = new Timer(DoFetchData, null, timeToFirstAquisition, this.refreshInterval);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Timed BGP data fetching service is stopping.");

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
                this.logger.LogError("SKIPPING BGP data aquisition: Previous aquisition still ongoing. Please adjust interval.");
            }
        }

        /// <summary>
        /// Performs the actual data aquisition.
        /// </summary>
        private void PerformDataAquisition()
        {
            IConfigurationSection hamnetDbConfig = this.configuration.GetSection(Program.BgpAquisitionServiceSectionKey);

            // detect if we're due to run and, if we are, record the start of the run
            using (var transaction = this.resultDatabaseContext.Database.BeginTransaction())
            {
                var status = resultDatabaseContext.Status;
                var nowItIs = DateTime.UtcNow;
                var sinceLastScan = nowItIs - status.LastBgpQueryStart;
                if ((sinceLastScan < this.refreshInterval - Hysteresis) && (status.LastBgpQueryStart <= status.LastBgpQueryEnd))
                {
                    this.logger.LogInformation($"SKIPPING: BGP aquisition not yet due: Last aquisition started {status.LastBgpQueryStart} ({sinceLastScan} ago, hysteresis {Hysteresis}), configured interval {this.refreshInterval}");
                    return;
                }

                // we restart the timer so that in case we've been blocked by Mutexes etc. the interval really starts from scratch        
                this.timer.Change(this.refreshInterval, this.refreshInterval);

                this.logger.LogInformation($"STARTING: Retrieving BGP monitoring data as configured in HamnetDB - last run: Started {status.LastBgpQueryStart} ({sinceLastScan} ago)");

                status.LastBgpQueryStart = DateTime.UtcNow;

                resultDatabaseContext.SaveChanges();
                transaction.Commit();
            }

            HamnetDbPoller hamnetDbPoller = new HamnetDbPoller(this.configuration);
            List<IHamnetDbHost> hostsSlicedAccordingToConfiguration = hamnetDbPoller.FetchBgpRoutersFromHamnetDb();

            this.logger.LogDebug($"SNMP querying {hostsSlicedAccordingToConfiguration.Count} entries");

            this.SendPrepareToDataHandlers();

            NetworkExcludeFile excludes = new NetworkExcludeFile(hamnetDbConfig);
            var excludeNets = excludes?.ParsedNetworks?.ToList();

            this.logger.LogDebug($"Launching {this.maxParallelQueries} parallel BGP aquisition threads");

            var filteredHosts = hostsSlicedAccordingToConfiguration
                // the following where is just a hack until HamnetDB has a flag clearly identifying routers that shall be queried for BGP
                // and have opened their API port to "monitoring" user.
                .Where(hsatc => this.filterRegexList.Any(fr => fr.IsMatch(hsatc.Callsign)))
                .ToList(); // for debugger

            Parallel.ForEach(
                filteredHosts,
                new ParallelOptions { MaxDegreeOfParallelism = this.maxParallelQueries },
            host =>
            {
                if ((excludeNets != null) && (excludeNets.Any(exclude => exclude.Contains(host.Address))))
                {
                    this.logger.LogInformation($"Skipping subnet {host.Address} due to exclude list");
                    return;
                }

                try
                {
                    this.QueryPeersForSingleHost(host);
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Exception caught and ignored in BGP parallel data aquisition thread: {ex.ToString()}");
                }
            });

            this.SendFinishedToDataHandlers();

            // record the regular end of the run
            using (var transaction = this.resultDatabaseContext.Database.BeginTransaction())
            {
                var status = resultDatabaseContext.Status;

                status.LastBgpQueryEnd = DateTime.UtcNow;

                this.logger.LogInformation($"COMPLETED: Retrieving BGP monitoring data as configured in HamnetDB at {status.LastBgpQueryEnd}, duration {status.LastBgpQueryEnd - status.LastBgpQueryStart}");

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
        /// Queries the BGP peers of a single host.
        /// </summary>
        /// <param name="host">The host to query BGP peers from.</param>
        private void QueryPeersForSingleHost(IHamnetDbHost host)
        {
            this.logger.LogInformation($"Querying BGP peers for host {host.Address} ({host.Name})");

            Exception hitException = null;
            try
            {
                using(var querier = SnmpQuerierFactory.Instance.Create(host.Address, this.snmpQuerierOptions))
                {
                    // NOTE: Do not Dispose the querier until ALL data has been copied to other containers!
                    //       Else the lazy-loading containers might fail to lazy-query the required values.

                    var bgpPeers = querier.FetchBgpPeers(null);

                    var storeOnlyDetailsClone = new BgpPeersStoreOnlyContainer(bgpPeers);

                    this.SendResultsToDataHandlers(host, storeOnlyDetailsClone, DateTime.UtcNow);
                }
            }
            catch (HamnetSnmpException ex)
            {
                this.logger.LogWarning($"Cannot get BGP peers for host {host.Address} ({host.Name}): Error: {ex.Message}");
                hitException = ex;
            }
            catch (SnmpException ex)
            {
                this.logger.LogWarning($"Cannot get BGP peers for host {host.Address} ({host.Name}): SNMP Error: {ex.Message}");
                hitException = ex;
            }
            catch (Exception ex)
            {
                this.logger.LogWarning($"Cannot get BGP peers for host {host.Address} ({host.Name}): General exception: {ex.Message}");
                hitException = ex;
            }

            if (hitException != null)
            {
                this.SendFailToDataHandlers(hitException, host);
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
        /// Calls the <see cref="IAquiredDataHandler.RecordFailingBgpQueryAsync" /> for all configured handlers.
        /// </summary>
        private void SendFailToDataHandlers(Exception hitException, IHamnetDbHost host)
        {
            foreach (IAquiredDataHandler handler in this.dataHandlers)
            {
                try
                {
                    handler.RecordFailingBgpQuery(hitException, host);
                }
                catch(Exception ex)
                {
                    this.logger.LogError($"Excpetion recording failed query at handler {handler.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Calls the IAquiredDataHandler.RecordDetailsInDatabaseAsync for all configured handlers.
        /// </summary>
        private void SendResultsToDataHandlers(IHamnetDbHost host, IBgpPeers peers, DateTime queryTime)
        {
            foreach (IAquiredDataHandler handler in this.dataHandlers)
            {
                try
                {
                    handler.RecordDetailsInDatabaseAsync(host, peers, queryTime);
                }
                catch(Exception ex)
                {
                    this.logger.LogError($"Excpetion recording failed query at handler {handler.Name}: {ex.Message}");
                }
            }
        }
    }
}
