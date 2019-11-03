using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HamnetDbAbstraction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SnmpAbstraction;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Class doing a link test between two hosts specified by IP or host name.
    /// </summary>
    internal class NetworkTest
    {
        private readonly IPNetwork network;

        private readonly ILogger logger;

        private readonly IConfiguration configuration;
        
        private IQuerierOptions querierOptions;

        /// <summary>
        /// Construct for a specific host.
        /// </summary>
        /// <param name="network">The network to test in CIDR or IP/Netmask notation.</param>
        /// <param name="logger">The logger to use.</param>
        /// <param name="configuration">The service configuration.</param>
        /// <param name="querierOptions">The options to the Hamnet querier.</param>
        public NetworkTest(string network, ILogger logger, IConfiguration configuration, IQuerierOptions querierOptions)
        {
            if (string.IsNullOrWhiteSpace(network))
            {
                throw new ArgumentNullException(nameof(network), "Network to test is null, empty or white-space-only");
            }

            IPNetwork subnet = null;
            if (!IPNetwork.TryParse(network, out subnet))
            {
                throw new ArgumentException($"Specified network '{network}' is not a valid IP network specification", nameof(network));
            }

            this.logger = logger;
            this.configuration = configuration;
            this.network = subnet;
            this.querierOptions = querierOptions ?? new FromUrlQueryQuerierOptions();
        }

        /// <summary>
        /// Asynchronuously executes the ping task.
        /// </summary>
        /// <returns></returns>
        internal async Task<ActionResult<IStatusReply>> Execute()
        {
            return await Task.Run(this.DoNetworkTest);
        }

        private ActionResult<IStatusReply> DoNetworkTest()
        {
            try
            {
                var matchingHosts = this.FetchSubnetsWithHostsFromHamnetDb(this.configuration.GetSection(HamnetDbProvider.HamnetDbSectionName));

                if ((matchingHosts == null) || (matchingHosts.Count == 0))
                {
                    throw new InvalidOperationException($"Cannot find any unique combination of hosts for network {this.network}");
                }

                List<ILinkDetailReply> returnList = new List<ILinkDetailReply>();
                foreach (var item in matchingHosts)
                {
                    returnList.AddRange(this.QueryLinkOfSingleSubnet(item).Details);
                }

                return new LinkDetailsReply(returnList);
            }
            catch(Exception ex)
            {
                return new ErrorReply(ex);
            }
        }

        /// <summary>
        /// Retrieves the list of subnets with their hosts to monitor from HamnetDB.
        /// </summary>
        /// <param name="hamnetDbConfig">The configuration section.</param>
        /// <returns>The list of subnets with their hosts to monitor from HamnetDB.</returns>
        private IReadOnlyDictionary<IHamnetDbSubnet, IHamnetDbHosts> FetchSubnetsWithHostsFromHamnetDb(IConfigurationSection hamnetDbConfig)
        {
            using(var accessor = HamnetDbProvider.Instance.GetHamnetDbFromConfiguration(hamnetDbConfig))
            {
                var uniquePairs = accessor.UniqueMonitoredHostPairsInSubnet(this.network);

                return uniquePairs;
            }
        }

        /// <summary>
        /// Queries the link for a single subnet.
        /// </summary>
        /// <param name="pair">The pair of hosts inside the subnet to query.</param>
        private ILinkDetailsReply QueryLinkOfSingleSubnet(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> pair)
        {
            IPAddress address1 = pair.Value.First().Address;
            IPAddress address2 = pair.Value.Last().Address;

            using(var querier = SnmpQuerierFactory.Instance.Create(address1, this.querierOptions))
            {
                var linkDetails = querier.FetchLinkDetails(address2.ToString());

                return new LinkDetailsReply(linkDetails);
            }
        }
    }
}
