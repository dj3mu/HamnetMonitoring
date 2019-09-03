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
        /// <param name="hostNameOrAddress">The host name or OP address of the device to query.</param>
        /// <param name="options">The options for the query.</param>
        /// <returns>An <see cref="IHamnetSnmpQuerier" /> that talks to the given address.</returns>
        public IHamnetSnmpQuerier Create(string hostNameOrAddress, IQuerierOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(hostNameOrAddress))
            {
                throw new ArgumentNullException(nameof(hostNameOrAddress), "host name or address is null, empty or white-space-only");
            }

            IPAddress address;
            if (!GetResolvedConnecionIPAddress(hostNameOrAddress, out address))
            {
                throw new HamnetSnmpException($"Host name or address '{hostNameOrAddress}' cannot be resolved to a valid IP address");
            }

            ISnmpLowerLayer lowerLayer = new SnmpLowerLayer(new IpAddress(address), options);

            var detector = new DeviceDetector(lowerLayer);
            var handler = detector.Detect();
            var querier = new HamnetSnmpQuerier(handler);

            return querier;
        }

        /// <summary>
        /// Resolve string as either IP or host name (derived from <see href="https://www.codeproject.com/Tips/440861/Resolving-a-hostname-in-Csharp-and-retrieving-IP-v" />).
        /// </summary>
        /// <param name="hostNameOrAddress">The string containing the host name or IP address.</param>
        /// <param name="resolvedIPAddress">The resolved IP address.</param>
        /// <returns></returns>
        private static bool GetResolvedConnecionIPAddress(string hostNameOrAddress, out IPAddress resolvedIPAddress)
        {
            bool isResolved = false;
            IPHostEntry hostEntry = null;
            IPAddress resolvIP = null;
            try
            {
                if (!IPAddress.TryParse(hostNameOrAddress, out resolvIP))
                {
                    hostEntry = Dns.GetHostEntry(hostNameOrAddress);

                    if (hostEntry != null && hostEntry.AddressList != null 
                                && hostEntry.AddressList.Length > 0)
                    {
                        if (hostEntry.AddressList.Length == 1)
                        {
                            resolvIP = hostEntry.AddressList[0];
                            isResolved = true;
                        }
                        else
                        {
                            foreach (IPAddress var in hostEntry.AddressList)
                            {
                                if (var.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    resolvIP = var;
                                    isResolved = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    isResolved = true;
                }
            }
            catch (Exception)
            {
                isResolved = false;
                resolvIP = null;
            }
            finally
            {
                resolvedIPAddress = resolvIP;
            }

            return isResolved;
        }
    }
}
