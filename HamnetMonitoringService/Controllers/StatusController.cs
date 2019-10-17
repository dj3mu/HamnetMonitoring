using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HamnetDbAbstraction;
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
        private const string PasswordReplacementString = "***";

        private static readonly Regex PasswordReplaceRegex = new Regex(@"((Pw|Pass|Secret|Ui|User|Server)\w*=).*?;", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private static readonly Regex CompletlyHideKeyRegex = new Regex(@"Pass.*|DatabaseUri", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

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

            this.AddConfiguration(reply, Program.RssiAquisitionServiceSectionKey);
            this.AddConfiguration(reply, MaintenanceService.MaintenanceServiceSectionKey);
            this.AddConfiguration(reply, Program.InfluxSectionKey);
            this.AddConfiguration(reply, QueryResultDatabaseProvider.ResultDatabaseSectionName);
            this.AddConfiguration(reply, HamnetDbProvider.HamnetDbSectionName);
            this.AddConfiguration(reply, "CacheDatabase");
            this.AddConfiguration(reply, "DeviceDatabase");
            this.AddConfiguration(reply, Program.MonitoringAccountsSectionKey, Program.BgpAccountSectionKey);

            var statusTableRow = this.dbContext.MonitoringStatus.First();

            var queryResultStats = new DatabaseStatistic()
            {
                { "UniqueRssiValues", this.dbContext.RssiValues.Count().ToString() },
                { "TotalFailures", this.dbContext.RssiFailingQueries.Count().ToString() },
                { "TimeoutFailures", this.dbContext.RssiFailingQueries.Where(q => q.ErrorInfo.Contains("Timeout") || q.ErrorInfo.Contains("Request has reached maximum retries")).Count().ToString() },
                { "NonTimeoutFailures", this.dbContext.RssiFailingQueries.Where(q => !q.ErrorInfo.Contains("Timeout") && !q.ErrorInfo.Contains("Request has reached maximum retries")).Count().ToString() },
                { "LastRssiAquisitionStart", statusTableRow.LastRssiQueryStart.ToString("yyyy-MM-ddTHH\\:mm\\:sszzz") },
                { "LastRssiAquisitionEnd", statusTableRow.LastRssiQueryEnd.ToString("yyyy-MM-ddTHH\\:mm\\:sszzz") },
                { "LastBgpAquisitionStart", statusTableRow.LastBgpQueryStart.ToString("yyyy-MM-ddTHH\\:mm\\:sszzz") },
                { "LastBgpAquisitionEnd", statusTableRow.LastBgpQueryEnd.ToString("yyyy-MM-ddTHH\\:mm\\:sszzz") },
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
        /// <param name="subSectionKey">The status output sub-section key to use for the sub-section values.</param>
        private void AddConfiguration(ServerStatusReply reply, string sectionKey, string subSectionKey)
        {
            ConfigurationInfo configuration = new ConfigurationInfo();
            foreach (var item in this.configuration.GetSection(sectionKey).GetSection(subSectionKey).GetChildren())
            {
                string valueToAdd = item.Value;
                if (CompletlyHideKeyRegex.IsMatch(item.Key))
                {
                    valueToAdd = PasswordReplacementString;
                }
                else if (item.Key.Contains("connection", StringComparison.InvariantCultureIgnoreCase))
                {
                    valueToAdd = PasswordReplaceRegex.Replace(valueToAdd, $"$1{PasswordReplacementString};");
                }

                configuration.Add(item.Key, valueToAdd);
            }

            reply.Add($"{sectionKey}.{subSectionKey}", configuration);
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
                string valueToAdd = item.Value;
                if (CompletlyHideKeyRegex.IsMatch(item.Key))
                {
                    valueToAdd = PasswordReplacementString;
                }
                else if (item.Key.Contains("connection", StringComparison.InvariantCultureIgnoreCase))
                {
                    valueToAdd = PasswordReplaceRegex.Replace(valueToAdd, $"$1{PasswordReplacementString};");
                }

                configuration.Add(item.Key, valueToAdd);
            }

            reply.Add(sectionKey, configuration);
        }
    }
}
