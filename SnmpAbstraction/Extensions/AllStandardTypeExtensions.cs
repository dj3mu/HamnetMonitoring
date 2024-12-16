using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Extension methods for all standard .NET types (except string).
    /// </summary>
    public static class AllStandardTypeExtensions
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Converts the string into a <see cref="PeeringState" /> enumeration value.
        /// </summary>
        /// <param name="stateString">The string to convert.</param>
        /// <returns>The enum mathcing the string or <see cref="PeeringState.Unknown" /> if the string cannot be converted.</returns>
        public static PeeringState ToBgpStateEnumeration(this string stateString)
        {
            if (string.IsNullOrWhiteSpace(stateString))
            {
                return PeeringState.Unknown;
            }

            if (!Enum.TryParse<PeeringState>(stateString, true, out PeeringState parsedState))
            {
                log.Warn($"Cannot parse string '{stateString}' as PeeringState enum. Returning '{PeeringState.Unknown}'");
                parsedState = PeeringState.Unknown;
            }

            return parsedState;
        }

        /// <summary>
        /// Puts the given element at the given index of the <see cref="IList{T}" />.
        /// </summary>
        /// <param name="list">The list to work on.</param>
        /// <param name="index">The index to put the element on.</param>
        /// <param name="newElement">The elment to put.</param>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <returns>The element that was previously at given index or default(T) if the list didn't yet contain sufficient elements.</returns>
        public static T PutAt<T>(this IList<T> list, int index, T newElement)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list), "The list to PutAt is null");
            }

            int missingElements = index - list.Count;
            if (missingElements >= 0)
            {
                // add the missing elements as "default"
                for (int i = 0; i < missingElements; i++)
                {
                    list.Add(default);
                }

                list.Add(newElement);

                return default;
            }
            else
            {
                T returnElement = list[index];
                list[index] = newElement;
                return returnElement;
            }
        }

        /// <summary>
        /// Converts the given list of strings to a <see cref="DeviceSupportedFeatures" /> enum.
        /// </summary>
        /// <param name="featuresList">The list of feature strings</param>
        /// <returns>The flags enum <see cref="DeviceSupportedFeatures" />.</returns>
        public static DeviceSupportedFeatures ToDeviceSupportedFeatures(this string[] featuresList)
        {
            DeviceSupportedFeatures dsf = featuresList.Select(x =>
            {
                if (!Enum.TryParse(x, true, out DeviceSupportedFeatures outenum))
                {
                    throw new ArgumentOutOfRangeException(nameof(featuresList), $"Feature '{x}' is not a known feature name. Supported names are {string.Join(", ", Enum.GetNames(typeof(DeviceSupportedFeatures)).Where(f => f != "None"))}");
                }

                return outenum;

            }).Aggregate((prev , next) => prev | next);

            return dsf;
        }

        /// <summary>
        /// Converts the integer array to a hex string.
        /// </summary>
        /// <param name="unitList">The integer array to convert.</param>
        /// <param name="separator">Optionally the separator character to use between ints.</param>
        /// <returns>Hex string representation of the int array.</returns>
        public static string ToHexString(this IEnumerable<uint> unitList, char separator = ':')
        {
            if (unitList == null)
            {
                throw new ArgumentNullException(nameof(unitList), "The list of uints to convert to HexString is null");
            }

            var uintArray = unitList as uint[] ?? unitList.ToArray();
            StringBuilder hex = new StringBuilder(uintArray.Length * 2);
            foreach (uint b in uintArray)
            {
                if (hex.Length > 0)
                {
                    hex.Append(separator);
                }

                hex.AppendFormat("{0:X2}", b);
            }

            return hex.ToString();
        }

        /// <summary>
        /// Converts the integer array to a hex string.
        /// </summary>
        /// <param name="byteList">The integer array to convert.</param>
        /// <param name="separator">Optionally the separator character to use between ints.</param>
        /// <returns>Hex string representation of the int array.</returns>
        public static string ToHexString(this IEnumerable<byte> byteList, char separator = ':')
        {
            if (byteList == null)
            {
                throw new ArgumentNullException(nameof(byteList), "The list of bytes to convert to HexString is null");
            }

            var uintArray = byteList as byte[] ?? byteList.ToArray();
            StringBuilder hex = new StringBuilder(uintArray.Length * 2);
            foreach (uint b in uintArray)
            {
                if (hex.Length > 0)
                {
                    hex.Append(separator);
                }

                hex.AppendFormat("{0:X2}", b);
            }

            return hex.ToString();
        }

        /// <summary>
        /// Converts the integer array to a hex string.
        /// </summary>
        /// <returns>The SNMP version matching the integer.</returns>
        public static SnmpVersion ToSnmpVersion(this int intVersion)
        {
            return intVersion switch
            {
                1 => SnmpVersion.Ver1,
                2 => SnmpVersion.Ver2,
                3 => SnmpVersion.Ver3,
                _ => throw new ArgumentOutOfRangeException(nameof(intVersion), "The integer {intVersion} cannot be converted to a valid SnmpVersion"),
            };
        }
    }
}
