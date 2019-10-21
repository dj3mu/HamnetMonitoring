using System.Collections.Generic;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the result of a traceroute operation.
    /// </summary>
    public interface ITracerouteResult : IHamnetSnmpQuerierResult
    {
        /// <summary>
        /// Gets the address from which the traceroute has been initiated.
        /// </summary>
        IpAddress FromAddress { get; }

        /// <summary>
        /// Gets the address of the destination to which the traceroute has been performed.
        /// </summary>
        IpAddress ToAddress { get; }

        /// <summary>
        /// Gets the single hops of the route.
        /// </summary>
        IEnumerable<ITracerouteHop> Hops { get; }

        /// <summary>
        /// Gets the count of hops to the destination.
        /// </summary>
        int HopCount { get; }
    }
}