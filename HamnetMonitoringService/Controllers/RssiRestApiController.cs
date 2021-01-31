using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestService.Database;
using RestService.DataFetchingService;
using RestService.Model;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Controller class for the &quot;vw_rest_rssi&quot; REST API
    /// </summary>
    [Route("api/v1/rssi")]
    [ApiController]
    public class RestController : ControllerBase
    {
        private readonly ILogger logger;

        private readonly QueryResultDatabaseContext dbContext;

        private readonly IFailureRetryFilteringDataHandler retryFeasibleHandler;

        /// <summary>
        /// Instantiates the controller taking a logger.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        /// <param name="dbContext">The database context to get the data from.</param>
        /// <param name="retryFeasibleHandler">The handler to check whether retry is feasible.</param>
        public RestController(ILogger<RestController> logger, QueryResultDatabaseContext dbContext, IFailureRetryFilteringDataHandler retryFeasibleHandler)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger), "The logger to use is null");
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), "The database context to take the data from is null");
            this.retryFeasibleHandler = retryFeasibleHandler ?? throw new ArgumentNullException(nameof(retryFeasibleHandler), "The retry feasible handler singleton has not been provided by DI engine");
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rssi>>> GetRssi(string host)
        {
            Program.RequestStatistics.ApiV1RssiRequests++;
            return await this.dbContext.RssiValues.AsQueryable().ToListAsync();
        }

        /// <summary>
        /// Implementation of GET /failing request.
        /// </summary>
        /// <returns>The results of the get /failing request.</returns>
        [HttpGet("failing")]
        public async Task<ActionResult<IEnumerable<RssiFailingQueryWithPenaltyInfo>>> GetFailingRssiQueries()
        {
            Program.RequestStatistics.ApiV1RssiFailingRequests++;
            return await this.dbContext.RssiFailingQueries.AsAsyncEnumerable()
                .Select(ds => new RssiFailingQueryWithPenaltyInfo(ds, this.retryFeasibleHandler.QueryPenaltyDetails(QueryType.RssiQuery, IPNetwork.Parse(ds.Subnet))))
                .OrderBy(ds => ds.PenaltyInfo != null ? ds.PenaltyInfo.OccuranceCount : uint.MaxValue)
                .ThenByDescending(ds => ds.PenaltyInfo != null ? ds.PenaltyInfo.LastOccurance : DateTime.MaxValue)
                .ThenByDescending(ds => ds.TimeStamp)
                .ThenBy(ds => ds.Subnet)
                .ToListAsync();
        }

        /// <summary>
        /// Implementation of GET /failing request.
        /// </summary>
        /// <returns>The results of the get /failing request.</returns>
        [HttpGet("failing/timeout")]
        public async Task<ActionResult<IEnumerable<RssiFailingQueryWithPenaltyInfo>>> GetTimeoutFailingRssiQueries()
        {
            Program.RequestStatistics.ApiV1RssiFailingRequests++;
            return await this.dbContext.RssiFailingQueries.AsQueryable()
                .Where(q => q.ErrorInfo.Contains("Timeout") || q.ErrorInfo.Contains("Request has reached maximum retries"))
                .AsAsyncEnumerable()
                .Select(ds => new RssiFailingQueryWithPenaltyInfo(ds, this.retryFeasibleHandler.QueryPenaltyDetails(QueryType.RssiQuery, IPNetwork.Parse(ds.Subnet))))
                .OrderBy(ds => ds.PenaltyInfo != null ? ds.PenaltyInfo.OccuranceCount : uint.MaxValue)
                .ThenByDescending(ds => ds.PenaltyInfo != null ? ds.PenaltyInfo.LastOccurance : DateTime.MaxValue)
                .ThenByDescending(ds => ds.TimeStamp)
                .ThenBy(ds => ds.Subnet)
                .ToListAsync();
        }

        /// <summary>
        /// Implementation of GET /failing request.
        /// </summary>
        /// <returns>The results of the get /failing request.</returns>
        [HttpGet("failing/nontimeout")]
        public async Task<ActionResult<IEnumerable<RssiFailingQueryWithPenaltyInfo>>> GetNonTimeoutFailingRssiQueries()
        {
            Program.RequestStatistics.ApiV1RssiFailingRequests++;
            return await this.dbContext.RssiFailingQueries.AsQueryable()
                .Where(q => !q.ErrorInfo.Contains("Timeout") && !q.ErrorInfo.Contains("Request has reached maximum retries"))
                .AsAsyncEnumerable()
                .Select(ds => new RssiFailingQueryWithPenaltyInfo(ds, this.retryFeasibleHandler.QueryPenaltyDetails(QueryType.RssiQuery, IPNetwork.Parse(ds.Subnet))))
                .OrderBy(ds => ds.PenaltyInfo != null ? ds.PenaltyInfo.OccuranceCount : uint.MaxValue)
                .ThenByDescending(ds => ds.PenaltyInfo != null ? ds.PenaltyInfo.LastOccurance : DateTime.MaxValue)
                .ThenByDescending(ds => ds.TimeStamp)
                .ThenBy(ds => ds.Subnet)
                .ToListAsync();
        }

        /// <summary>
        /// Extension of RssiFailingQuery with penalty info.
        /// </summary>
        public class RssiFailingQueryWithPenaltyInfo : RssiFailingQuery
        {
            /// <summary>
            /// Creates a new object from the underlying failing query.
            /// </summary>
            /// <param name="underlyingFailingQuery">The underlying failing query dataset.</param>
            /// <param name="penaltyInfo">The penalty info.</param>
            /// <remarks>
            /// Yes, this is code duplication with BgpRestApiController and a common base-class would make sense design-wise (is-a FailingQuery).
            /// But this has been added later and the underlying classes are Entity Framework Database Models.
            /// I'm simply scared of modifying them to have a common base class. Not sure if EF migration framework is able to handle this.
            /// </remarks>
            public RssiFailingQueryWithPenaltyInfo(RssiFailingQuery underlyingFailingQuery, ISingleFailureInfo penaltyInfo)
            {
                if(underlyingFailingQuery == null)
                {
                    throw new ArgumentNullException(nameof(underlyingFailingQuery), "The underlying failing query data is null");
                }

                this.AffectedHosts = underlyingFailingQuery.AffectedHosts;
                this.ErrorInfo = underlyingFailingQuery.ErrorInfo;
                this.Subnet = underlyingFailingQuery.Subnet;
                this.TimeStamp = underlyingFailingQuery.TimeStamp;
                this.PenaltyInfo = underlyingFailingQuery.PenaltyInfo;

                if (penaltyInfo != null)
                {
                    // only replacing the object that has potentially been retrieved from database
                    // if we have a more recent one directly from handler.
                    if (this.PenaltyInfo == null)
                    {
                        // we have not penalty info -> unconditionally set the new one
                        this.PenaltyInfo = penaltyInfo;
                    }
                    else if (this.PenaltyInfo.LastOccurance <= penaltyInfo.LastOccurance)
                    {
                        // we have a penalty info -> only set if new one is newer or same
                        this.PenaltyInfo = penaltyInfo;
                    }
                }
            }
        }
    }
}
