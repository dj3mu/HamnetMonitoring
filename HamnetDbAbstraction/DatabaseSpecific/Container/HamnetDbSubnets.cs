using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Container for a list of HamnetDB subnet entries.
    /// </summary>
    internal class HamnetDbSubnets : IHamnetDbSubnets
    {
        private List<IHamnetDbSubnet> networks;

        /// <summary>
        /// Construct from a list of subnets.
        /// </summary>
        /// <param name="subnets"></param>
        public HamnetDbSubnets(IEnumerable<IHamnetDbSubnet> subnets)
        {
            if (subnets == null)
            {
                throw new ArgumentNullException(nameof(subnets), "The lists of networks is null");
            }

            this.networks = subnets as List<IHamnetDbSubnet> ?? subnets.ToList();
        }

        /// <inheritdoc />
        public int Count => this.networks.Count;

        /// <inheritdoc />
        public IEnumerator<IHamnetDbSubnet> GetEnumerator()
        {
            return this.networks.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.networks.GetEnumerator();
        }
    }
}