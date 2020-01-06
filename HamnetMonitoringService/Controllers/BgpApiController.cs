using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestService.Database;
using RestService.Model;
using SnmpAbstraction;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Controller class for the &quot;vw_rest_rssi&quot; REST API
    /// </summary>
    [Route("api/v1/bgp")]
    [ApiController]
    public class BgpController : ControllerBase
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
        public BgpController(ILogger<RestController> logger, IConfiguration configuration, QueryResultDatabaseContext dbContext)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger), "The logger to use is null");
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration), "The configuration to use is null");
            this.dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext), "The database context to take the data from is null");
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("peers/{host}")]
        public async Task<ActionResult<IStatusReply>> PingHost(string host, [FromQuery]FromUrlQueryQuerierOptions options)
        {
            Program.RequestStatistics.ApiV1BgpHostSpecificRequests++;

            IQuerierOptions optionsInUse = this.CreateOptions(options);

            return await new BgpPeersAction(WebUtility.UrlDecode(host), null, optionsInUse as FromUrlQueryQuerierOptions).Execute();
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("peers/{host}/{remoteIp}")]
        public async Task<ActionResult<IStatusReply>> PingHost(string host, string remoteIp, [FromQuery]FromUrlQueryQuerierOptions options)
        {
            Program.RequestStatistics.ApiV1BgpHostSpecificRequests++;

            IQuerierOptions optionsInUse = this.CreateOptions(options);

            return await new BgpPeersAction(WebUtility.UrlDecode(host), WebUtility.UrlDecode(remoteIp), optionsInUse as FromUrlQueryQuerierOptions).Execute();
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("monitoredRouters/{host?}")]
        public async Task<ActionResult<IEnumerable<IBgpPeerData>>> GetMonitoredRouters(string host)
        {
            Program.RequestStatistics.ApiV1BgpMonitoredRoutersRequests++;

            if (!string.IsNullOrWhiteSpace(host))
            {
                return await this.dbContext.BgpPeers.Where(p => p.LocalAddress == host).Select(p => new BgpPeerResponseData(p)).ToListAsync();
            }

            return await this.dbContext.BgpPeers.Select(p => new BgpPeerResponseData(p)).ToListAsync();
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("monitoredRouters/failing")]
        public async Task<ActionResult<IEnumerable<BgpFailingQuery>>> GetFailingRouters()
        {
            Program.RequestStatistics.ApiV1BgpFailingRequests++;

            return await this.dbContext.BgpFailingQueries.ToListAsync();
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("monitoredRouters/failing/timeout")]
        public async Task<ActionResult<IEnumerable<BgpFailingQuery>>> GetFailingRoutersTimeout()
        {
            Program.RequestStatistics.ApiV1BgpFailingRequests++;

            return await this.dbContext.BgpFailingQueries.Where(q => q.ErrorInfo.Contains("Timeout") || q.ErrorInfo.Contains("timed out")).ToListAsync();
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("monitoredRouters/failing/nontimeout")]
        public async Task<ActionResult<IEnumerable<BgpFailingQuery>>> GetFailingRoutersNonTimeout()
        {
            Program.RequestStatistics.ApiV1BgpFailingRequests++;

            return await this.dbContext.BgpFailingQueries.Where(q => !q.ErrorInfo.Contains("Timeout") && !q.ErrorInfo.Contains("timed out")).ToListAsync();
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
