using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Container for a list of HamnetDB site entries.
    /// </summary>
    internal class HamnetDbSites : IHamnetDbSites
    {
        private readonly List<IHamnetDbSite> sites;

        /// <summary>
        /// Construct from a list of subnets.
        /// </summary>
        /// <param name="sites"></param>
        public HamnetDbSites(IEnumerable<IHamnetDbSite> sites)
        {
            if (sites == null)
            {
                throw new ArgumentNullException(nameof(sites), "The lists of sites is null");
            }

            this.sites = sites as List<IHamnetDbSite> ?? sites.ToList();
        }

        /// <inheritdoc />
        public int Count => this.sites.Count;

        /// <inheritdoc />
        public IEnumerator<IHamnetDbSite> GetEnumerator()
        {
            return this.sites.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.sites.GetEnumerator();
        }
    }
}