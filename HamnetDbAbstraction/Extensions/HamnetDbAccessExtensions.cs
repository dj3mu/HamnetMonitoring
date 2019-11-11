using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Extension methods for <see cref="IHamnetDbAccess" />.
    /// </summary>
    public static class HamnetDbAccessExtensions
    {
        /// <summary>
        /// Gets all unique <c>pairs</c> of hosts with monitoring flag set and which share the same subnet.<br/>
        /// Subnets with less than or more than two hosts will be discarded.
        /// </summary>
        /// <param name="hamnetDbAccess">The handle to access the database.</param>
        /// <returns>The dictionary mapping a subnet to its unique monitored host pair.</returns>
        public static IReadOnlyDictionary<IHamnetDbSubnet, IHamnetDbHosts> UniqueMonitoredHostPairsInSameSubnet(this IHamnetDbAccess hamnetDbAccess)
        {
            var hosts = hamnetDbAccess.QueryMonitoredHosts();
            var subnets = hamnetDbAccess.QuerySubnets();

            var association = subnets.AssociateHosts(hosts);

            var uniquePairs = association.Where(a => a.Value.Count == 2).ToDictionary(k => k.Key, v => v.Value);

            return uniquePairs;
        }

        /// <summary>
        /// Gets all unique <c>pairs</c> of hosts with monitoring flag set and which share the same subnet.<br/>
        /// Subnets with less than or more than two hosts will be discarded.
        /// </summary>
        /// <param name="hamnetDbAccess">The handle to access the database.</param>
        /// <param name="subnet">The subnet to return data for.</param>
        /// <returns>The dictionary mapping a subnet to its unique monitored host pair.</returns>
        public static IReadOnlyDictionary<IHamnetDbSubnet, IHamnetDbHosts> UniqueMonitoredHostPairsInSubnet(this IHamnetDbAccess hamnetDbAccess, IPNetwork subnet)
        {
            var hosts = hamnetDbAccess.QueryMonitoredHosts();
            var allSubnets = hamnetDbAccess.QuerySubnets();

            // filter out parents for which we have nested subnets
            var subnets = allSubnets.Where(s => !allSubnets.Any(a => (s.Subnet != a.Subnet) && s.Subnet.Contains(a.Subnet)));

            var association = subnets.AssociateHosts(hosts);

            var uniquePairs = association
                .Where(a => subnet.Contains(a.Key.Subnet) || subnet.Equals(a.Key.Subnet))
                .Where(a => a.Value.Count == 2)
                .ToDictionary(k => k.Key, v => v.Value);

            return uniquePairs;
        }
    }
}
