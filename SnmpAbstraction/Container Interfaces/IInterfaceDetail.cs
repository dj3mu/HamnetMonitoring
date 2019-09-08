namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the details of a single interface of a device.
    /// </summary>
    public interface IInterfaceDetail : IHamnetSnmpQuerierResult, ILazyEvaluated
    {
        /// <summary>
        /// Gets the numeric ID of the interface.
        /// </summary>
        int InterfaceId { get; }

        /// <summary>
        /// Gets the type of the interface as integer (e.g. 71 = ieee80211).
        /// </summary>
        IanaInterfaceType InterfaceType { get; }

        /// <summary>
        /// Gets the MAC address of the interface as hex string (e.g. &quot;0:c:42:9f:88:f9&quot;).
        /// </summary>
        string MacAddressString { get; }

        /// <summary>
        /// Gets the name of the interface as reported by the device.
        /// </summary>
        string InterfaceName { get; }
    }
}
