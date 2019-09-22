using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        private readonly ILogger<MaintenanceService> logger;

        private readonly IConfiguration configuration;
        
        private bool disposedValue = false;

        private Timer timer;

        private QuerierOptions snmpQuerierOptions = QuerierOptions.Default;

        private object lockObject = new object();

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
            var maintenanceIntervalMins = this.configuration.GetSection(MaintenanceServiceSectionKey).GetValue<int>("MaintenanceIntervalMins");

            this.logger.LogInformation("Maintenance service is starting with an interval of {refreshIntervalSecs} minutes");

            this.timer = new Timer(DoMaintenance, null, TimeSpan.FromMinutes(maintenanceIntervalMins / 2), TimeSpan.FromMinutes(maintenanceIntervalMins));

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogDebug("Maintenance service is stopping.");

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
            if (Monitor.TryEnter(this.lockObject))
            {
                try
                {
                    this.logger.LogInformation("Maintenance: Still a NOP");
                }
                catch(Exception ex)
                {
                    this.logger.LogError($"Excpetion caught and ignored in maintenance thread: {ex.ToString()}");
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
    }
}
