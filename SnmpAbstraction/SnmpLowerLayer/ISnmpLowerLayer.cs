using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Lower-layered interface to the SNMP functions.
    /// </summary>
    internal interface ISnmpLowerLayer
    {
        /// <summary>
        /// Gets the IP Address that this lower layer is talking to.
        /// </summary>
        /// <value></value>
        IpAddress Address { get; }

        /// <summary>
        /// Gets the options that are used by this lower layer.
        /// </summary>
        IQuerierOptions Options { get; }

        /// <summary>
        /// Gets a container that holds basic system data of the device.<br/>
        /// The container and even single properties of that container might be retrieved lazily (i.e. on first request).
        /// </summary>
        IDeviceSystemData SystemData { get; }

        /// <summary>
        /// Queries the given OIDs from the device and returns the received data as <see cref="VbCollection" />
        /// </summary>
        /// <param name="oids">The OIDs to query.</param>
        /// <returns>A <see cref="VbCollection" /> with the received data.</returns>
        VbCollection Query(params Oid[] oids);

        /// <summary>
        /// Performs an SNMP-Walk starting at the value specified by <paramref name="interfaceIdWalkRootOid" />
        /// and recusing with the depth specified by <paramref name="depth" />.
        /// </summary>
        /// <param name="interfaceIdWalkRootOid">The retrievable value to start walking at. The actual OID is resolved from the device database.</param>
        /// <param name="depth">The recursion depth. A value of 0 means not to recurse at all and just return the direct children.</param>
        /// <returns>A <see cref="VbCollection" /> with the received data.</returns>
        VbCollection DoWalk(Oid interfaceIdWalkRootOid, int depth);
    }
}
