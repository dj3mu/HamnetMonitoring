using System;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Enumeration of supported features of the device.
    /// </summary>
    [Flags]
    public enum DeviceSupportedFeatures
    {
        /// <summary>
        /// Device doesn't support any features or features have not been detected.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Support for querying RSSI values.
        /// </summary>
        Rssi = 0x1,

        /// <summary>
        /// Support for querying BGP peers.
        /// </summary>
        BgpPeers = 0x2,

        /// <summary>
        /// Support for executing a traceroute operation.
        /// </summary>
        Traceroute = 0x4
    }

    /// <summary>
    /// Container to the device's system data (i.e. the .1.3.6.1.2.1.1 subtree)
    /// </summary>
    public interface IDeviceSystemData : IHamnetSnmpQuerierResult, ILazyEvaluated, ICachableOids
    {
        /// <summary>
        /// Gets the system's description or null if not provided.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the system's enterprise root OID or null if not provided.
        /// </summary>
        Oid EnterpriseObjectId { get; }

        /// <summary>
        /// Gets the system's admin name or null if not available.
        /// </summary>
        string Contact { get; }

        /// <summary>
        /// Gets the system's location or null if not available.
        /// </summary>
        string Location { get; }

        /// <summary>
        /// Gets the system's name or null if not available.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the system's uptime (at the time of the query that filled the container).
        /// </summary>
        TimeSpan? Uptime { get; }

        /// <summary>
        /// Gets the device model name.
        /// </summary>
        /// <value></value>
        string Model { get; }

        /// <summary>
        /// Gets the device's software version.
        /// </summary>
        SemanticVersion Version { get; }

        /// <summary>
        /// Gets the device's minimum supported SNMP version.
        /// </summary>
        SnmpVersion MinimumSnmpVersion { get; }

        /// <summary>
        /// Gets the device's maximum supported SNMP version.
        /// </summary>
        SnmpVersion MaximumSnmpVersion { get; }

        /// <summary>
        /// Gets the features supported by the device.
        /// </summary>
        DeviceSupportedFeatures SupportedFeatures { get; }
    }
}
