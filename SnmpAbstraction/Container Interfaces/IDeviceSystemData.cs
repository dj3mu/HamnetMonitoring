using System;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Container to the device's system data (i.e. the .1.3.6.1.2.1.1 subtree)
    /// </summary>
    public interface IDeviceSystemData : IHamnetSnmpQuerierResult, ILazyEvaluated
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
        /// Gets the device's maximum supported SNMP version.
        /// </summary>
        SnmpVersion MaximumSnmpVersion { get; }
    }
}
