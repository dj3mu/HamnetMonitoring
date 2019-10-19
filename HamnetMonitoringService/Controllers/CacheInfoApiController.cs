using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SnmpAbstraction;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Controller class for the &quot;vw_rest_rssi&quot; REST API
    /// </summary>
    [Route("api/v1/cacheInfo")]
    [ApiController]
    public class CacheInfoApiController : ControllerBase
    {
        private readonly ILogger logger;

        private readonly IConfiguration configuration;

        /// <summary>
        /// Instantiates the controller taking a logger.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        /// <param name="configuration">The configuration settings.</param>
        public CacheInfoApiController(ILogger<RestController> logger, IConfiguration configuration)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger), "The logger to use is null");
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration), "The configuration to use is null");
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<ICacheData>>> GetAll()
        {
            Program.RequestStatistics.ApiV1CacheInfoRequests++;

            return await Task.Run(this.FetchCacheEntries);
        }

        /// <summary>
        /// Fetches the cache data and converts to an array.
        /// </summary>
        /// <returns>The result list.</returns>
        private ActionResult<IEnumerable<ICacheData>> FetchCacheEntries()
        {
            var cacheMaintenance = new CacheMaintenance(true);
            return cacheMaintenance.FetchEntryList().ToArray();
        }
    }
}
