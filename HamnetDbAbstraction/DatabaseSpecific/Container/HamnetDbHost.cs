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
        /// <param name="name">The name of the host.</param>
        /// <param name="type">The type of the host.</param>
        public HamnetDbHost(IPAddress address, string callsign, string name, string type)
        {
            this.Address = address;
            this.Callsign = callsign;
            this.Name = name;
            this.HostType = type;
        }

        /// <inheritdoc />
        public IPAddress Address { get; }

        /// <inheritdoc />
        public string Callsign { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string HostType { get; }
    }
}