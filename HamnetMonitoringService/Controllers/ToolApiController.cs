using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestService.Database;
using SnmpAbstraction;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Controller class for the &quot;vw_rest_rssi&quot; REST API
    /// </summary>
    [Route("api/v1/tools")]
    [ApiController]
    public class ToolController : ControllerBase
    {
        private readonly ILogger logger;

        private readonly IConfiguration configuration;

        private readonly QueryResultDatabaseContext dbContext;

        /// <summary>
        /// Instantiates the controller taking a logger.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="dbContext">The databse context to use for retrieving the values to return.</param>
        public ToolController(ILogger<RestController> logger, IConfiguration configuration, QueryResultDatabaseContext dbContext)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger), "The logger to use is null");
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration), "The configuration to use is null");
            this.dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext), "The database context to take the data from is null");
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("traceroute/{fromHost}/{toHost}/{count?}")]
        public async Task<ActionResult<IStatusReply>> TracerouteHost(string fromHost, string toHost, int count, [FromQuery]FromUrlQueryQuerierOptions options)
        {
            Program.RequestStatistics.ApiV1TraceRouteRequests++;

            IQuerierOptions optionsInUse = this.CreateOptions(options);

            return await new TracerouteAction(WebUtility.UrlDecode(fromHost), WebUtility.UrlDecode(toHost), count, optionsInUse as FromUrlQueryQuerierOptions).Execute();
        }

        /// <summary>
        /// Creates the querier options prioritizing query URL over the confiugration settings.
        /// </summary>
        /// <param name="options">The options from query URL.</param>
        /// <returns>The assembled options to use.</returns>
        private IQuerierOptions CreateOptions(FromUrlQueryQuerierOptions options)
        {
            FromUrlQueryQuerierOptions optionsInUse = options;
            
            var monitoringAccountsSection = this.configuration.GetSection(Program.MonitoringAccountsSectionKey).GetSection(Program.BgpAccountSectionKey);

            var loginUserName = monitoringAccountsSection.GetValue<string>("User");
            if (string.IsNullOrWhiteSpace(optionsInUse.LoginUser) && !string.IsNullOrWhiteSpace(loginUserName))
            {
                optionsInUse.LoginUser = loginUserName;
            }

            var loginPassword = monitoringAccountsSection.GetValue<string>("Password");
            if (string.IsNullOrWhiteSpace(optionsInUse.LoginPassword) && !string.IsNullOrWhiteSpace(loginPassword))
            {
                optionsInUse.LoginPassword = loginPassword;
            }

            return optionsInUse;
        }
    }
}
