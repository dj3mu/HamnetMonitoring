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
    [Route("api/v1/bgp")]
    [ApiController]
    public class BgpController : ControllerBase
    {
        private readonly ILogger logger;

        private readonly IConfiguration configuration;

        /// <summary>
        /// Instantiates the controller taking a logger.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        /// <param name="configuration">The configuration settings.</param>
        public BgpController(ILogger<RestController> logger, IConfiguration configuration)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger), "The logger to use is null");
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration), "The configuration to use is null");
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("peers/{host}")]
        public async Task<ActionResult<IStatusReply>> PingHost(string host, [FromQuery]FromUrlQueryQuerierOptions options)
        {
            return await new BgpPeersAction(WebUtility.UrlDecode(host), null, options).Execute();
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("peers/{host}/{remoteIp}")]
        public async Task<ActionResult<IStatusReply>> PingHost(string host, string remoteIp, [FromQuery]FromUrlQueryQuerierOptions options)
        {
            return await new BgpPeersAction(WebUtility.UrlDecode(host), WebUtility.UrlDecode(remoteIp), options).Execute();
        }
    }
}
