using System;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Container for options to SNMP queriers.
    /// </summary>
    internal class QuerierOptions : IQuerierOptions
    {
        /// <summary>
        /// Construct from all the parameters.
        /// </summary>
        /// <param name="port">The port number to use for the queries.</param>
        /// <param name="protocolVersion">The SNMP protocol version to use for the queries.</param>
        /// <param name="community">The community string to use for the queries.</param>
        /// <param name="timeout">The query timeout.</param>
        /// <param name="retries">The SNMP peer maximum retires setting. Value of 0 will result in a single request with no retries.</param>
        public QuerierOptions(int port, SnmpVersion protocolVersion, OctetString community, TimeSpan timeout, int retries)
        {
            this.Port = port;
            this.ProtocolVersion = protocolVersion;
            this.Community = community;
            this.Timeout = timeout;
            this.Retries = retries;
        }

        /// <summary>
        /// Construct with default values.
        /// </summary>
        /// <remarks>Private! Use static property <see cref="Default" /> to get the default object.</remarks>
        private QuerierOptions()
        {
        }

        /// <summary>
        /// Gets all default options.
        /// </summary>
        public static QuerierOptions Default { get; } = new QuerierOptions();

        /// <inheritdoc />
        public int Port { get; } = 161;

        /// <inheritdoc />
        public SnmpVersion ProtocolVersion { get; } = SnmpVersion.Ver1;

        /// <inheritdoc />
        public OctetString Community { get; } = new OctetString("public");

        /// <inheritdoc />
        public TimeSpan Timeout { get; } = TimeSpan.FromSeconds(2);

        /// <inheritdoc />
        public int Retries { get; } = 1;

        /// <summary>
        /// Creates a new object that is a copy of this object with modified port number.
        /// </summary>
        /// <param name="port">The new port number to use for the queries.</param>
        /// <returns>A new object that is a copy of this object with modified port number.</returns>
        public QuerierOptions WithPort(int port)
        {
            return new QuerierOptions(port, this.ProtocolVersion, this.Community, this.Timeout, this.Retries);
        }

        /// <summary>
        /// Creates a new object that is a copy of this object with modified SNMP protocol version.
        /// </summary>
        /// <param name="protocolVersion">The new SNMP protocol version to use for the queries.</param>
        /// <returns>A new object that is a copy of this object with modified SNMP protocol version.</returns>
        public QuerierOptions WithProtocolVersion(SnmpVersion protocolVersion)
        {
            return new QuerierOptions(this.Port, protocolVersion, this.Community, this.Timeout, this.Retries);
        }

        /// <summary>
        /// Creates a new object that is a copy of this object with modified community string.
        /// </summary>
        /// <param name="community">The new community string to use for the queries.</param>
        /// <returns>A new object that is a copy of this object with modified community string.</returns>
        public QuerierOptions WithCommunity(OctetString community)
        {
            return new QuerierOptions(this.Port, this.ProtocolVersion, community, this.Timeout, this.Retries);
        }

        /// <summary>
        /// Creates a new object that is a copy of this object with modified timeout.
        /// </summary>
        /// <param name="timeout">The new query timeout.</param>
        /// <returns>A new object that is a copy of this object with modified timeout.</returns>
        public QuerierOptions WithTimeout(TimeSpan timeout)
        {
            return new QuerierOptions(this.Port, this.ProtocolVersion, this.Community, timeout, this.Retries);
        }

        /// <summary>
        /// Creates a new object that is a copy of this object with modified retries count.
        /// </summary>
        /// <param name="retries">The new SNMP peer maximum retires setting. Value of 0 will result in a single request with no retries.</param>
        /// <returns>A new object that is a copy of this object with modified retries count.</returns>
        public QuerierOptions WithRetries(int retries)
        {
            return new QuerierOptions(this.Port, this.ProtocolVersion, this.Community, this.Timeout, retries);
        }
    }
}