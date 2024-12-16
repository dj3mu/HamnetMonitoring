using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Container for a list of HamnetDB hosts.
    /// </summary>
    internal class HamnetDbHosts : IHamnetDbHosts
    {
        private readonly List<IHamnetDbHost> hosts;

        /// <summary>
        /// Construct from a list of hosts.
        /// </summary>
        /// <param name="hosts"></param>
        public HamnetDbHosts(IEnumerable<IHamnetDbHost> hosts)
        {
            if (hosts == null)
            {
                throw new ArgumentNullException(nameof(hosts), "The lists of hosts is null");
            }

            this.hosts = hosts as List<IHamnetDbHost> ?? hosts.ToList();
        }

        /// <inheritdoc />
        public int Count => this.hosts.Count;

        /// <inheritdoc />
        public IEnumerator<IHamnetDbHost> GetEnumerator()
        {
            return this.hosts.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.hosts.GetEnumerator();
        }
    }
}