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
        /// Tries to convert the given version into a valid <see cref="SemanticVersion" />
        /// </summary>
        /// <param name="versionString">The version string to convert.</param>
        /// <param name="outputVersion">The result of the conversion.</param>
        /// <returns><c>true</c> if the converstion was successful.</returns>
        public static bool TryToSemanticVersion(this string versionString, out SemanticVersion outputVersion)
        {
            outputVersion = null;

            try
            {
                string[] splitVersion = versionString.Split('.', StringSplitOptions.None);

                int major = 0, minor = 0, patch = 0;
                string build = string.Empty, prerelease = string.Empty;

                if (splitVersion.Length < 1)
                {
                    log.Warn($"Trying to get SemanticVersion of string '{versionString}': Splitting didn't reveal even a major version");
                    return false;
                }

                major = Convert.ToInt32(splitVersion[0]);

                if (splitVersion.Length > 1)
                {
                    minor = Convert.ToInt32(splitVersion[1]);
                }

                if (splitVersion.Length > 2)
                {
                    patch = Convert.ToInt32(splitVersion[2]);
                }

                if (splitVersion.Length > 3)
                {
                    prerelease = splitVersion[3];
                }

                outputVersion = new SemanticVersion(major, minor, patch, prerelease, build);

                return true;
            }
            catch (System.FormatException ex)
            {
                log.Warn($"Trying to get SemanticVersion of string '{versionString}' ran into exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tries to convert the given version into a valid <see cref="SemanticVersion" />
        /// </summary>
        /// <param name="versionString">The version string to convert.</param>
        /// <returns>The converted <see cref="SemanticVersion" />.</returns>
        public static SemanticVersion ToSemanticVersion(this string versionString)
        {
            if (!versionString.TryToSemanticVersion(out SemanticVersion returnVersion))
            {
                throw new FormatException($"Cannot convert string '{versionString}' to a valid Semantic Version");
            }

            return returnVersion;
        }

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

            if (!SemanticVersion.TryParse(versionString, out SemanticVersion version))
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

            if (string.IsNullOrWhiteSpace(hexString))
            {
                return Array.Empty<byte>();
            }

            return hexString.Split(separator).Select(x => Convert.ToByte(x.Trim(), 16)).ToArray();
        }

        /// <summary>
        /// Converts a string to a <see cref="IanaInterfaceType" /> enum.
        /// </summary>
        /// <param name="interfaceTypeString">The string to convert.</param>
        /// <returns>The enum of the converstion result. Returns <see cref="IanaInterfaceType.NotAvailable" />
        /// if the input is invalid. Returns <see cref="IanaInterfaceType.Other" /> if the input is a valid string but cannot be convert.</returns>
        public static IanaInterfaceType ToIanaInterfaceType(this string interfaceTypeString)
        {
            if (interfaceTypeString == null)
            {
                throw new ArgumentNullException(nameof(interfaceTypeString), "The interface type string to convert is null");
            }

            if (string.IsNullOrWhiteSpace(interfaceTypeString))
            {
                return IanaInterfaceType.NotAvailable;
            }

            if (Enum.TryParse<IanaInterfaceType>(interfaceTypeString, true, out IanaInterfaceType interfaceType))
            {
                // simple enum parsing success
                return interfaceType;
            }

            return interfaceTypeString.ToUpperInvariant() switch
            {
                "ETHER" => IanaInterfaceType.EthernetCsmacd,
                "WLAN" => IanaInterfaceType.Ieee80211,
                _ => IanaInterfaceType.Other,
            };
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