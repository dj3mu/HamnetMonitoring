using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Lower-layered interface to the SNMP functions.
    /// </summary>
    public interface ISnmpLowerLayer
    {
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
    }
}
