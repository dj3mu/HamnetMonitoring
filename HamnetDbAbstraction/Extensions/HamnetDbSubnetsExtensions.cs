using System;
using System.Collections.Generic;
using System.Linq;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Extension methods for <see cref="IHamnetDbSubnets" /> or <see cref="IHamnetDbSubnet" />.
    /// </summary>
    public static class HamnetDbSubnetExtensions
    {
        private static readonly log4net.ILog log = HamnetDbAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Associates the given list of hosts with the subnets operated on.
        /// </summary>
        /// <param name="nets">The subnets to group the hosts to.</param>
        /// <param name="hosts">The hosts to match against and associate with the subnet.</param>
        /// <returns>A dictionary mapping a collection of hosts to a single subnet.</returns>
        public static IReadOnlyDictionary<IHamnetDbSubnet, IHamnetDbHosts> AssociateHosts(this IEnumerable<IHamnetDbSubnet> nets, IHamnetDbHosts hosts)
        {
            if (nets == null)
            {
                throw new ArgumentNullException(nameof(nets), "Network list is null");
            }

            if (hosts == null)
            {
                throw new ArgumentNullException(nameof(hosts), "Host list is null");
            }

            Dictionary<IHamnetDbSubnet, IHamnetDbHosts> returnDict = new Dictionary<IHamnetDbSubnet, IHamnetDbHosts>();
            foreach (IHamnetDbSubnet subnet in nets)
            {
                var hostsInSubnet = from host in hosts
                                    where subnet.Subnet.Contains(host.Address)
                                    select host;

                if (hostsInSubnet.Any())
                {
                    // due to comparer we might already have a parent subnet in the dict which we hereby delete
                    // for to replace it with the smaller subnet
                    var alreadyHandledParentSubnet = returnDict.Keys.FirstOrDefault(k => {
                        return k.Subnet.Contains(subnet.Subnet);
                    });
                    if (alreadyHandledParentSubnet != null)
                    {
                        log.Debug($"Replacing already stored parent '{alreadyHandledParentSubnet.Subnet}' of subnet '{subnet.Subnet}' with this, smaller subnet");
                        returnDict.Remove(alreadyHandledParentSubnet);
                    }

                    returnDict.Add(subnet, new HamnetDbHosts(hostsInSubnet));
                }
            }

            return returnDict;
        }
    }

    /// <summary>
    /// Comparer that returns true of the RHS subnet is contained in the LHS subnet.
    /// </summary>
    internal class SubnetContainedComparer : IEqualityComparer<IHamnetDbSubnet>
    {
        /// <inheritdoc />
        public bool Equals(IHamnetDbSubnet x, IHamnetDbSubnet y)
        {
            if (object.ReferenceEquals(x, y))
            {
                return true;
            }

            if (object.ReferenceEquals(x, null))
            {
                return object.ReferenceEquals(y, null);
            }

            return x.Subnet.Contains(y.Subnet);
        }

        /// <inheritdoc />
        public int GetHashCode(IHamnetDbSubnet obj)
        {
            // we hereby enforce usage of the Equals method.
            return 0;
        }
    }
}