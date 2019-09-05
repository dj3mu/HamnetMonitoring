using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using SemVersion;

namespace SnmpAbstraction
{
    /// <summary>
    /// Extensions to the <see cref="string" /> type.
    /// </summary>
    internal static class StringExtensions
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Parses a string as <see cref="SemanticVersion" /> and returns the result or the fallback version if the string is not parsable.
        /// </summary>
        /// <param name="versionString">The version string to parse. It must at least contain two dots (i.e. three sections).</param>
        /// <param name="fallBackVersion">The fallback version to use in case the version is not parseable.</param>
        /// <returns>A semantic version derived from the string or the <paramref name="fallBackVersion" /> if the string cannot be converted.</returns>
        public static SemanticVersion ParseAsSemanticVersionWithFallback(this string versionString, SemanticVersion fallBackVersion)
        {
            if (string.IsNullOrWhiteSpace(versionString))
            {
                return fallBackVersion;
            }

            SemanticVersion version;
            if (!SemanticVersion.TryParse(versionString, out version))
            {
                log.Warn($"ParseVersion: Non-parseable version string '{versionString}' found. Returning version 'null'");
                return fallBackVersion;
            }

            return version;
        }

        /// <summary>
        /// Converts the given string to a byte array by interpreting it as a Hex String.<br/>
        /// For example: &quot;b8:27:eb:97:b6:39&quot; will convert to byte[] { 0xb8, 0x27, 0xeb, 0x97, 0xb6, 0x39 }
        /// </summary>
        /// <param name="hexString">The hex string to convert.</param>
        /// <param name="separator">An optional setting of the separator character. If not set, defaults to ':'.</param>
        /// <returns>A byte array with the hex values of the string.</returns>
        public static byte[] HexStringToByteArray(this string hexString, char separator = ':')
        {
            if (hexString == null)
            {
                throw new ArgumentNullException(nameof(hexString), "The hex string to convert is null");
            }

            return hexString.Split(separator).Select(x => Convert.ToByte(x.Trim(), 16)).ToArray();
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
        
        /// <summary>
        /// Resolve string as either IP or host name (derived from <see href="https://www.codeproject.com/Tips/440861/Resolving-a-hostname-in-Csharp-and-retrieving-IP-v" />).
        /// </summary>
        /// <param name="hostNameOrAddress">The string containing the host name or IP address.</param>
        /// <param name="resolvedIPAddress">The resolved IP address.</param>
        /// <returns></returns>
        public static bool TryGetResolvedConnecionIPAddress(this string hostNameOrAddress, out IPAddress resolvedIPAddress)
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