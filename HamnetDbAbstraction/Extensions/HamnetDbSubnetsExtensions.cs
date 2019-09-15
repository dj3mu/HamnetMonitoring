using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Extension methods for <see cref="IHamnetDbSubnets" /> or <see cref="IHamnetDbSubnet" />.
    /// </summary>
    public static class HamnetDbSubnetExtensions
    {
        /// <summary>
        /// Associates the given list of hosts with the subnets operated on.
        /// </summary>
        /// <param name="nets">The subnets to group the hosts to.</param>
        /// <param name="hosts">The hosts to match against and associate with the subnet.</param>
        /// <returns>A dictionary mapping a collection of hosts to a single subnet.</returns>
        public static IReadOnlyDictionary<IHamnetDbSubnet, IHamnetDbHosts> AssociateHosts(this IHamnetDbSubnets nets, IHamnetDbHosts hosts)
        {
            if (nets == null)
            {
                throw new ArgumentNullException(nameof(nets), "Network list is null");
            }

            if (hosts == null)
            {
                throw new ArgumentNullException(nameof(hosts), "Host list is null");
            }

            Dictionary<IHamnetDbSubnet, IHamnetDbHosts> returnDict = new Dictionary<IHamnetDbSubnet, IHamnetDbHosts>(nets.Count);
            foreach (IHamnetDbSubnet subnet in nets)
            {
                var hostsInSubnet = from host in hosts
                                    where subnet.Subnet.Contains(host.Address)
                                    select host;

                if (hostsInSubnet.Any())
                {
                    returnDict.Add(subnet, new HamnetDbHosts(hostsInSubnet));
                }
            }

            return returnDict;
        }
    }
}