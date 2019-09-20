using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HamnetDbAbstraction;
using HamnetDbRest;
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
        /// <summary>
        /// The section key for the Data Aquisition service configuration.
        /// </summary>
        public static readonly string AquisitionServiceSectionKey = "DataAquisitionService";
    
        private const string RssiMetricName = "RSSI";

        private const int RssiMetricId = 1;
        
        private readonly ILogger<DataAquisitionService> logger;

        private readonly IConfiguration configuration;
        
        private bool disposedValue = false;

        private Timer timer;

        private QuerierOptions snmpQuerierOptions = QuerierOptions.Default;

        private object lockObject = new object();

        private TimeSpan refreshInterval;

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
            this.refreshInterval = TimeSpan.FromSeconds(this.configuration.GetSection(AquisitionServiceSectionKey).GetValue<int>("RefreshIntervalSecs"));

            var snmpVersion = this.configuration.GetSection(AquisitionServiceSectionKey).GetValue<int>("SnmpVersion");
            if (snmpVersion != 0)
            {
                // 0 is the default value set by config framework and doesn't make any sense here - so we can use it to identify a missing config
                this.snmpQuerierOptions = this.snmpQuerierOptions.WithProtocolVersion(snmpVersion.ToSnmpVersion());
            }

            var snmpTimeoutConfig = this.configuration.GetSection(AquisitionServiceSectionKey).GetValue<int>("SnmpTimeoutSeconds");
            if (snmpTimeoutConfig != 0)
            {
                // 0 is the default value set by config framework and doesn't make any sense here - so we can use it to identify a missing config
                this.snmpQuerierOptions = this.snmpQuerierOptions.WithTimeout(TimeSpan.FromSeconds(snmpTimeoutConfig));
            }

            var snmpRetriesConfig = this.configuration.GetSection(AquisitionServiceSectionKey).GetValue<int>("SnmpRetries");
            if (snmpRetriesConfig != 0)
            {
                // 0 is the default value set by config framework and doesn't make any sense here - so we can use it to identify a missing config
                this.snmpQuerierOptions = this.snmpQuerierOptions.WithRetries(snmpRetriesConfig);
            }

            this.logger.LogInformation("Timed data fetching service is starting with a refresh interval of {refreshIntervalSecs} seconds");

            this.timer = new Timer(DoFetchData, null, TimeSpan.FromSeconds(3) /* waiting a couple of secs before first Hamnet scan */, this.refreshInterval);

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
            if (Monitor.TryEnter(this.lockObject))
            {
                try
                {
                    this.PerformDataAquisition();
                }
                catch(Exception ex)
                {
                    this.logger.LogError($"Excpetion caught and ignored in timer-called data aquisition thread: {ex.ToString()}");
                }
                finally
                {
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
        /// Performs the actual data aquisition.
        /// </summary>
        private void PerformDataAquisition()
        {
            IConfigurationSection hamnetDbConfig = this.configuration.GetSection(AquisitionServiceSectionKey);

            Dictionary<IHamnetDbSubnet, IHamnetDbHosts> pairsSlicedAccordingToConfiguration;

            using (QueryResultDatabaseContext resultDb = DatabaseProvider.Instance.CreateContext())
            {
                var status = resultDb.Status;
                var nowItIs = DateTime.Now;
                if ((nowItIs - status.LastQueryEnd) < this.refreshInterval)
                {
                    this.logger.LogInformation($"SKIPPING: Aquisition not yet due: Last aquisition ended {status.LastQueryEnd}, configured interval {this.refreshInterval}");
                    return;
                }

                status.LastQueryStart = DateTime.Now;
                resultDb.SaveChanges();

                this.logger.LogInformation($"STARTING: Retrieving monitoring data as configured in HamnetDB - last run ended at {status.LastQueryEnd}");

                pairsSlicedAccordingToConfiguration = FetchSubnetsWithHostsFromHamnetDb(hamnetDbConfig);

                this.logger.LogDebug($"SNMP querying {pairsSlicedAccordingToConfiguration.Count} entries");

                if (hamnetDbConfig.GetValue<bool>("TruncateFailingQueries"))
                {
                    using (var transaction = resultDb.Database.BeginTransaction())
                    {
                        resultDb.Database.ExecuteSqlCommand("DELETE FROM RssiFailingQueries");
                        resultDb.SaveChanges();
                        transaction.Commit();
                    }
                }
            }

            int maxParallelQueries = hamnetDbConfig.GetValue<int>("MaximumParallelQueries");
            if (maxParallelQueries == 0)
            {
                maxParallelQueries = 4;
            }

            NetworkExcludeFile excludes = this.GetExcludes(hamnetDbConfig);

            this.logger.LogDebug($"Launching {maxParallelQueries} parallel aquisition threads");

            Parallel.ForEach(pairsSlicedAccordingToConfiguration, new ParallelOptions { MaxDegreeOfParallelism = maxParallelQueries },
            pair =>
            {
                if ((excludes != null) && (excludes.ParsedNetworks.Any(exclude => exclude.Equals(pair.Key.Subnet) || exclude.Contains(pair.Key.Subnet))))
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

            using (QueryResultDatabaseContext resultDb = DatabaseProvider.Instance.CreateContext())
            {
                var status = resultDb.Status;
                status.LastQueryEnd = DateTime.Now;
                resultDb.SaveChanges();

                this.logger.LogInformation($"COMPLETED: Retrieving monitoring data as configured in HamnetDB at {status.LastQueryEnd}, duration {status.LastQueryEnd - status.LastQueryStart}");
            }
        }

        /// <summary>
        /// Gets the excludes if the exclude file exists.
        /// </summary>
        /// <param name="hamnetDbConfig">The configuration to take the exclude file name from.</param>
        /// <returns>The exclude file handler class.</returns>
        private NetworkExcludeFile GetExcludes(IConfigurationSection hamnetDbConfig)
        {
            NetworkExcludeFile excludes = null;
            string excludeFileName = hamnetDbConfig.GetValue<string>("ExcludeFile")?.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            if (!string.IsNullOrWhiteSpace(excludeFileName))
            {
                if (File.Exists(excludeFileName))
                {
                    this.logger.LogDebug($"Reading Exclude file {excludeFileName} parallel aquisition threads");
                    excludes = new NetworkExcludeFile(excludeFileName);
                    excludes.Parse();
                }
                else
                {
                    this.logger.LogDebug($"Exclude file '{excludeFileName}' does not exist. Not using any excludes.");
                }
            }

            return excludes;
        }

        /// <summary>
        /// Retrieves the list of subnets with their hosts to monitor from HamnetDB.
        /// </summary>
        /// <param name="hamnetDbConfig">The configuration section.</param>
        /// <returns>The list of subnets with their hosts to monitor from HamnetDB.</returns>
        private Dictionary<IHamnetDbSubnet, IHamnetDbHosts> FetchSubnetsWithHostsFromHamnetDb(IConfigurationSection hamnetDbConfig)
        {
            string connectionStringFile = hamnetDbConfig.GetValue<string>("ConnectionStringFile");

            this.logger.LogDebug($"Getting unique host pairs to be monitored from HamnetDB. Please stand by ...");

            using(var accessor = HamnetDbProvider.Instance.GetHamnetDb(connectionStringFile))
            {
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

                return pairsSlicedForOptions;
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

            using (var resultDb = DatabaseProvider.Instance.CreateContext())
            {
                try
                {
                    using(var querier = SnmpQuerierFactory.Instance.Create(address1, this.snmpQuerierOptions))
                    {
                        // NOTE: Do not Dispose the querier until ALL data has been copied to other containers!
                        //       Else the lazy-loading containers will fail to lazy-query the required values.

                        var linkDetails = querier.FetchLinkDetails(address2.ToString());

                        using (var transaction = resultDb.Database.BeginTransaction())
                        {
                            this.RecordDetailsInDatabase(resultDb, linkDetails, DateTime.UtcNow);

                            this.DeleteFailingQuery(resultDb, pair.Key);
                    
                            resultDb.SaveChanges();

                            transaction.Commit();
                        }
                    }
                }
                catch (HamnetSnmpException ex)
                {
                    this.logger.LogWarning($"Cannot get link details for pair {address1} <-> {address2} (subnet {pair.Key.Subnet}): Error: {ex.Message}");
                    this.RecordFailingQuery(resultDb, ex, pair);
                }
                catch (SnmpException ex)
                {
                    this.logger.LogWarning($"Cannot get link details for pair {address1} <-> {address2} (subnet {pair.Key.Subnet}): SNMP Error: {ex.Message}");
                    this.RecordFailingQuery(resultDb, ex, pair);
                }
                catch (Exception ex)
                {
                    this.logger.LogWarning($"Cannot get link details for pair {address1} <-> {address2} (subnet {pair.Key.Subnet}): General exception: {ex.Message}");
                    this.RecordFailingQuery(resultDb, ex, pair);
                }
            }
        }

        /// <summary>
        /// Deletes an entry in the failing query table.
        /// </summary>
        /// <param name="resultDb">The databse handle.</param>
        /// <param name="subnet">The subnet which serves as key to the entry to delete.</param>
        private void DeleteFailingQuery(QueryResultDatabaseContext resultDb, IHamnetDbSubnet subnet)
        {
            var failingSubnetString = subnet.Subnet.ToString();
            var entryToRemove = resultDb.RssiFailingQueries.SingleOrDefault(e => e.Subnet == failingSubnetString);
            if (entryToRemove != null)
            {
                this.logger.LogDebug($"Removing fail entry for subnet '{failingSubnetString}'");
                resultDb.Remove(entryToRemove);
            }
        }

        /// <summary>
        /// Records a failing query.
        /// </summary>
        /// <param name="resultDb">The databse handle.</param>
        /// <param name="ex">The exception that caused the failure.</param>
        /// <param name="pair">The pair of hosts inside the subnet to query.</param>
        private void RecordFailingQuery(QueryResultDatabaseContext resultDb, Exception ex, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> pair)
        {
            using (var transaction = resultDb.Database.BeginTransaction())
            {
                RecordFailingQueryEntry(resultDb, ex, pair);

                resultDb.SaveChanges();

                transaction.Commit();
            }
        }

        /// <summary>
        /// Removes the RSSI table entry for the given address.
        /// </summary>
        /// <param name="resultDb">The databse handle.</param>
        /// <param name="address">The address for which to remove the entry.</param>
        private static void RemoveRssiTableEntryForHost(QueryResultDatabaseContext resultDb, string address)
        {
            var entryToRemove = resultDb.RssiValues.Find(address);
            if (entryToRemove != null)
            {
                resultDb.Remove(entryToRemove);
            }
        }

        /// <summary>
        /// Records a failing query.
        /// </summary>
        /// <param name="resultDb">The databse handle.</param>
        /// <param name="ex">The exception that caused the failure.</param>
        /// <param name="pair">The pair of hosts inside the subnet to query.</param>
        private static void RecordFailingQueryEntry(QueryResultDatabaseContext resultDb, Exception ex, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> pair)
        {
            var failingSubnetString = pair.Key.Subnet.ToString();
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

//#if DEBUG
            failEntry.ErrorInfo = ex.ToString();
//#else
//                Exception currentExcpetion = ex;
//                string errorInfo = string.Empty;
//                while(currentExcpetion != null)
//                {
//                    if (errorInfo.Length > 0)
//                    {
//                        errorInfo += Environment.NewLine;
//                    }
//
//                    errorInfo += currentExcpetion.Message;
//                    currentExcpetion = currentExcpetion.InnerException;
//                }
//
//                failEntry.ErrorInfo = errorInfo;
//#endif
        }

        /// <summary>
        /// Records the link details in the database.
        /// </summary>
        /// <param name="resultDb">The databse handle.</param>
        /// <param name="linkDetails">The link details to record.</param>
        /// <param name="queryTime">The time of the data aquisition (recorded with the data).</param>
        private void RecordDetailsInDatabase(QueryResultDatabaseContext resultDb, ILinkDetails linkDetails, DateTime queryTime)
        {
            foreach (var item in linkDetails.Details)
            {
                SetNewRssiForLink(resultDb, queryTime, item, item.Address1.ToString(), item.RxLevel1at2);
                SetNewRssiForLink(resultDb, queryTime, item, item.Address2.ToString(), item.RxLevel2at1);
            }
        }

        /// <summary>
        /// Adds or modifies an RSSI entry for the given link detail.
        /// </summary>
        /// <param name="resultDb">The databse handle.</param>
        /// <param name="queryTime">The time of the data aquisition (recorded with the data).</param>
        /// <param name="linkDetail">The link details to record.</param>
        /// <param name="adressToSearch">The host address to search for (and modify if found).</param>
        /// <param name="rssiToSet">The RSSI value to record.</param>
        private static void SetNewRssiForLink(QueryResultDatabaseContext resultDb, DateTime queryTime, ILinkDetail linkDetail, string adressToSearch, double rssiToSet)
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
            adressEntry.UnixTimeStamp = (ulong)queryTime.ToUniversalTime().Subtract(Program.UnixTimeStampBase).TotalSeconds;
        }
    }
}
