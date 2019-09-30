using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Controller class for the &quot;vw_rest_rssi&quot; REST API
    /// </summary>
    [Route("api/status")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly ILogger logger;

        private readonly IConfiguration configuration;

        /// <summary>
        /// Instantiates the controller taking a logger.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        /// <param name="configuration">The configuration settings.</param>
        public StatusController(ILogger<RestController> logger, IConfiguration configuration)
        {
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
            return new ServerStatusReply
            {
                MaximumSupportedApiVersion = 1, // change this when creating new API version
                ServerVersion = Program.LibraryInformationalVersion,
                ProcessUptime = DateTime.Now - Process.GetCurrentProcess().StartTime
            };
        }
    }
}
