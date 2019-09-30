using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HamnetDbRest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestService.Database;
using SnmpAbstraction;
using SnmpAbstraction.CachingLayer;

namespace RestService.DataFetchingService
{
    /// <summary>
    /// Hosted service to regularly retrieve the data to be reported via REST api.
    /// </summary>
    public class MaintenanceService : IHostedService, IDisposable
    {
        /// <summary>
        /// The key for the maitenance service configuration section.
        /// </summary>
        public static readonly string MaintenanceServiceSectionKey = "MaintenanceSerivce";

        private static readonly TimeSpan Hysteresis = TimeSpan.FromSeconds(10);

        private readonly ILogger<MaintenanceService> logger;

        private readonly IConfiguration configuration;

        private readonly Mutex mutex = new Mutex(false, Program.ProgramWideMutexName);

        private bool disposedValue = false;

        private Timer timer;

        private QuerierOptions snmpQuerierOptions = QuerierOptions.Default;

        private object lockObject = new object();

        private QueryResultDatabaseContext resultDatabaseContext;

        private TimeSpan maintenanceInterval;

        private bool timerReAdjustmentNeeded = false;

        private bool dryRunMode = false;

        /// <summary>
        /// Constructs taking the logger.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The service configuration.</param>
        public MaintenanceService(ILogger<MaintenanceService> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;

            this.resultDatabaseContext = DatabaseProvider.Instance.CreateContext();
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
            var section = this.configuration.GetSection(MaintenanceServiceSectionKey);
            this.maintenanceInterval = TimeSpan.FromSeconds(section.GetValue<int>("MaintenanceIntervalMins"));

            this.dryRunMode = section.GetValue<bool>("DryRun");

            TimeSpan timeToFirstMaintenance = TimeSpan.FromSeconds(7);

            // by default waiting a couple of secs before first Hamnet scan
            var status = this.resultDatabaseContext.Status;
            var nowItIs = DateTime.Now;
            var timeSinceLastMaintenanceStart = (nowItIs - status.LastMaintenanceStart);
            if (status.LastMaintenanceStart > status.LastMaintenanceEnd)
            {
                this.logger.LogInformation($"STARTING maintenance immediately: Last maintenance started {status.LastMaintenanceStart} seems not to have ended successfully (last end time {status.LastMaintenanceEnd})");
            }
            else if (timeSinceLastMaintenanceStart < this.maintenanceInterval)
            {
                // no aquisition required yet (e.g. service restart)
                timeToFirstMaintenance = this.maintenanceInterval - timeSinceLastMaintenanceStart;
                this.timerReAdjustmentNeeded = true;
            }

            this.logger.LogInformation($"STARTING: Next maintenance run after restart in {timeToFirstMaintenance}: Last aquisition started {status.LastQueryStart}, configured interval {this.maintenanceInterval}");

            this.timer = new Timer(DoMaintenance, null, timeToFirstMaintenance, this.maintenanceInterval);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Maintenance service is stopping.");

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
        private void DoMaintenance(object state)
        {
            // NOTE: The Monitor handles multiple concurrent runs while the mutex prevents running of aquisition and maintenance at the same time.
            if (Monitor.TryEnter(this.lockObject))
            {
                try
                {
                    this.mutex.WaitOne();
                    
                    // make sure to change back the due time of the timer
                    if (this.timerReAdjustmentNeeded)
                    {
                        this.logger.LogInformation($"Re-adjusting timer with due time and interval to {this.maintenanceInterval}");
                        this.timer.Change(this.maintenanceInterval, this.maintenanceInterval);
                        this.timerReAdjustmentNeeded = false;
                    }

                    this.RunMaintenance();
                }
                catch(Exception ex)
                {
                    this.logger.LogError($"Excpetion caught and ignored in maintenance thread: {ex.ToString()}");
                }
                finally
                {
                    this.mutex.ReleaseMutex();

                    Monitor.Exit(this.lockObject);

                    GC.Collect(); // free as much memory as we can
                }
            }
            else
            {
                this.logger.LogError("SKIPPING data aquisition: Previous aquisition still ongoing. Please adjust interval.");
            }
        }

        /// <summary>
        /// Runs all scheduled maintenance tasks.
        /// </summary>
        private void RunMaintenance()
        {
            var configurationSection = this.configuration.GetSection(MaintenanceServiceSectionKey);

            this.NewDatabaseContext();

            using (var transaction = this.resultDatabaseContext.Database.BeginTransaction())
            {
                var status = resultDatabaseContext.Status;
                var nowItIs = DateTime.Now;
                var sinceLastScan = nowItIs - status.LastMaintenanceStart;
                if ((sinceLastScan < this.maintenanceInterval - Hysteresis) && (status.LastMaintenanceStart <= status.LastMaintenanceEnd))
                {
                    this.logger.LogInformation($"SKIPPING: Maintenance not yet due: Last aquisition started {status.LastMaintenanceStart} ({sinceLastScan} ago, hysteresis {Hysteresis}), configured interval {this.maintenanceInterval}");
                    return;
                }
        
                this.logger.LogInformation($"STARTING: Data maintenance - last run: Started {status.LastMaintenanceStart} ({sinceLastScan} ago)");

                status.LastMaintenanceStart = DateTime.Now;

                resultDatabaseContext.SaveChanges();
                transaction.Commit();
            }

            this.RemovedOutdatedResults(configurationSection);

            var cacheMaintenance = new CacheMaintenance(this.dryRunMode);
            cacheMaintenance.RemoveFromCacheIfModificationOlderThan(configurationSection.GetValue<TimeSpan>("CacheInvalidAfter"));

            using (var transaction = this.resultDatabaseContext.Database.BeginTransaction())
            {
                var status = resultDatabaseContext.Status;

                status.LastMaintenanceEnd = DateTime.Now;

                this.logger.LogInformation($"COMPLETED: Database maintenance at {status.LastMaintenanceEnd}, duration {status.LastMaintenanceEnd - status.LastMaintenanceStart}");

                resultDatabaseContext.SaveChanges();
                transaction.Commit();
            }

            this.DisposeDatabaseContext();
        }

        /// <summary>
        /// Removes results (in result database) for which we didn't see an update for a configured amount of time.
        /// </summary>
        private void RemovedOutdatedResults(IConfigurationSection configuration)
        {
            TimeSpan resultsOutdatedAfter = configuration.GetValue<TimeSpan>("ResultsOutdatedAfter");

            var currentUnixTimeStamp = (DateTime.UtcNow - Program.UnixTimeStampBase).TotalSeconds;
            using (var transaction = this.resultDatabaseContext.Database.BeginTransaction())
            {
                var outdatedRssis = this.resultDatabaseContext.RssiValues.Where(r => (currentUnixTimeStamp - r.UnixTimeStamp) > resultsOutdatedAfter.TotalSeconds);

                foreach (var item in outdatedRssis)
                {
                    this.logger.LogInformation($"Maintenance{(this.dryRunMode ? " DRY RUN: Would remove" : ": Removing")} RSSI entry for host {item.ForeignId} which hast last been updated at {item.TimeStampString} (i.e. {TimeSpan.FromSeconds(currentUnixTimeStamp - item.UnixTimeStamp)} ago)");
                }

                var cacheMaintenance = new CacheMaintenance(this.dryRunMode);
                cacheMaintenance.DeleteForAddress(outdatedRssis.Select(e => IPAddress.Parse(e.ForeignId)));

                if (!this.dryRunMode)
                {
                    this.resultDatabaseContext.RemoveRange(outdatedRssis);

                    this.resultDatabaseContext.SaveChanges();
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }
            }
        }

        /// <summary>
        /// Creates a new database context for the result database.
        /// </summary>
        private void NewDatabaseContext()
        {
            this.DisposeDatabaseContext();

            this.resultDatabaseContext = DatabaseProvider.Instance.CreateContext();
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
    }
}
