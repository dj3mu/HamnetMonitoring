using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HamnetDbAbstraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestService.Database;
using RestService.Model;
using SnmpAbstraction;
using SnmpSharpNet;

namespace RestService.DataFetchingService
{
    /// <summary>
    /// Hosted service to regularly retrieve the data to be reported via REST api.
    /// </summary>
    public class DataAquisitionService : IHostedService, IDisposable
    {
        private const string HamnetDbSectionKey = "HamnetQuery";
    
        private const string RssiMetricName = "RSSI";

        private const int RssiMetricId = 1;
        
        private static readonly DateTime UnixTimeStampBase = new DateTime(1970, 1, 1);

        private readonly ILogger<DataAquisitionService> logger;

        private readonly IConfiguration configuration;
        
        private bool disposedValue = false;

        private Timer timer;

        private QuerierOptions snmpQuerierOptions = QuerierOptions.Default;
        
        private int auqisitionInProgress = 0;

        /// <summary>
        /// Constructs taking the logger.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The service configuration.</param>
        public DataAquisitionService(ILogger<DataAquisitionService> logger, IConfiguration configuration)
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
            var refreshIntervalSecs = this.configuration.GetSection(HamnetDbSectionKey).GetValue<int>("RefreshIntervalSecs");

            var snmpVersion = this.configuration.GetSection(HamnetDbSectionKey).GetValue<int>("SnmpVersion");

            this.snmpQuerierOptions = this.snmpQuerierOptions.WithProtocolVersion(snmpVersion.ToSnmpVersion());

            this.logger.LogDebug("Timed data fetching service is starting with a refresh interval of {refreshIntervalSecs} seconds");

            this.timer = new Timer(DoFetchData, null, TimeSpan.FromSeconds(3) /* waiting a couple of secs before first Hamnet scan */, TimeSpan.FromSeconds(refreshIntervalSecs));

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogDebug("Timed data fetching service is stopping.");

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
        private void DoFetchData(object state)
        {
            if (Interlocked.CompareExchange(ref this.auqisitionInProgress, 1, 0) == 1)
            {
                this.logger.LogWarning("SKIPPING data aquisition: Already in progress");
                return;
            }

            this.logger.LogInformation("STARTING: Retrieving monitoring data as configured in HamnetDB");

            IConfigurationSection hamnetDbConfig = this.configuration.GetSection(HamnetDbSectionKey);
            string connectionStringFile = hamnetDbConfig.GetValue<string>("ConnectionStringFile");

            var accessor = HamnetDbProvider.Instance.GetHamnetDb(connectionStringFile);

            this.logger.LogDebug($"Getting unique host pairs to be monitored from HamnetDB. Please stand by ...");

            var uniquePairs = accessor.UniqueMonitoredHostPairsInSameSubnet();

            this.logger.LogDebug($"... found {uniquePairs.Count} unique pairs");

            int maximumSubnetCount = hamnetDbConfig.GetValue<int>("MaximumSubnetCount");
            if (maximumSubnetCount == 0)
            {
                // config returns 0 if not defined --> turn it to the reasonable "maximum" value
                maximumSubnetCount = int.MaxValue;
            }

            int startOffset = hamnetDbConfig.GetValue<int>("SubnetStartOffset"); // will implicitly return 0 if not defined

            var pairsSlicedForOptions = uniquePairs.Skip(startOffset).Take(maximumSubnetCount).ToDictionary(k => k.Key, v => v.Value);

            this.logger.LogDebug($"SNMP querying {pairsSlicedForOptions.Count} entries");

            HashSet<IPAddress> queriedHosts = new HashSet<IPAddress>();

            QueryResultDatabaseContext resultDb = DatabaseProvider.Instance.ResultDatabase;

            if (hamnetDbConfig.GetValue<bool>("TruncateFailingQueries"))
            {
                resultDb.Database.ExecuteSqlCommand("DELETE FROM RssiFailingQueries");
            }

            foreach (var pair in pairsSlicedForOptions)
            {
                this.QuerySingleSubnet(pair, queriedHosts, resultDb);
            }

            this.logger.LogInformation("COMPLETED: Retrieving monitoring data as configured in HamnetDB");

            Interlocked.Exchange(ref this.auqisitionInProgress, 0);
        }

