using System.Net;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Interface to the data of a single subnet.
    /// </summary>
    public interface IHamnetDbSubnet
    {
        /// <summary>
        /// Gets the network data set of the subnet.
        /// </summary>
        IPNetwork Subnet { get; }
    }
}