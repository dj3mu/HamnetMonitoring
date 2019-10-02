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
    [Route("api/v1/rssi")]
    [ApiController]
    public class RestController : ControllerBase
    {
        private readonly ILogger logger;

        private readonly QueryResultDatabaseContext dbContext;

        /// <summary>
        /// Instantiates the controller taking a logger.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        /// <param name="dbContext">The database context to get the data from.</param>
        public RestController(ILogger<RestController> logger, QueryResultDatabaseContext dbContext)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger), "The logger to use is null");
            this.dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext), "The database context to take the data from is null");
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("{host?}")]
        public async Task<ActionResult<IEnumerable<Rssi>>> GetRssi(string host)
        {
            return await this.dbContext.RssiValues.ToListAsync();
        }

        /// <summary>
        /// Implementation of GET /failing request.
        /// </summary>
        /// <returns>The results of the get /failing request.</returns>
        [HttpGet("failing")]
        public async Task<ActionResult<IEnumerable<RssiFailingQuery>>> GetFailingRssiQueries()
        {
            return await this.dbContext.RssiFailingQueries.ToListAsync();
        }

        /// <summary>
        /// Implementation of GET /failing request.
        /// </summary>
        /// <returns>The results of the get /failing request.</returns>
        [HttpGet("failing/timeout")]
        public async Task<ActionResult<IEnumerable<RssiFailingQuery>>> GetTimeoutFailingRssiQueries()
        {
            return await this.dbContext.RssiFailingQueries.Where(q => q.ErrorInfo.Contains("Timeout") || q.ErrorInfo.Contains("Request has reached maximum retries")).ToListAsync();
        }

        /// <summary>
        /// Implementation of GET /failing request.
        /// </summary>
        /// <returns>The results of the get /failing request.</returns>
        [HttpGet("failing/nontimeout")]
        public async Task<ActionResult<IEnumerable<RssiFailingQuery>>> GetNonTimeoutFailingRssiQueries()
        {
            return await this.dbContext.RssiFailingQueries.Where(q => !q.ErrorInfo.Contains("Timeout") && !q.ErrorInfo.Contains("Request has reached maximum retries")).ToListAsync();
        }
    }
}
