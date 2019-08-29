using System;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    public interface IQuerierOptions
    {
        /// <summary>
        /// Gets the port number to use for the queries.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets the SNMP protocol version to use for the queries.
        /// </summary>
        SnmpVersion ProtocolVersion { get; }

        /// <summary>
        /// Gets the community string to use for the queries.
        /// </summary>
        OctetString Community { get; }

        /// <summary>
        /// Gets the query timeout.
        /// </summary>
        TimeSpan Timeout { get; }

        /// <summary>
        /// Gets the SNMP peer maximum retires setting. Value of 0 will result in a single request with no retries.
        /// </summary>
        int Retries { get; }
    }
}
