using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HamnetDbAbstraction;
using HamnetDbRest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RestService.Database;
using RestService.Model;
using SnmpAbstraction;

namespace RestService.DataFetchingService
{
    /// <summary>
    /// Implementation of an <see cref="IAquiredDataHandler" /> that writes the results to the ResultDatabase.
    /// </summary>
    internal class ResultDatabaseDataHandler : IAquiredDataHandler
    {
        /// <summary>
        /// The metric name to use for the RSSI value.
        /// </summary>
        private const string RssiMetricName = "RSSI";

        /// <summary>
        /// The metric ID to use for the RSSI value.
        /// </summary>
        private const int RssiMetricId = 1;

        private static readonly log4net.ILog log = Program.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IConfiguration configuration;

        private readonly IConfigurationSection hamnetDbConfig;
        
        private bool disposedValue = false;

        /// <summary>
        /// Construct for the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration to construct for.</param>
        public ResultDatabaseDataHandler(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "configuration is null when creating a ResultDatabaseDataHandler");

            this.hamnetDbConfig = this.configuration.GetSection(Program.RssiAquisitionServiceSectionKey);
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~ResultDatabaseDataHandler()
        // {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        /// <inheritdoc />
        public string Name { get; } = "Result Database";

        /// <inheritdoc />
        public void AquisitionFinished()
        {
            // NOP for now
        }

        /// <inheritdoc />
        public void PrepareForNewAquisition()
        {
            using(var databaseContext = QueryResultDatabaseProvider.Instance.CreateContext())
            {
                if (hamnetDbConfig.GetValue<bool>("TruncateFailingQueries"))
                {
                    using (var transaction = databaseContext.Database.BeginTransaction())
                    {
                        databaseContext.Database.ExecuteSqlCommand("DELETE FROM RssiFailingQueries");
                        databaseContext.SaveChanges();
                        transaction.Commit();
                    }
                }
            }
        }

        /// <inheritdoc />
        public void RecordDetailsInDatabase(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData, ILinkDetails linkDetails, DateTime queryTime)
        {
            using(var databaseContext = QueryResultDatabaseProvider.Instance.CreateContext())
            {
                using (var transaction = databaseContext.Database.BeginTransaction())
                {
                    this.DoRecordDetailsInDatabase(databaseContext, inputData, linkDetails, DateTime.UtcNow);

                    this.DoDeleteFailingQuery(databaseContext, inputData.Key);
            
                    databaseContext.SaveChanges();

                    transaction.Commit();
                }
            }
        }

        /// <inheritdoc />
        public Task RecordDetailsInDatabaseAsync(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData, ILinkDetails linkDetails, DateTime queryTime)
        {
            return Task.Run(() =>
            {
                try
                {
                    this.RecordDetailsInDatabase(inputData, linkDetails, queryTime);
                }
                catch(Exception ex)
                {
                    // we don not want to throw from an async task
                    log.Error($"Caught and ignored exception in async recording of details for {inputData.Key} @ {queryTime}: {ex.Message}", ex);
                }
            });
        }

        /// <inheritdoc />
        public void RecordFailingQuery(Exception exception, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData)
        {
            using(var databaseContext = QueryResultDatabaseProvider.Instance.CreateContext())
            {
                using (var transaction = databaseContext.Database.BeginTransaction())
                {
                    this.DoRecordFailingQueryEntry(databaseContext, exception, inputData);

                    databaseContext.SaveChanges();

                    transaction.Commit();
                }
            }
        }
 
        /// <inheritdoc />
        public Task RecordFailingQueryAsync(Exception exception, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData)
        {
            return Task.Run(() =>
            {
                try
                {
                    this.RecordFailingQuery(exception, inputData);
                }
                catch(Exception ex)
                {
                    // we don not want to throw from an async task
                    log.Error($"Caught and ignored exception in async recording of failing query for {inputData.Key}: {ex.Message}", ex);
                }
            });
        }

        // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            this.Dispose(true);

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
        /// Records the link details in the database.
        /// </summary>
        /// <param name="databaseContext">The database context to work with.</param>
        /// <param name="inputData">The input data of the query.</param>
        /// <param name="linkDetails">The link details to record.</param>
        /// <param name="queryTime">The time of the data aquisition (recorded with the data).</param>
        private void DoRecordDetailsInDatabase(QueryResultDatabaseContext databaseContext, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData, ILinkDetails linkDetails, DateTime queryTime)
        {
            string host1call = inputData.Value.First().Callsign?.ToUpperInvariant();
            string host2call = inputData.Value.Last().Callsign?.ToUpperInvariant();

            foreach (var item in linkDetails.Details)
            {
                this.SetNewRssiForLink(databaseContext, inputData.Key, queryTime, item, item.Address1.ToString(), item.RxLevel1at2, host1call, $"{host1call} at {host2call}");
                this.SetNewRssiForLink(databaseContext, inputData.Key, queryTime, item, item.Address2.ToString(), item.RxLevel2at1 , host2call, $"{host2call} at {host1call}");
            }
        }
 
         /// <summary>
        /// Deletes an entry in the failing query table.
        /// </summary>
        /// <param name="databaseContext">The database context to work with.</param>
        /// <param name="subnet">The subnet which serves as key to the entry to delete.</param>
        private void DoDeleteFailingQuery(QueryResultDatabaseContext databaseContext, IHamnetDbSubnet subnet)
        {
            var failingSubnetString = subnet.Subnet.ToString();
            var entryToRemove = databaseContext.RssiFailingQueries.SingleOrDefault(e => e.Subnet == failingSubnetString);
            if (entryToRemove != null)
            {
                log.Debug($"Removing fail entry for subnet '{failingSubnetString}'");
                databaseContext.Remove(entryToRemove);
            }
        }

        /// <summary>
        /// Adds or modifies an RSSI entry for the given link detail.
        /// </summary>
        /// <param name="databaseContext">The database context to work with.</param>
        /// <param name="subnet">The subnet that is being recorded.</param>
        /// <param name="queryTime">The time of the data aquisition (recorded with the data).</param>
        /// <param name="linkDetail">The link details to record.</param>
        /// <param name="adressToSearch">The host address to search for (and modify if found).</param>
        /// <param name="rssiToSet">The RSSI value to record.</param>
        /// <param name="hostCall">The call of the foreign host.</param>
        /// <param name="description">The description for this value.</param>
        private void SetNewRssiForLink(QueryResultDatabaseContext databaseContext, IHamnetDbSubnet subnet, DateTime queryTime, ILinkDetail linkDetail, string adressToSearch, double rssiToSet, string hostCall, string description)
        {
            var adressEntry = databaseContext.RssiValues.Find(adressToSearch);
            if (adressEntry == null)
            {
                adressEntry = new Rssi
                {
                    ForeignId = adressToSearch,
                    Metric = RssiMetricName,
                    MetricId = RssiMetricId,
                    ParentSubnet = subnet.Subnet?.ToString(),
                    Description = description,
                    ForeignCall = hostCall
                };

                databaseContext.RssiValues.Add(adressEntry);
            }

            adressEntry.RssiValue = rssiToSet.ToString("0.0");

            // we're setting a couple of values here again so that migrated database will get the value added
            adressEntry.ParentSubnet = subnet.Subnet?.ToString();
            adressEntry.Description = description;
            adressEntry.ForeignCall = hostCall;

            adressEntry.TimeStampString = queryTime.ToUniversalTime().ToString("yyyy-MM-ddTHH\\:mm\\:sszzz");
            adressEntry.UnixTimeStamp = (ulong)queryTime.ToUniversalTime().Subtract(Program.UnixTimeStampBase).TotalSeconds;
        }
 
        /// <summary>
        /// Records a failing query.
        /// </summary>
        /// <param name="databaseContext">The database context to work with.</param>
        /// <param name="ex">The exception that caused the failure.</param>
        /// <param name="pair">The pair of hosts inside the subnet to query.</param>
        private void DoRecordFailingQueryEntry(QueryResultDatabaseContext databaseContext, Exception ex, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> pair)
        {
            var failingSubnetString = pair.Key.Subnet.ToString();
            var failEntry = databaseContext.RssiFailingQueries.Find(failingSubnetString);
            var hamnetSnmpEx = ex as HamnetSnmpException;
            if (failEntry == null)
            {
                failEntry = new RssiFailingQuery
                {
                    Subnet = failingSubnetString,
                    AffectedHosts = (hamnetSnmpEx != null) ? hamnetSnmpEx.AffectedHosts : pair.Value.Select(h => h.Address?.ToString()).ToArray()
                };

                databaseContext.RssiFailingQueries.Add(failEntry);
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
    }
}