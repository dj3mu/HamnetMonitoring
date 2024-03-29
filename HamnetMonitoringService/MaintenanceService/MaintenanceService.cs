using System;
using System.Collections.Generic;
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

        private readonly Mutex rssiMutex = new Mutex(false, Program.RssiRunningMutexName);
        private readonly Mutex bgpMutex = new Mutex(false, Program.BgpRunningMutexName);

        private bool disposedValue = false;

        private Timer timer;

        private readonly QuerierOptions snmpQuerierOptions = QuerierOptions.Default;

        private readonly object lockObject = new object();

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
            this.maintenanceInterval = TimeSpan.Parse(section.GetValue<string>("MaintenanceInterval"));

            this.dryRunMode = section.GetValue<bool>("DryRun");

            TimeSpan timeToFirstMaintenance = TimeSpan.FromSeconds(7);

            // by default waiting a couple of secs before first Hamnet scan
            using var resultDatabase = QueryResultDatabaseProvider.Instance.CreateContext();
            var status = resultDatabase.Status;
            var nowItIs = DateTime.UtcNow;
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

            this.logger.LogInformation($"STARTING: Next maintenance run after restart in {timeToFirstMaintenance}: Last maintenance started {status.LastRssiQueryStart}, configured interval {this.maintenanceInterval}");

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

            GC.SuppressFinalize(this);
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
        /// Checks if the given value is outdated.
        /// </summary>
        /// <param name="nowItIs">The current time.</param>
        /// <param name="dataSetWithTimeStampColumn">The dataset with a &quot;TimeStamp&quot; column.</param>
        /// <param name="resultsOutdatedAfter">The maximum allowed age of the dataset.</param>
        /// <returns><c>true</c> if the dataset is outdated.</returns>
        private static bool IsOutdatedTimeStampColumn(DateTime nowItIs, dynamic dataSetWithTimeStampColumn, TimeSpan resultsOutdatedAfter)
        {
            bool isOutdated = (nowItIs - dataSetWithTimeStampColumn.TimeStamp) > resultsOutdatedAfter;

            return isOutdated;
        }

        /// <summary>
        /// Checks if the given value is outdated.
        /// </summary>
        /// <param name="currentUnixTimeStamp">The current unix time stamp.</param>
        /// <param name="dataSetWithUnixTimeStampColumn">The dataset with a unix timestamp column.</param>
        /// <param name="resultsOutdatedAfter">The maximum allowed age of the dataset.</param>
        /// <returns><c>true</c> if the dataset is outdated.</returns>
        private static bool IsOutdatedUnixTimeStampColumn(double currentUnixTimeStamp, dynamic dataSetWithUnixTimeStampColumn, TimeSpan resultsOutdatedAfter)
        {
            bool isOutdated = (currentUnixTimeStamp - dataSetWithUnixTimeStampColumn.UnixTimeStamp) > resultsOutdatedAfter.TotalSeconds;

            return isOutdated;
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
                    // for maintenance we lock down any aquisition
                    this.rssiMutex.WaitOne();
                    this.bgpMutex.WaitOne();

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
                    this.logger.LogError($"Excpetion caught and ignored in maintenance thread: {ex}");
                }
                finally
                {
                    this.rssiMutex.ReleaseMutex();
                    this.bgpMutex.ReleaseMutex();

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

            using var resultDatabase = QueryResultDatabaseProvider.Instance.CreateContext();

            using (var transaction = resultDatabase.Database.BeginTransaction())
            {
                var status = resultDatabase.Status;
                var nowItIs = DateTime.UtcNow;
                var sinceLastScan = nowItIs - status.LastMaintenanceStart;
                if ((sinceLastScan < this.maintenanceInterval - Hysteresis) && (status.LastMaintenanceStart <= status.LastMaintenanceEnd))
                {
                    this.logger.LogInformation($"SKIPPING: Maintenance not yet due: Last aquisition started {status.LastMaintenanceStart} ({sinceLastScan} ago, hysteresis {Hysteresis}), configured interval {this.maintenanceInterval}");
                    return;
                }

                this.logger.LogInformation($"STARTING: Data maintenance - last run: Started {status.LastMaintenanceStart} ({sinceLastScan} ago)");

                status.LastMaintenanceStart = DateTime.UtcNow;

                resultDatabase.SaveChanges();
                transaction.Commit();
            }

            Program.RequestStatistics.MaintenanceRuns++;

            this.RemoveOutdatedResults(resultDatabase, configurationSection);

            var cacheMaintenance = new CacheMaintenance(this.dryRunMode);
            cacheMaintenance.RemoveFromCacheIfModificationOlderThan(configurationSection.GetValue<TimeSpan>("CacheInvalidAfter"));

            RemoveCacheEntriesForFailures(resultDatabase, cacheMaintenance);

            using (var transaction = resultDatabase.Database.BeginTransaction())
            {
                var status = resultDatabase.Status;

                status.LastMaintenanceEnd = DateTime.UtcNow;

                this.logger.LogInformation($"COMPLETED: Database maintenance at {status.LastMaintenanceEnd}, duration {status.LastMaintenanceEnd - status.LastMaintenanceStart}");

                resultDatabase.SaveChanges();
                transaction.Commit();
            }
        }

        /// <summary>
        /// Removes the cache entries for failures recorded in the result database
        /// </summary>
        /// <param name="resultDatabase">The database context to work on.</param>
        /// <param name="cacheMaintenance">The cache maintenance object that supports deletion of entries.</param>
        private static void RemoveCacheEntriesForFailures(QueryResultDatabaseContext resultDatabase, CacheMaintenance cacheMaintenance)
        {
            var affectedHosts = resultDatabase.RssiFailingQueries.AsQueryable().Select(q => q.AffectedHosts);
            List<IPAddress> toDelete = new List<IPAddress>();
            foreach (IReadOnlyCollection<string> item in affectedHosts)
            {
                toDelete.AddRange(item.Select(ah => IPAddress.Parse(ah)));
            }

            cacheMaintenance.DeleteForAddress(toDelete.Distinct());
        }

        /// <summary>
        /// Removes results (in result database) for which we didn't see an update for a configured amount of time.
        /// </summary>
        /// <param name="resultDatabase">The database context to work on.</param>
        /// <param name="configuration">The configuration to use.</param>
        private void RemoveOutdatedResults(QueryResultDatabaseContext resultDatabase, IConfigurationSection configuration)
        {
            TimeSpan resultsOutdatedAfter = configuration.GetValue<TimeSpan>("ResultsOutdatedAfter");

            DateTime nowItIs = DateTime.UtcNow;
            double currentUnixTimeStamp = (nowItIs - Program.UnixTimeStampBase).TotalSeconds;
            using var transaction = resultDatabase.Database.BeginTransaction();
            var outdatedRssis = resultDatabase.RssiValues.AsEnumerable().Where(r => IsOutdatedUnixTimeStampColumn(currentUnixTimeStamp, r, resultsOutdatedAfter)).ToList();
            foreach (var item in outdatedRssis)
            {
                this.logger.LogInformation($"Maintenance{(this.dryRunMode ? " DRY RUN: Would remove" : ": Removing")} RSSI entry for host {item.ForeignId} which hast last been updated at {item.TimeStampString} (i.e. {TimeSpan.FromSeconds(currentUnixTimeStamp - item.UnixTimeStamp)} ago)");
            }

            var outdatedRssiFailures = resultDatabase.RssiFailingQueries.AsEnumerable().Where(r => IsOutdatedTimeStampColumn(nowItIs, r, resultsOutdatedAfter)).ToList();
            foreach (var item in outdatedRssiFailures)
            {
                this.logger.LogInformation($"Maintenance{(this.dryRunMode ? " DRY RUN: Would remove" : ": Removing")} RSSI failing query entry for host {item.Subnet} which hast last been updated at {item.TimeStamp} (i.e. {item.TimeStamp - nowItIs} ago)");
            }

            var outdatedBgpPeers = resultDatabase.BgpPeers.AsEnumerable().Where(r => IsOutdatedUnixTimeStampColumn(currentUnixTimeStamp, r, resultsOutdatedAfter)).ToList();
            foreach (var item in outdatedBgpPeers)
            {
                this.logger.LogInformation($"Maintenance{(this.dryRunMode ? " DRY RUN: Would remove" : ": Removing")} BGP peer entry from host {item.LocalAddress} to {item.RemoteAddress} which hast last been updated at {item.TimeStampString} (i.e. {TimeSpan.FromSeconds(currentUnixTimeStamp - item.UnixTimeStamp)} ago)");
            }

            var outdatedBgpPeerFailures = resultDatabase.BgpFailingQueries.AsEnumerable().Where(r => IsOutdatedTimeStampColumn(nowItIs, r, resultsOutdatedAfter)).ToList();
            foreach (var item in outdatedBgpPeerFailures)
            {
                this.logger.LogInformation($"Maintenance{(this.dryRunMode ? " DRY RUN: Would remove" : ": Removing")} BGP failing peer entry from host {item.Host} which hast last been updated at {item.TimeStamp} (i.e. {item.TimeStamp - nowItIs} ago)");
            }

            var cacheMaintenance = new CacheMaintenance(this.dryRunMode);
            cacheMaintenance.DeleteForAddress(outdatedRssis.Select(e => IPAddress.Parse(e.ForeignId)));
            cacheMaintenance.DeleteForAddress(outdatedRssiFailures.SelectMany(e => e.AffectedHosts.Select(h => IPAddress.Parse(h))));
            cacheMaintenance.DeleteForAddress(outdatedBgpPeers.Select(e => IPAddress.Parse(e.LocalAddress)));
            cacheMaintenance.DeleteForAddress(outdatedBgpPeerFailures.Select(e => IPAddress.Parse(e.Host)));

            if (!this.dryRunMode)
            {
                resultDatabase.RemoveRange(outdatedRssis);
                resultDatabase.RemoveRange(outdatedRssiFailures);
                resultDatabase.RemoveRange(outdatedBgpPeers);
                resultDatabase.RemoveRange(outdatedBgpPeerFailures);

                resultDatabase.SaveChanges();
                transaction.Commit();
            }
            else
            {
                transaction.Rollback();
            }
        }
    }
}
