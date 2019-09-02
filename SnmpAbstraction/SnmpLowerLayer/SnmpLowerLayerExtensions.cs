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
            try
            {
                VbCollection result = lowerLayer.Query(oid);
                var retVal = result[oid]?.Value?.ToString();
                if (retVal == null)
                {
                    log.Warn($"Querying '{what}' from '{lowerLayer.Address}' as string returned null");
                }

                return retVal;
            }
            catch(SnmpException snmpException)
            {
                log.Debug($"SNMP Exception querying '{what}' from '{lowerLayer.Address}' as string", snmpException);
            }
            catch(HamnetSnmpException hamnetSnmpException)
            {
                log.Debug("Hamnet SNMP Exception querying '{what}' from '{lowerLayer.Address}' as string", hamnetSnmpException);
            }

            return null;
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
            try
            {
                VbCollection result = lowerLayer.Query(oid);
                var retVal = result[oid]?.Value as Oid;
                if (retVal == null)
                {
                    log.Warn($"Querying '{what}' from '{lowerLayer.Address}' as OID returned null");
                }

                return retVal;
            }
            catch(SnmpException snmpException)
            {
                log.Debug($"SNMP Exception querying '{what}' from '{lowerLayer.Address}' as OID", snmpException);
            }
            catch(HamnetSnmpException hamnetSnmpException)
            {
                log.Debug("Hamnet SNMP Exception querying '{what}' from '{lowerLayer.Address}' as OID", hamnetSnmpException);
            }

            return null;
        }

        /// <summary>
        /// Queries the given OIDs and returns the value as OID or null if the OID is not available.
        /// </summary>
        /// <param name="lowerLayer">The lower layer engine to extend.</param>
        /// <param name="oid">The OID to query.</param>
        /// <param name="what">Descriptive text what is being queried. Used for debug/error output</param>
        /// <returns>The OID's value as OID or null if the OID is not available.</returns>
        public static TimeTicks QueryAsTimeTicks(this ISnmpLowerLayer lowerLayer, Oid oid, string what)
        {
            try
            {
                VbCollection result = lowerLayer.Query(oid);
                var retVal = result[oid]?.Value as TimeTicks;
                if (retVal == null)
                {
                    log.Warn($"Querying '{what}' from '{lowerLayer.Address}' as TimeTicks returned null");
                }

                return retVal;
            }
            catch(SnmpException snmpException)
            {
                log.Debug($"SNMP Exception querying '{what}' from '{lowerLayer.Address}' as OID", snmpException);
            }
            catch(HamnetSnmpException hamnetSnmpException)
            {
                log.Debug("Hamnet SNMP Exception querying '{what}' from '{lowerLayer.Address}' as OID", hamnetSnmpException);
            }

            return null;
        }
    }
}
