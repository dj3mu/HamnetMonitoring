using System.Collections.Generic;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the list and the details of the interfaces of a device.
    /// </summary>
    public interface IInterfaceDetails : IHamnetSnmpQuerierResult, ILazyEvaluated, IEnumerable<IInterfaceDetail>
    {
        /// <summary>
        /// Gets the list of interface details.
        /// </summary>
        IReadOnlyList<IInterfaceDetail> Details { get; }
    }
}
