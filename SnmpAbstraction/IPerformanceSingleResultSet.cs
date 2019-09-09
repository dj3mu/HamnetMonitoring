namespace SnmpAbstraction
{
    /// <summary>
    /// Sets of performance counter results.
    /// </summary>
    public interface IPerformanceSingleResultSet
    {
        /// <summary>
        /// Gets the total number of SNMP request PDUs.
        /// </summary>
        int TotalNumberOfRequestPdus { get; }

        /// <summary>
        /// Gets the total number of SNMP response PDUs.
        /// </summary>
        int TotalNumberOfResponsePdus { get; }

        /// <summary>
        /// Gets the total number of SNMP responses that contained an error code other than 0.
        /// </summary>
        int TotalNumberOfErrorResponses { get; }
    }
}