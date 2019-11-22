using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HamnetDbAbstraction;
using HamnetDbRest;
using InfluxDB.LineProtocol.Client;
using InfluxDB.LineProtocol.Payload;
using Microsoft.Extensions.Configuration;
using SnmpAbstraction;

namespace RestService.DataFetchingService
{
    /// <summary>
    /// Implementation of an <see cref="IAquiredDataHandler" /> that writes the results to an Influx time-series database.
    /// </summary>
    internal class InfluxDatabaseDataHandler : IAquiredDataHandler
    {
        private const string InfluxValueKey = "value";

        private const string InfluxRssiDatapointName = "RSSI";

        private const string InfluxLinkUptimeDatapointName = "LinkUptime";

        private const string InfluxDeviceUptimeDatapointName = "DeviceUptime";

        private const string InfluxHostTagName = "host";

        private const string InfluxSubnetTagName = "subnet";

        private const string InfluxCallTagName = "call";
        
        private const string InfluxCall2TagName = "call2";

        private const string InfluxDescriptionTagName = "desc";

        private static readonly log4net.ILog log = Program.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IConfiguration configuration;

        private IConfigurationSection influxConfiguration;

        private LineProtocolClient influxClient = null;

        private LineProtocolPayload currentPayload = null;

        private bool disposedValue = false;

        private object recordingLock = new object();

        /// <summary>
        /// Construct for the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration to construct for.</param>
        public InfluxDatabaseDataHandler(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.influxConfiguration = configuration.GetSection(Program.InfluxSectionKey);
            this.CreateInfluxClient();
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~InfluxDatabaseDataHandler()
        // {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        /// <inheritdoc />
        public string Name { get; } = "Influx Database";

        /// <inheritdoc />
        public void AquisitionFinished()
        {
            this.SendCurrentPayload();
        }

        /// <inheritdoc />
        public void PrepareForNewAquisition()
        {
            this.CreateNewPayload();
        }

        /// <inheritdoc />
        public void RecordRssiDetailsInDatabase(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData, ILinkDetails linkDetails, DateTime queryTime)
        {
            lock(this.recordingLock)
            {
                this.CreateNewPayload();

                string host1call = inputData.Value.First().Callsign?.ToUpperInvariant();
                string host2call = inputData.Value.Last().Callsign?.ToUpperInvariant();

                this.currentPayload.Add(
                    new LineProtocolPoint(
                        InfluxLinkUptimeDatapointName,
                        new Dictionary<string, object>
                        {
                            { InfluxValueKey, linkDetails.Details.First().LinkUptime }
                        },
                        new Dictionary<string, string>
                        {
                            { InfluxSubnetTagName, inputData.Key.Subnet.ToString() },
                            { InfluxCallTagName, host1call },
                            { InfluxCall2TagName, host2call },
                            { InfluxDescriptionTagName, $"{host1call} and {host2call}" }
                        },
                        queryTime.ToUniversalTime()));

                foreach (var item in linkDetails.Details)
                {
                    this.currentPayload.Add(
                        new LineProtocolPoint(
                            InfluxRssiDatapointName,
                            new Dictionary<string, object>
                            {
                                { InfluxValueKey, item.RxLevel1at2 }
                            },
                            new Dictionary<string, string>
                            {
                                { InfluxHostTagName, item.Address1.ToString() },
                                { InfluxSubnetTagName, inputData.Key.Subnet.ToString() },
                                { InfluxCallTagName, host1call },
                                { InfluxDescriptionTagName, $"{host1call} at {host2call}" }
                            },
                            queryTime.ToUniversalTime()));

                    this.currentPayload.Add(
                            new LineProtocolPoint(
                            InfluxRssiDatapointName,
                            new Dictionary<string, object>
                            {
                                { InfluxValueKey, item.RxLevel2at1 }
                            },
                            new Dictionary<string, string>
                            {
                                { InfluxHostTagName, item.Address2.ToString() },
                                { InfluxSubnetTagName, inputData.Key.Subnet.ToString() },
                                { InfluxCallTagName, host2call },
                                { InfluxDescriptionTagName, $"{host2call} at {host1call}" }
                            },
                            queryTime.ToUniversalTime()));
                }

                this.SendCurrentPayload();
            }
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
                catch(Exception ex)
                {
                    // we don not want to throw from an async task
                    log.Error($"Caught and ignored exception in async recording of details for {inputData.Key} @ {queryTime}: {ex.Message}", ex);
                }
            });
        }

        /// <inheritdoc />
        public void RecordFailingRssiQuery(Exception exception, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData)
        {
            // NOP here - as of now, no failing queries recorded in InfluxDB
        }

