using System.Runtime.CompilerServices;
using SnmpSharpNet;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]
[assembly: InternalsVisibleTo("SnmpAbstractionTests")]
namespace SnmpAbstraction
{
    /// <summary>
    /// Factory class for creating SNMP queriers to specific devices.<br/>
    /// Access via singleton property <see cref="Instance" />.
    /// </summary>
    public class SnmpQuerierFactory
    {
        /// <summary>
        /// Prevent instantiation from outside.
        /// </summary>
        private SnmpQuerierFactory()
        {
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static SnmpQuerierFactory Instance { get; } = new SnmpQuerierFactory();

        /// <summary>
        /// Creates a new querier to the given address using the given options.
        /// </summary>
        /// <param name="address">The address of the to query.</param>
        /// <param name="options">The options for the query.</param>
        /// <returns>An <see cref="IHamnetSnmpQuerier" /> that talks to the given address.</returns>
        public IHamnetSnmpQuerier Create(IpAddress address, IQuerierOptions options = null)
        {
            ISnmpLowerLayer lowerLayer = new SnmpLowerLayer(address, options);

            var detector = new DeviceDetector(lowerLayer);

            throw new System.NotImplementedException();
        }
    }
}
