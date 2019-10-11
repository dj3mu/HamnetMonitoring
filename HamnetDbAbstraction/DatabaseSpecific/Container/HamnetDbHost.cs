using System.Net;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Container for a single host entry in HamnetDB.
    /// </summary>
    internal class HamnetDbHost : IHamnetDbHost
    {
        /// <summary>
        /// Contruct taking all parameters.
        /// </summary>
        /// <param name="address">The address of the host.</param>
        /// <param name="callsign">The callsign of the host.</param>
        public HamnetDbHost(IPAddress address, string callsign)
        {
            this.Address = address;
            this.Callsign = callsign;
        }

        /// <inheritdoc />
        public IPAddress Address { get; }

        /// <inheritdoc />
        public string Callsign { get; }
    }
}