        /// <inheritdoc />
        public Task RecordFailingRssiQueryAsync(Exception exception, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData)
        {
            // NOP here - as of now, no failing queries recorded in InfluxDB
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void RecordFailingBgpQuery(Exception exception, IHamnetDbHost host)
        {
            // NOP here - as of now, no failing queries recorded in InfluxDB
        }

        /// <inheritdoc />
        public Task RecordFailingBgpQueryAsync(Exception exception, IHamnetDbHost host)
        {
            // NOP here - as of now, no failing queries recorded in InfluxDB
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void RecordDetailsInDatabase(IHamnetDbHost host, IBgpPeers peers, DateTime queryTime)
        {
            // NOP here - as of now, no failing queries recorded in InfluxDB
        }

        /// <inheritdoc />
        public Task RecordDetailsInDatabaseAsync(IHamnetDbHost host, IBgpPeers peers, DateTime queryTime)
        {
            // NOP here - as of now, no failing queries recorded in InfluxDB
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void RecordUptimesInDatabase(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData, ILinkDetails linkDetails, IEnumerable<IDeviceSystemData> systemDatas, DateTime queryTime)
        {
            lock(this.recordingLock)
            {
                this.CreateNewPayload();

                string host1call = inputData.Value.First().Callsign?.ToUpperInvariant();
                string host2call = inputData.Value.Last().Callsign?.ToUpperInvariant();

                this.currentPayload.Add(
                    new LineProtocolPoint(
                        InfluxLinkUptimeDatapointName,
                        new Dictionary<string, object>
                        {
                            { InfluxValueKey, linkDetails.Details.First().LinkUptime }
                        },
                        new Dictionary<string, string>
                        {
                            { InfluxSubnetTagName, inputData.Key.Subnet.ToString() },
                            { InfluxCallTagName, host1call },
                            { InfluxCall2TagName, host2call },
                            { InfluxDescriptionTagName, $"{host1call} and {host2call}" }
                        },
                        queryTime.ToUniversalTime()));

                foreach (var item in systemDatas)
                {
                    if (!item.Uptime.HasValue)
                    {
                        continue;
                    }

                    var itemDbHost = inputData.Value.FirstOrDefault(h => h.Address.Equals((IPAddress)item.DeviceAddress));
                    if (itemDbHost == null)
                    {
                        log.Error($"Cannot find address {item.DeviceAddress} in HamnetDB. Hence cannot provide call for that address. Skipping this device for recording of device uptime in InfluxDB");
                        continue;
                    }
        
                    string hostCall = itemDbHost.Callsign.ToUpperInvariant();

                    this.currentPayload.Add(
                        new LineProtocolPoint(
                            InfluxDeviceUptimeDatapointName,
                            new Dictionary<string, object>
                            {
                                { InfluxValueKey, item.Uptime.Value }
                            },
                            new Dictionary<string, string>
                            {
                                { InfluxHostTagName, item.DeviceAddress.ToString() },
                                { InfluxSubnetTagName, inputData.Key.Subnet.ToString() },
                                { InfluxCallTagName, hostCall },
                                { InfluxDescriptionTagName, $"System Uptime {hostCall}" }
                            },
                            queryTime.ToUniversalTime()));
                }

                this.SendCurrentPayload();
            }
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
                catch(Exception ex)
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
                    this.SendCurrentPayload();
                    this.influxClient = null; // it's not IDisposable
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                // TODO: große Felder auf Null setzen.

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Creates the Influx client or sets the field to null if Influx is not configured.
        /// </summary>
        private void CreateInfluxClient()
        {
            if (this.influxClient != null)
            {
                return;
            }

            string currentKey = "DatabaseUri";
            string databaseUri = this.influxConfiguration.GetValue<string>(currentKey);
            if (string.IsNullOrWhiteSpace(databaseUri))
            {
                throw new InvalidOperationException($"Influx database: No key '{currentKey}' in '{Program.InfluxSectionKey}' configration section");
            }

            currentKey = "DatabaseName";
            string databaseName = this.influxConfiguration.GetValue<string>(currentKey);
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new InvalidOperationException($"Influx database: No key '{currentKey}' in '{Program.InfluxSectionKey}' configration section");
            }

            string databaseUser = this.influxConfiguration.GetValue<string>("DatabaseUser");
            if (string.IsNullOrWhiteSpace(databaseUser))
            {
                databaseUser = null;
            }

            string databasePassword = this.influxConfiguration.GetValue<string>("DatabasePassword");
            if (string.IsNullOrWhiteSpace(databasePassword))
            {
                databasePassword = null;
            }

            if ((databaseUser == null) && (databasePassword != null))
            {
                throw new InvalidOperationException($"Influx database: Inconsistent Influx database configuration: Found password but no user name in '{Program.InfluxSectionKey}' configration section");
            }

            log.Info($"Initialized Influx reporting to URI '{databaseUri}', database '{databaseName}'");

            this.influxClient = new LineProtocolClient(new Uri(databaseUri), databaseName, databaseUser, databasePassword);
        }
 
         private void CreateNewPayload()
        {
            this.SendCurrentPayload();

            this.currentPayload = new LineProtocolPayload();
        }

        private void SendCurrentPayload()
        {
            if ((this.influxClient != null) && (this.currentPayload != null))
            {
                var result = this.influxClient.WriteAsync(this.currentPayload).Result;
                if (!result.Success)
                {
                    log.Error($"Error (ignored) writing Influx data: {result.ErrorMessage}");
                }
            }

            this.currentPayload = null;
        }
    }
}