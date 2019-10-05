using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestService.Database;
using RestService.DataFetchingService;
using SnmpAbstraction;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Controller class for the &quot;vw_rest_rssi&quot; REST API
    /// </summary>
    [Route("api/status")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly ILogger logger;

        private readonly IConfiguration configuration;

        private readonly QueryResultDatabaseContext dbContext;

        /// <summary>
        /// Instantiates the controller taking a logger.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="dbContext">The query result database context to use.</param>
        public StatusController(ILogger<RestController> logger, IConfiguration configuration, QueryResultDatabaseContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), "The database context is null");
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger), "The logger to use is null");
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration), "The configuration to use is null");
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet]
        public async Task<ActionResult<IServerStatusReply>> Get()
        {
            return await Task.Run(this.GetVersionInformation);
        }

        /// <summary>
        /// Collects and returns the version information.
        /// </summary>
        /// <returns>The collected version information.</returns>
        private ActionResult<IServerStatusReply> GetVersionInformation()
        {
            var myProcess = Process.GetCurrentProcess();
            var reply = new ServerStatusReply
            {
                MaximumSupportedApiVersion = 1, // change this when creating new API version
                ServerVersion = Program.LibraryInformationalVersion,
                ProcessUptime = DateTime.UtcNow - myProcess.StartTime,
                ProcessCpuTime = myProcess.TotalProcessorTime,
                ProcessWorkingSet = myProcess.WorkingSet64,
                ProcessPrivateSet = myProcess.PrivateMemorySize64,
                ProcessThreads = myProcess.Threads.Count,
                ProcessStartTime = myProcess.StartTime,
            };

            this.AddConfiguration(reply, DataAquisitionService.AquisitionServiceSectionKey);
            this.AddConfiguration(reply, MaintenanceService.MaintenanceServiceSectionKey);
            this.AddConfiguration(reply, DataAquisitionService.InfluxSectionKey);
            this.AddConfiguration(reply, "ConnectionStrings");

            var statusTableRow = this.dbContext.MonitoringStatus.First();

            var queryResultStats = new DatabaseStatistic()
            {
                { "UniqueRssiValues", this.dbContext.RssiValues.Count().ToString() },
                { "TotalFailures", this.dbContext.RssiFailingQueries.Count().ToString() },
                { "TimeoutFailures", this.dbContext.RssiFailingQueries.Where(q => q.ErrorInfo.Contains("Timeout") || q.ErrorInfo.Contains("Request has reached maximum retries")).Count().ToString() },
                { "NonTimeoutFailures", this.dbContext.RssiFailingQueries.Where(q => !q.ErrorInfo.Contains("Timeout") && !q.ErrorInfo.Contains("Request has reached maximum retries")).Count().ToString() },
                { "LastAquisitionStart", statusTableRow.LastQueryStart.ToString("yyyy-MM-ddTHH\\:mm\\:sszzz") },
                { "LastAquisitionEnd", statusTableRow.LastQueryEnd.ToString("yyyy-MM-ddTHH\\:mm\\:sszzz") },
                { "LastMaintenanceStart", statusTableRow.LastMaintenanceStart.ToString("yyyy-MM-ddTHH\\:mm\\:sszzz") },
                { "LastMaintenanceEnd", statusTableRow.LastMaintenanceEnd.ToString("yyyy-MM-ddTHH\\:mm\\:sszzz") },
            };

            reply.Add("ResultDatabase", queryResultStats);

            var cacheMaintenance = new CacheMaintenance(true /* we don't want to modify anything - so set dry-run to be sure */);
            reply.Add("CacheDatabase", new DatabaseStatistic(cacheMaintenance.CacheStatistics()));

            var devDbMaintenance = new DeviceDatabaseMaintenance(true /* we don't want to modify anything - so set dry-run to be sure */);
            reply.Add("DeviceDatabase", new DatabaseStatistic(devDbMaintenance.CacheStatistics()));

            return reply;
        }

        /// <summary>
        /// Adds the given configuration section using given section key.
        /// </summary>
        /// <param name="reply">The reply to receive the section settings.</param>
        /// <param name="sectionKey">The status output key to use for the section values.</param>
        private void AddConfiguration(ServerStatusReply reply, string sectionKey)
        {
            ConfigurationInfo configuration = new ConfigurationInfo();
            foreach (var item in this.configuration.GetSection(sectionKey).GetChildren())
            {
                configuration.Add(item.Key, item.Key.Contains("password", StringComparison.InvariantCultureIgnoreCase) ? "***" : item.Value);
            }

            reply.Add(sectionKey, configuration);
        }
    }
}
