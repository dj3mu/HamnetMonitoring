using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestService.Database;
using RestService.DataFetchingService;
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

        private readonly IFailureRetryFilteringDataHandler retryFeasibleHandler;

        /// <summary>
        /// Instantiates the controller taking a logger.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="dbContext">The databse context to use for retrieving the values to return.</param>
        /// <param name="retryFeasibleHandler">The handler to check whether retry is feasible.</param>
        public BgpController(ILogger<RestController> logger, IConfiguration configuration, QueryResultDatabaseContext dbContext, IFailureRetryFilteringDataHandler retryFeasibleHandler)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "The logger to use is null");
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "The configuration to use is null");
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), "The database context to take the data from is null");
            this.retryFeasibleHandler = retryFeasibleHandler ?? throw new ArgumentNullException(nameof(retryFeasibleHandler), "The retry feasible handler singleton has not been provided by DI engine");
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
        public async Task<ActionResult<IEnumerable<BgpFailingQueryWithPenaltyInfo>>> GetFailingRouters()
        {
            Program.RequestStatistics.ApiV1BgpFailingRequests++;

            return await this.dbContext.BgpFailingQueries
                .Select(ds => new BgpFailingQueryWithPenaltyInfo(ds, this.retryFeasibleHandler.QueryPenaltyDetails(QueryType.BgpQuery, IPAddress.Parse(ds.Host))))
                .OrderBy(ds => ds.PenaltyInfo != null ? ds.PenaltyInfo.OccuranceCount : uint.MaxValue)
                .ThenBy(ds => ds.Host)
                .ToListAsync();
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("monitoredRouters/failing/timeout")]
        public async Task<ActionResult<IEnumerable<BgpFailingQueryWithPenaltyInfo>>> GetFailingRoutersTimeout()
        {
            Program.RequestStatistics.ApiV1BgpFailingRequests++;

            return await this.dbContext.BgpFailingQueries
                .Where(q => q.ErrorInfo.Contains("Timeout") || q.ErrorInfo.Contains("timed out"))
                .Select(ds => new BgpFailingQueryWithPenaltyInfo(ds, this.retryFeasibleHandler.QueryPenaltyDetails(QueryType.BgpQuery, IPAddress.Parse(ds.Host))))
                .OrderBy(ds => ds.PenaltyInfo != null ? ds.PenaltyInfo.OccuranceCount : uint.MaxValue)
                .ThenBy(ds => ds.Host)
                .ToListAsync();
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("monitoredRouters/failing/nontimeout")]
        public async Task<ActionResult<IEnumerable<BgpFailingQueryWithPenaltyInfo>>> GetFailingRoutersNonTimeout()
        {
            Program.RequestStatistics.ApiV1BgpFailingRequests++;

            return await this.dbContext.BgpFailingQueries
                .Where(q => !q.ErrorInfo.Contains("Timeout") && !q.ErrorInfo.Contains("timed out"))
                .Select(ds => new BgpFailingQueryWithPenaltyInfo(ds, this.retryFeasibleHandler.QueryPenaltyDetails(QueryType.BgpQuery, IPAddress.Parse(ds.Host))))
                .OrderBy(ds => ds.PenaltyInfo != null ? ds.PenaltyInfo.OccuranceCount : uint.MaxValue)
                .ThenBy(ds => ds.Host)
                .ToListAsync();
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

        /// <summary>
        /// Extension of RssiFailingQuery with penalty info.
        /// </summary>
        public class BgpFailingQueryWithPenaltyInfo : BgpFailingQuery
        {
            /// <summary>
            /// Creates a new object from the underlying failing query.
            /// </summary>
            /// <param name="underlyingFailingQuery">The underlying failing query dataset.</param>
            /// <param name="penaltyInfo">The penalty info.</param>
            public BgpFailingQueryWithPenaltyInfo(BgpFailingQuery underlyingFailingQuery, ISingleFailureInfo penaltyInfo)
            {
                if(underlyingFailingQuery == null)
                {
                    throw new ArgumentNullException(nameof(underlyingFailingQuery), "The underlying failing query data is null");
                }

                this.Host = underlyingFailingQuery.Host;
                this.ErrorInfo = underlyingFailingQuery.ErrorInfo;
                this.TimeStamp = underlyingFailingQuery.TimeStamp;
                this.PenaltyInfo = penaltyInfo;
            }

            /// <summary>
            /// Gets the information about how much penalty this errors already receives.
            /// </summary>
            [JsonProperty(Order = 4)]
            public ISingleFailureInfo PenaltyInfo { get; }
        }
    }
}
