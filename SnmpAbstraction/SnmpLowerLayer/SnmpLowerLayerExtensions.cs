using System;
using System.Net;
using System.Runtime.CompilerServices;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Implementation of the lower layer SNMP parts.<br/>
    /// Mainly encapsulates the necessary 
    /// /// </summary>
    internal static class SnmpLowerLayerExtensions
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        /// <summary>
        /// Queries the given OIDs and returns the value as string or null if the OID is not available.
        /// </summary>
        /// <param name="lowerLayer">The lower layer engine to extend.</param>
        /// <param name="oid">The OID to query.</param>
        /// <param name="what">Descriptive text what is being queried. Used for debug/error output</param>
        /// <returns>The OID's value as string or null if the OID is not available.</returns>
        public static string QueryAsString(this ISnmpLowerLayer lowerLayer, Oid oid, string what)
        {
            try
            {
                VbCollection result = lowerLayer.Query(oid);
                return result[0]?.Value?.ToString();
            }
            catch(SnmpException snmpException)
            {
                log.Debug($"SNMP Exception querying '{what}' as string", snmpException);
            }
            catch(HamnetSnmpException hamnetSnmpException)
            {
                log.Debug("Hamnet SNMP Exception querying '{what}' as string", hamnetSnmpException);
            }

            return null;
        }

    }
}
