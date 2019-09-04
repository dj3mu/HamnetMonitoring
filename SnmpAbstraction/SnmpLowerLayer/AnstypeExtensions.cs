using System;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Extension methods for the <see cref="AsnType" />.
    /// </summary>
    public static class AnstypeExtensions
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Converts the 
        /// </summary>
        /// <param name="asnType">The <see cref="AsnType" /> to convert.</param>
        /// <returns>Integer representation of the <see cref="AsnType" />.</returns>
        public static int ToInt(this AsnType asnType)
        {
            if (asnType == null)
            {
                throw new ArgumentNullException(nameof(asnType), "The AsnType to convert to Integer is null");
            }
            
            if (asnType.Type != AsnType.INTEGER)
            {
                throw new HamnetSnmpException($"Cannot convert an ASN Type '{SnmpConstants.GetTypeName(asnType.Type)}' of value '{asnType.ToString()}' to an integer: Non-matching type");
            }

            int convertedValue;
            if (!int.TryParse(asnType.ToString(), out convertedValue))
            {
                throw new HamnetSnmpException($"Cannot convert an ASN Type '{SnmpConstants.GetTypeName(asnType.Type)}' of value '{asnType.ToString()}' to an integer: Value malformatted or out of range");
            }

            return convertedValue;
        }

        /// <summary>
        /// Converts the 
        /// </summary>
        /// <param name="asnType">The <see cref="AsnType" /> to convert.</param>
        /// <param name="converted">Returs the integer representation of the <see cref="AsnType" />.</param>
        /// <returns><c>true</c> if the conversion was successful. Otherwise <c>false</c>.</returns>
        public static bool TryToInt(this AsnType asnType, out int converted)
        {
            if (asnType == null)
            {
                throw new ArgumentNullException(nameof(asnType), "The AsnType to convert to Integer is null");
            }
            
            converted = int.MinValue;
            if (asnType.Type != AsnType.INTEGER)
            {
                log.Warn($"Cannot convert an ASN Type '{SnmpConstants.GetTypeName(asnType.Type)}' of value '{asnType.ToString()}' to an integer: Non-matching type");
                return false;
            }

            if (!int.TryParse(asnType.ToString(), out converted))
            {
                log.Warn($"Cannot convert an ASN Type '{SnmpConstants.GetTypeName(asnType.Type)}' of value '{asnType.ToString()}' to an integer: Value malformatted or out of range");
                return false;
            }

            return true;
        }
    }
}
