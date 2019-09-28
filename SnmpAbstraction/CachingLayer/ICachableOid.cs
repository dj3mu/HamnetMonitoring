using System.Collections.Generic;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Enumerationg of supported value meanings.
    /// </summary>
    public enum CachableValueMeanings
    {
        /// <summary>
        /// The remote peers RSSI (RX signal strength) as provided by <see cref="IWirelessPeerInfo" />.
        /// </summary>
        WirelessRxSignalStrength = 1,

        /// <summary>
        /// The remote wireless TX signal strength as provided by <see cref="IWirelessPeerInfo" />.
        /// </summary>
        WirelessTxSignalStrength = 2,

        /// <summary>
        /// The uptime of the wireless link as provided by <see cref="IWirelessPeerInfo" />.
        /// </summary>
        WirelessLinkUptime = 3,
    }

    /// <summary>
    /// Interface to the combination of a host name and OID to query.
    /// </summary>
    public interface ICachableOid
    {
        /// <summary>
        /// The address of the host that a query can be cached for.
        /// </summary>
        IpAddress Address { get; }

        /// <summary>
        /// Gets the meaning of the value that is queried with the <see cref="Oid" />.
        /// </summary>
        CachableValueMeanings Meaning { get; }

        /// <summary>
        /// Gets a value indicating whether the value of the given meaning is retrieved with a single OID.
        /// </summary>
        /// <value>
        /// <c>true</c> if the value of given meaning is retrieved with a single OID.
        /// <c>false</c> if the output of multiple OIDs must be combined to obtain the value.
        /// The way how the values are to be combined in case <c>false</c> is returned, is not availble with this interface.
        /// It must be known to the user of the interface.
        /// </value>
        bool IsSingleOid { get; }

        /// <summary>
        /// Gets the SNMP OID that shall be queried to obtain the value of the given meaning.
        /// </summary>
        /// <remarks>
        /// This property will throw if there is than one OID needs to be queried
        /// (i.e. if <see cref="IsSingleOid" /> returns <c>false</c>).
        /// </remarks>
        Oid Oid { get; }

        /// <summary>
        /// Gets the SNMP OIDs that shall be queried to obtain the values that are to be combined to get the final value of the given meaning.
        /// </summary>
        IEnumerable<Oid> Oids { get; }
    }
}
