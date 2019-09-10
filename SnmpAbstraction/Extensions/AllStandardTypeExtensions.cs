using System;
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
        /// Converts the integer array to a hex string.
        /// </summary>
        /// <param name="uintArray">The integer array to convert.</param>
        /// <param name="separator">Optionally the separator character to use between ints.</param>
        /// <returns>Hex string representation of the int array.</returns>
        public static string ToHexString(this uint[] uintArray, char separator = ':')
        {
            StringBuilder hex = new StringBuilder(uintArray.Length * 2);
            foreach (uint b in uintArray)
            {
                if (hex.Length > 0)
                {
                    hex.Append(":");
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
            switch (intVersion)
            {
                case 1:
                    return SnmpVersion.Ver1;

                case 2:
                    return SnmpVersion.Ver2;

                case 3:
                    return SnmpVersion.Ver3;

                default:
                    throw new ArgumentOutOfRangeException(nameof(intVersion), "The integer {intVersion} cannot be converted to a valid SnmpVersion");
            }
        }
    }
}
