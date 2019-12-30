using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SnmpAbstraction;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Class doing a link test between two hosts specified by IP or host name.
    /// </summary>
    internal class HostInfo
    {
        private readonly string host;

        private readonly ILogger logger;

        private readonly IConfiguration configuration;
        
        private IQuerierOptions querierOptions;

        /// <summary>
        /// Construct for a specific host.
        /// </summary>
        /// <param name="host">The host to get the info for.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The service configuration.</param>
        /// <param name="querierOptions">The options to the Hamnet querier.</param>
        public HostInfo(string host, ILogger logger, IConfiguration configuration, IQuerierOptions querierOptions)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentNullException(nameof(host), "Host to test is null, empty or white-space-only");
            }

            this.logger = logger;
            this.configuration = configuration;
            this.host = host;
            this.querierOptions = querierOptions ?? new FromUrlQueryQuerierOptions();
        }

        /// <summary>
        /// Asynchronuously executes the ping task.
        /// </summary>
        /// <returns></returns>
        internal async Task<ActionResult<IStatusReply>> Execute()
        {
            return await Task.Run(this.DoHostTest);
        }

        private ActionResult<IStatusReply> DoHostTest()
        {
            try
            {
                using(var querier = SnmpQuerierFactory.Instance.Create(this.host, this.querierOptions))
                {
                    var systemData = querier.SystemData;

                    return new HostInfoReply(querier.Address, systemData, querier.Api);
                }
            }
            catch(Exception ex)
            {
                return new ErrorReply(ex);
            }
        }
    }
}
