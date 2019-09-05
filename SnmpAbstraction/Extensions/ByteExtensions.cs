using System;
using System.Linq;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Extensions to the <see cref="byte" /> type.
    /// </summary>
    internal static class ByteExtensions
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Converts the given byte array to a dotted decimal string.<br/>
        /// For example: byte[] { 0xb8, 0x27, 0xeb, 0x97, 0xb6, 0x39 } will convert to "184.39.235.151.182.57".
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <returns>A byte array with the hex values of the string.</returns>
        public static string ToDottedDecimalString(this byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes), "The bytes array to convert to dotted-decimal is null");
            }

            return bytes.Aggregate(string.Empty, (aggregated, current) => 
            {
                return aggregated += (aggregated.Length > 0) ? ("." + ((int)current)) : ((int)current).ToString();
            });
        }

        /// <summary>
        /// Converts the given byte array to an OID.<br/>
        /// Optionally, the constructed OID will be appended at the end of the <paramref name="rootOid" />.paramref name="rootOid"<br/>
        /// For example: byte[] { 0xb8, 0x27, 0xeb, 0x97, 0xb6, 0x39 } will convert to "184.39.235.151.182.57".
        /// </summary>
        /// <param name="bytes">The byte array to convert.</param>
        /// <param name="rootOid">The root OID to append the converted byte array to.</param>
        /// <returns>A byte array with the hex values of the string.</returns>
        public static Oid ToDottedDecimalOid(this byte[] bytes, Oid rootOid = null)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes), "The bytes array to convert is null");
            }

            Oid returnOid = (Oid)rootOid?.Clone() ?? new Oid();

            returnOid.Add(bytes.ToDottedDecimalString());

            return returnOid;
        }
    }
}