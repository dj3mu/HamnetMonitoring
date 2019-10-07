using System.Collections.Generic;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the details of the links between devices.
    /// </summary>
    public interface ILinkDetails : IHamnetSnmpQuerierResult, ILazyEvaluated
    {
        /// <summary>
        /// Gets the list of link details.
        /// </summary>
        IReadOnlyList<ILinkDetail> Details { get; }
    }
}
