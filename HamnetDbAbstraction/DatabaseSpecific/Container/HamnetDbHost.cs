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
        public HamnetDbHost(IPAddress address)
        {
            this.Address = address;
        }

        /// <inheritdoc />
        public IPAddress Address { get; }
    }
}