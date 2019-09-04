using System;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the settgins of an IHamnetSnmpQuerier.
    /// </summary>
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

        /// <summary>
        /// Gets a value telling the SNMP Agent how many GETNEXT like variables to
        /// retrieve (single Vb returned per request) before MaxRepetitions value takes affect.
        /// If you wish to retrieve as many values as you can in a single request, set this
        /// value to 0.
        /// </summary>
        int Ver2cMaximumValuesPerRequest { get; }

        /// <summary>
        /// Gets a value telling the SNMP Agent how many VBs to include in a single request.
        /// Only valid on GETBULK requests.
        /// </summary>
        int Ver2cMaximumRequests { get; }
    }
}
