using System.Collections.Generic;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the list and the details of the interfaces of a device.
    /// </summary>
    public interface IWirelessPeerInfos : IHamnetSnmpQuerierResult, ILazyEvaluated, IEnumerable<IWirelessPeerInfo>
    {
        /// <summary>
        /// Gets the list of wireless peer details.
        /// </summary>
        IReadOnlyList<IWirelessPeerInfo> Details { get; }
    }
}