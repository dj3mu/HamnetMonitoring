using System;
using System.Net;
using System.Net.Sockets;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Factory class for creating SNMP queriers to specific devices.<br/>
    /// Access via singleton property <see cref="Instance" />.
    /// </summary>
    public class SnmpQuerierFactory
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        /// <param name="hostNameOrAddress">The host name or IP address of the device to query.</param>
        /// <param name="options">The options for the query.</param>
        /// <returns>An <see cref="IHamnetQuerier" /> that talks to the given address.</returns>
        public IHamnetQuerier Create(string hostNameOrAddress, IQuerierOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(hostNameOrAddress))
            {
                throw new ArgumentNullException(nameof(hostNameOrAddress), "host name or address is null, empty or white-space-only");
            }

            IPAddress address;
            if (!hostNameOrAddress.TryGetResolvedConnecionIPAddress(out address))
            {
                throw new HamnetSnmpException($"Host name or address '{hostNameOrAddress}' cannot be resolved to a valid IP address");
            }

            return this.Create(address, options);
        }

        /// <summary>
        /// Creates a new querier to the given address using the given options.
        /// </summary>
        /// <param name="address">The IP address of the device to query.</param>
        /// <param name="options">The options for the query.</param>
        /// <returns>An <see cref="IHamnetQuerier" /> that talks to the given address.</returns>
        public IHamnetQuerier Create(IPAddress address, IQuerierOptions options = null)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address), "address is null");
            }

            ISnmpLowerLayer lowerLayer = new SnmpLowerLayer(new IpAddress(address), options);

            var detector = new DeviceDetector(lowerLayer);
            var handler = detector.Detect();

            if (handler == null)
            {
                var errorInfo = $"Cannot obtain a feasible device handler for device '{address}'";
                log.Error(errorInfo);
                throw new HamnetSnmpException(errorInfo);
            }

            var querier = new HamnetQuerier(handler, lowerLayer.Options);

            return querier;
        }
    }
}
