using System.Collections.Generic;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Interface to the result of a traceroute operation.
    /// </summary>
    public interface ITracerouteWebResult : IStatusReply
    {
        /// <summary>
        /// Gets the address from which the traceroute has been initiated.
        /// </summary>
        string FromAddress { get; }

        /// <summary>
        /// Gets the address of the destination to which the traceroute has been performed.
        /// </summary>
        string ToAddress { get; }

        /// <summary>
        /// Gets the single hops of the route.
        /// </summary>
        IReadOnlyList<ITracerouteWebHop> Hops { get; }

        /// <summary>
        /// Gets the count of hops to the destination.
        /// </summary>
        int HopCount { get; }
    }
}