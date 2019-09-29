using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Controller class for the &quot;vw_rest_rssi&quot; REST API
    /// </summary>
    [Route("api/v1/linktest")]
    [ApiController]
    public class LinkTestController : ControllerBase
    {
        private readonly ILogger logger;

        private readonly IConfiguration configuration;

        /// <summary>
        /// Instantiates the controller taking a logger.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        /// <param name="configuration">The configuration settings.</param>
        public LinkTestController(ILogger<RestController> logger, IConfiguration configuration)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger), "The logger to use is null");
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration), "The configuration to use is null");
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("ping/{host}")]
        public async Task<ActionResult<IStatusReply>> PingHost(string host)
        {
            return await new PingTest(WebUtility.UrlDecode(host)).Execute();
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("link/{host1}/{host2}")]
        public async Task<ActionResult<IStatusReply>> LinkTest(string host1, string host2, [FromQuery]FromUrlQueryQuerierOptions options)
        {
            return await new LinkTest(WebUtility.UrlDecode(host1), WebUtility.UrlDecode(host2), options).Execute();
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("network/{net}")]
        public async Task<ActionResult<IStatusReply>> NetworkTest(string net)
        {
            return await new NetworkTest(WebUtility.UrlDecode(net), this.logger, this.configuration, null).Execute();
        }
    }
}
