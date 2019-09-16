using System.Net;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Interface to the data of a single host.
    /// </summary>
    public interface IHamnetDbHost
    {
        /// <summary>
        /// Gets the address of this host.
        /// </summary>
        IPAddress Address { get; }
    }
}