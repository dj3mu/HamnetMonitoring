using System;
using System.Collections.Generic;
using System.Linq;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Implementation of the lower layer SNMP parts.<br/>
    /// Mainly encapsulates the necessary 
    /// /// </summary>
    internal static class SnmpLowerLayerExtensions
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        /// <summary>
        /// Queries the given OIDs and returns the value as string or null if the OID is not available.
        /// </summary>
        /// <param name="lowerLayer">The lower layer engine to extend.</param>
        /// <param name="oid">The OID to query.</param>
        /// <param name="what">Descriptive text what is being queried. Used for debug/error output</param>
        /// <returns>The OID's value as string or null if the OID is not available.</returns>
        public static string QueryAsString(this ISnmpLowerLayer lowerLayer, Oid oid, string what)
        {
            VbCollection result = lowerLayer.Query(oid);
            var retVal = result[oid]?.Value?.ToString();
            if (retVal == null)
            {
                log.Warn($"Querying '{what}' from '{lowerLayer.Address}' as string returned null");
            }

            return retVal;
        }

        /// <summary>
        /// Queries the given OIDs and returns the value as string or null if the OID is not available.
        /// </summary>
        /// <param name="lowerLayer">The lower layer engine to extend.</param>
        /// <param name="oid">The OID to query.</param>
        /// <param name="what">Descriptive text what is being queried. Used for debug/error output</param>
        /// <returns>The OID's value as int or <see cref="int.MinValue" /> if the OID is not available.</returns>
        public static int QueryAsInt(this ISnmpLowerLayer lowerLayer, Oid oid, string what)
        {
            VbCollection result = lowerLayer.Query(oid);

            int retVal = int.MinValue;
            if (!(result[oid]?.Value?.TryToInt(out retVal) ?? false))
            {
                log.Warn($"Querying '{what}' from '{lowerLayer.Address}' as string returned null");
            }

            return retVal;
        }

        /// <summary>
        /// Queries the given OIDs and returns the value as string or null if the OID is not available.
        /// </summary>
        /// <param name="lowerLayer">The lower layer engine to extend.</param>
        /// <param name="oids">The OIDs list to query.</param>
        /// <param name="what">Descriptive text what is being queried. Used for debug/error output</param>
        /// <returns>The OID's value as int or or <see cref="int.MinValue" /> if the OID is not available.</returns>
        public static Dictionary<Oid, int> QueryAsInt(this ISnmpLowerLayer lowerLayer, IEnumerable<Oid> oids, string what)
        {
            if (oids == null)
            {
                throw new ArgumentNullException(nameof(oids), "The array of oids to query is null");
            }
            
            VbCollection queryResult = lowerLayer.Query(oids);

            return queryResult.ToDictionary(qr => qr.Oid, qr =>
            {
                int retVal = int.MinValue;
                if (!(qr.Value?.TryToInt(out retVal) ?? false))
                {
                    log.Warn($"Querying '{what}' from '{lowerLayer.Address}' as Dictionary<Oid, int>: Conversion to int returned null for response '{qr}'");
                }

                return retVal;
            });
        }

        /// <summary>
        /// Queries the given OIDs and returns the value as OID or null if the OID is not available.
        /// </summary>
        /// <param name="lowerLayer">The lower layer engine to extend.</param>
        /// <param name="oid">The OID to query.</param>
        /// <param name="what">Descriptive text what is being queried. Used for debug/error output</param>
        /// <returns>The OID's value as OID or null if the OID is not available.</returns>
        public static Oid QueryAsOid(this ISnmpLowerLayer lowerLayer, Oid oid, string what)
        {
            VbCollection result = lowerLayer.Query(oid);
            var retVal = result[oid]?.Value as Oid;
            if (retVal == null)
            {
                log.Warn($"Querying '{what}' from '{lowerLayer.Address}' as OID returned null");
            }

            return retVal;
        }

        /// <summary>
        /// Queries the given OIDs and returns the value as <see cref="TimeSpan" /> or null if the OID is not available.
        /// </summary>
        /// <param name="lowerLayer">The lower layer engine to extend.</param>
        /// <param name="oid">The OID to query.</param>
        /// <param name="what">Descriptive text what is being queried. Used for debug/error output</param>
        /// <returns>The OID's value as <see cref="TimeSpan" /> or null if the OID is not available or cannot be parsed as <see cref="TimeSpan" />.</returns>
        public static TimeSpan? QueryAsTimeSpan(this ISnmpLowerLayer lowerLayer, Oid oid, string what)
        {
            VbCollection result = lowerLayer.Query(oid);
            var retVal = result[oid]?.Value as TimeTicks;
            if (retVal == null)
            {
                log.Warn($"Querying '{what}' from '{lowerLayer.Address}' as TimeTicks returned null");
                return null;
            }

            return TimeSpan.FromMilliseconds(retVal.Milliseconds);
        }
    }
}