        private void QuerySingleSubnet(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> pair, HashSet<IPAddress> queriedHosts, QueryResultDatabaseContext resultDb)
        {
            IPAddress address1 = pair.Value.First().Address;
            IPAddress address2 = pair.Value.Last().Address;

            if (queriedHosts.Contains(address1))
            {
                this.logger.LogDebug($"Skipping link details for pair {address1} <-> {address2} of subnet {pair.Key.Subnet}: {address1} has already been included in a query");
                return;
            }

            if (queriedHosts.Contains(address2))
            {
                this.logger.LogDebug($"Skipping link details for pair {address1} <-> {address2} of subnet {pair.Key.Subnet}: {address2} has already been included in a query");
                return;
            }

            queriedHosts.Add(address1);
            queriedHosts.Add(address2);

            this.logger.LogInformation($"Querying link details for pair {address1} <-> {address2} of subnet {pair.Key.Subnet}");

            using (var transaction = resultDb.Database.BeginTransaction())
            {
                try
                {
                    var querier = SnmpQuerierFactory.Instance.Create(address1, this.snmpQuerierOptions);

                    var linkDetails = querier.FetchLinkDetails(address2.ToString());

                    this.RecordDetailsInDatabase(resultDb, linkDetails, DateTime.UtcNow);

                    this.DeleteFailingQuery(resultDb, pair.Key);
                }
                catch (HamnetSnmpException ex)
                {
                    this.logger.LogWarning($"Cannot get link details for pair {address1} <-> {address2} (subnet {pair.Key.Subnet}): Error: {ex.Message}");
                    this.RecordFailingQuery(resultDb, queriedHosts, ex, pair.Key);
                }
                catch (SnmpException ex)
                {
                    this.logger.LogWarning($"Cannot get link details for pair {address1} <-> {address2} (subnet {pair.Key.Subnet}): SNMP Error: {ex.Message}");
                    this.RecordFailingQuery(resultDb, queriedHosts, ex, pair.Key);
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning($"Cannot get link details for pair {address1} <-> {address2} (subnet {pair.Key.Subnet}): General exception: {ex.Message}");
                    this.RecordFailingQuery(resultDb, queriedHosts, ex, pair.Key);
                }
                
                resultDb.SaveChanges();

                transaction.Commit();
            }
        }

        private void DeleteFailingQuery(QueryResultDatabaseContext resultDb, IHamnetDbSubnet subnet)
        {
            var failingSubnetString = subnet.Subnet.ToString();
            this.logger.LogDebug($"Removing fail entry for subnet '{failingSubnetString}'");
            var entryToRemove = resultDb.RssiFailingQueries.SingleOrDefault(e => e.Subnet == failingSubnetString);
            if (entryToRemove != null)
            {
                resultDb.Remove(entryToRemove);
            }
        }

        private void RecordFailingQuery(QueryResultDatabaseContext resultDb, HashSet<IPAddress> queriedHosts, Exception ex, IHamnetDbSubnet subnet)
        {
            var failingSubnetString = subnet.Subnet.ToString();
            var failEntry = resultDb.RssiFailingQueries.Find(failingSubnetString);
            if (failEntry == null)
            {
                failEntry = new RssiFailingQuery
                {
                    Subnet = failingSubnetString
                };

                resultDb.RssiFailingQueries.Add(failEntry);
            }

            failEntry.TimeStamp = DateTime.UtcNow;

#if DEBUG
            failEntry.ErrorInfo = ex.ToString();
#else
            Exception currentExcpetion = ex;
            string errorInfo = string.Empty;
            while(currentExcpetion != null)
            {
                if (errorInfo.Length > 0)
                {
                    errorInfo += Environment.NewLine;
                }

                errorInfo += currentExcpetion.Message;
                currentExcpetion = currentExcpetion.InnerException;
            }

            failEntry.ErrorInfo = errorInfo;
#endif
        }

        private void RecordDetailsInDatabase(QueryResultDatabaseContext resultDb, ILinkDetails linkDetails, DateTime queryTime)
        {
            foreach (var item in linkDetails.Details)
            {
                SetNewRssiForLink(resultDb, queryTime, item, item.Address1.ToString(), item.RxLevel1at2);
                SetNewRssiForLink(resultDb, queryTime, item, item.Address2.ToString(), item.RxLevel2at1);
            }
        }

        private static void SetNewRssiForLink(QueryResultDatabaseContext resultDb, DateTime queryTime, ILinkDetail item, string adressToSearch, double rssiToSet)
        {
            var adressEntry = resultDb.RssiValues.Find(adressToSearch);
            if (adressEntry == null)
            {
                adressEntry = new Rssi
                {
                    ForeignId = adressToSearch,
                    Metric = RssiMetricName,
                    MetricId = RssiMetricId,
                };

                resultDb.RssiValues.Add(adressEntry);
            }

            adressEntry.RssiValue = rssiToSet.ToString("0.0");
            adressEntry.TimeStampString = queryTime.ToUniversalTime().ToString("yyyy-MM-ddTHH\\:mm\\:sszzz");
            adressEntry.UnixTimeStamp = (ulong)queryTime.ToUniversalTime().Subtract(UnixTimeStampBase).TotalSeconds;
        }
    }
}
