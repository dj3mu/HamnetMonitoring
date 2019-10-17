using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the results of the performance counter.
    /// </summary>
    public interface IPerformanceCounter
    {
        /// <summary>
        /// Gets the overall accumulated results.
        /// </summary>
        IPerformanceSingleResultSet OverallResults { get; }

        /// <summary>
        /// Gets the mapping of result sets to request types (Get, Bulk, GetNext, ...)
        /// </summary>
        IReadOnlyDictionary<string, IPerformanceSingleResultSet> ResultsPerRequestType { get; }

        /// <summary>
        /// Gets the mapping of result sets to devices addresses.
        /// </summary>
        [JsonIgnore]
        IReadOnlyDictionary<IPAddress, IPerformanceSingleResultSet> ResultsPerDevice { get; }
    }
}
