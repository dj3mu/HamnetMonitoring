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
    
        /// <summary>
        /// Gets the callsign of the host.
        /// </summary>
        string Callsign { get; }
    }
}