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
    
        /// <summary>
        /// Gets the name of the host.
        /// </summary>
        string Name { get; }
    
        /// <summary>
        /// Gets the Type of the host.
        /// </summary>
        string HostType { get; }
    }
}