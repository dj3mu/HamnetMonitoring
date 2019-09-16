using System.Net;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Container for a single subnet entry in HamnetDB.
    /// </summary>
    internal class HamnetDbSubnet : IHamnetDbSubnet
    {
        /// <summary>
        /// Contruct taking all parameters.
        /// </summary>
        /// <param name="subnet">The network container of the subnet.</param>
        public HamnetDbSubnet(IPNetwork subnet)
        {
            this.Subnet = subnet;
        }

        /// <inheritdoc />
        public IPNetwork Subnet { get; }
    }
}