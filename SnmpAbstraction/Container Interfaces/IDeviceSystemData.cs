using System;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Container to the device's system data (i.e. the .1.3.6.1.2.1.1 subtree)
    /// </summary>
    public interface IDeviceSystemData
    {
        /// <summary>
        /// Gets the system's description or an empty string if not provided.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the system's enterprise root OID or null if not provided.
        /// </summary>
        Oid EnterpriseObjectId { get; }

        /// <summary>
        /// Gets the system's admin name.
        /// </summary>
        string Contact { get; }

        /// <summary>
        /// Gets the system's location.
        /// </summary>
        string Location { get; }

        /// <summary>
        /// Gets the system's name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the system's uptime (at the time of the query that filled the container).
        /// </summary>
        TimeSpan Uptime { get; }

        /// <summary>
        /// Gets the number of services that this system provides.
        /// </summary>
        int ServiceCount { get; }
    }
}