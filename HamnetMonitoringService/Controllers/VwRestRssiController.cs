using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestService.Database;
using RestService.Model;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Controller class for the &quot;vw_rest_rssi&quot; REST API
    /// </summary>
    [Route("vw_rest_rssi")]
    [ApiController]
    public class VwRestRssiController : ControllerBase
    {
#pragma warning disable IDE0052 // for future use
        private readonly ILogger logger;
#pragma warning restore

        private readonly QueryResultDatabaseContext dbContext;

        /// <summary>
        /// Instantiates the controller taking a logger.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        /// <param name="dbContext">The database context to get the data from.</param>
        public VwRestRssiController(ILogger<VwRestRssiController> logger, QueryResultDatabaseContext dbContext)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger), "The logger to use is null");
            this.dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext), "The database context to take the data from is null");
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rssi>>> GetRssi()
        {
            Program.RequestStatistics.LegacyVzwRssiRequests++;
            return await this.dbContext.RssiValues.AsQueryable().ToListAsync();
        }

        /// <summary>
        /// Implementation of GET /failing request.
        /// </summary>
        /// <returns>The results of the get /failing request.</returns>
        [HttpGet("failing")]
        public async Task<ActionResult<IEnumerable<RssiFailingQuery>>> GetFailingRssiQueries()
        {
            Program.RequestStatistics.LegacyVzwRssiFailingRequests++;
            return await this.dbContext.RssiFailingQueries.AsQueryable().ToListAsync();
        }

        /// <summary>
        /// Implementation of GET /failing request.
        /// </summary>
        /// <returns>The results of the get /failing request.</returns>
        [HttpGet("failing/timeout")]
        public async Task<ActionResult<IEnumerable<RssiFailingQuery>>> GetTimeoutFailingRssiQueries()
        {
            Program.RequestStatistics.LegacyVzwRssiFailingRequests++;
            return await this.dbContext.RssiFailingQueries.AsQueryable().Where(q => q.ErrorInfo.Contains("Timeout") || q.ErrorInfo.Contains("Request has reached maximum retries")).ToListAsync();
        }

        /// <summary>
        /// Implementation of GET /failing request.
        /// </summary>
        /// <returns>The results of the get /failing request.</returns>
        [HttpGet("failing/nontimeout")]
        public async Task<ActionResult<IEnumerable<RssiFailingQuery>>> GetNonTimeoutFailingRssiQueries()
        {
            Program.RequestStatistics.LegacyVzwRssiFailingRequests++;
            return await this.dbContext.RssiFailingQueries.AsQueryable().Where(q => !q.ErrorInfo.Contains("Timeout") && !q.ErrorInfo.Contains("Request has reached maximum retries")).ToListAsync();
        }
    }
}
