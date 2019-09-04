using System.Collections.Generic;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the list and the details of the interfaces of a device.
    /// </summary>
    public interface IInterfaceDetails : IHamnetSnmpQuerierResult, ILazyEvaluated
    {
        /// <summary>
        /// Gets the list of details.
        /// </summary>
        IReadOnlyList<IInterfaceDetail> Details { get; }
    }
}
