using System;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Container for options to SNMP queriers.
    /// </summary>
    public class QuerierOptions : IQuerierOptions
    {
        /// <summary>
        /// Construct from all the parameters.
        /// </summary>
        /// <param name="port">The port number to use for the queries.</param>
        /// <param name="protocolVersion">The SNMP protocol version to use for the queries.</param>
        /// <param name="community">The community string to use for the queries.</param>
        /// <param name="timeout">The query timeout.</param>
        /// <param name="retries">The SNMP peer maximum retires setting. Value of 0 will result in a single request with no retries.</param>
        /// <param name="ver2cMaximumValuesPerRequest">A value telling the SNMP Agent how many GETNEXT like variables to retrieve</param>
        /// <param name="ver2cMaximumRequests">A value telling the SNMP Agent how many VBs to include in a single request.<br/>
        /// Only valid on GETBULK requests.</param>
        /// <param name="enableCaching">Value indicating whether to use caching of non-volatile data.</param>
        /// <param name="loginUser">The user name for logins (if needed).</param>
        /// <param name="loginPassword">The password for logins (if needed).</param>
        /// <param name="allowedApis">The allowed APIs. By default, only SNMP is allowed.</param>
        /// <param name="querierClassHint">If not null or empty, a full qualified class name of the handler class to use (if possible).</param>
        public QuerierOptions(
            int port,
            SnmpVersion protocolVersion,
            OctetString community,
            TimeSpan timeout,
            int retries,
            int ver2cMaximumValuesPerRequest,
            int ver2cMaximumRequests,
            bool enableCaching,
            string loginUser,
            string loginPassword,
            QueryApis allowedApis,
            string querierClassHint = null)
        {
            this.Port = port;
            this.ProtocolVersion = protocolVersion;
            this.Community = community;
            this.Timeout = timeout;
            this.Retries = retries;
            this.Ver2cMaximumRequests = ver2cMaximumRequests;
            this.Ver2cMaximumValuesPerRequest = ver2cMaximumValuesPerRequest;
            this.EnableCaching = enableCaching;
            this.LoginUser = loginUser;
            this.LoginPassword = loginPassword;
            this.AllowedApis = allowedApis;
            this.QuerierClassHint = querierClassHint;
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
        public SnmpVersion ProtocolVersion { get; } = SnmpVersion.Ver2;

        /// <inheritdoc />
        public OctetString Community { get; } = new OctetString("public");

        /// <inheritdoc />
        public TimeSpan Timeout { get; } = TimeSpan.FromSeconds(2);

        /// <inheritdoc />
        public int Retries { get; } = 3;

        /// <inheritdoc />
        public int Ver2cMaximumValuesPerRequest { get; } = 0;

        /// <inheritdoc />
        public int Ver2cMaximumRequests { get; } = 5;

        /// <inheritdoc />
        public bool EnableCaching { get; } = true;

        /// <inheritdoc />
        public string LoginUser { get; } = "monitoring";

        /// <inheritdoc />
        public string LoginPassword { get; } = string.Empty;

        /// <inheritdoc />
        public QueryApis AllowedApis { get; } = QueryApis.Snmp;

        /// <summary>
        /// Gets a full qualified class name of the handler class to use (if possible). Use null or empty to not provide any handler class hint.
        /// </summary>
        public string QuerierClassHint { get; }

        /// <summary>
        /// Creates a new object that is a copy of this object with modified Ver2cMaximumRequests number.
        /// </summary>
        /// <param name="ver2cMaximumRequests">The new ver2cMaximumRequests number to use for the queries.</param>
        /// <returns>A new object that is a copy of this object with modified ver2cMaximumRequests number.</returns>
        public QuerierOptions WithVer2cMaximumRequests(int ver2cMaximumRequests)
        {
            return new QuerierOptions(this.Port, this.ProtocolVersion, this.Community, this.Timeout, this.Retries, this.Ver2cMaximumValuesPerRequest, ver2cMaximumRequests, this.EnableCaching, this.LoginUser, this.LoginPassword, this.AllowedApis);
        }

        /// <summary>
        /// Creates a new object that is a copy of this object with modified ver2cMaximumValuesPerRequest number.
        /// </summary>
        /// <param name="ver2cMaximumValuesPerRequest">The new ver2cMaximumValuesPerRequest number to use for the queries.</param>
        /// <returns>A new object that is a copy of this object with modified ver2cMaximumValuesPerRequest number.</returns>
        public QuerierOptions WithVer2cMaximumValuesPerRequest(int ver2cMaximumValuesPerRequest)
        {
            return new QuerierOptions(this.Port, this.ProtocolVersion, this.Community, this.Timeout, this.Retries, ver2cMaximumValuesPerRequest, this.Ver2cMaximumRequests, this.EnableCaching, this.LoginUser, this.LoginPassword, this.AllowedApis);
        }

        /// <summary>
        /// Creates a new object that is a copy of this object with modified port number.
        /// </summary>
        /// <param name="port">The new port number to use for the queries.</param>
        /// <returns>A new object that is a copy of this object with modified port number.</returns>
        public QuerierOptions WithPort(int port)
        {
            return new QuerierOptions(port, this.ProtocolVersion, this.Community, this.Timeout, this.Retries, this.Ver2cMaximumValuesPerRequest, this.Ver2cMaximumRequests, this.EnableCaching, this.LoginUser, this.LoginPassword, this.AllowedApis);
        }

        /// <summary>
        /// Creates a new object that is a copy of this object with modified SNMP protocol version.
        /// </summary>
        /// <param name="protocolVersion">The new SNMP protocol version to use for the queries.</param>
        /// <returns>A new object that is a copy of this object with modified SNMP protocol version.</returns>
        public QuerierOptions WithProtocolVersion(SnmpVersion protocolVersion)
        {
            return new QuerierOptions(this.Port, protocolVersion, this.Community, this.Timeout, this.Retries, this.Ver2cMaximumValuesPerRequest, this.Ver2cMaximumRequests, this.EnableCaching, this.LoginUser, this.LoginPassword, this.AllowedApis);
        }

        /// <summary>
        /// Creates a new object that is a copy of this object with modified community string.
        /// </summary>
        /// <param name="community">The new community string to use for the queries.</param>
        /// <returns>A new object that is a copy of this object with modified community string.</returns>
        public QuerierOptions WithCommunity(OctetString community)
        {
            return new QuerierOptions(this.Port, this.ProtocolVersion, community, this.Timeout, this.Retries, this.Ver2cMaximumValuesPerRequest, this.Ver2cMaximumRequests, this.EnableCaching, this.LoginUser, this.LoginPassword, this.AllowedApis);
        }

        /// <summary>
        /// Creates a new object that is a copy of this object with modified timeout.
        /// </summary>
        /// <param name="timeout">The new query timeout.</param>
        /// <returns>A new object that is a copy of this object with modified timeout.</returns>
        public QuerierOptions WithTimeout(TimeSpan timeout)
        {
            return new QuerierOptions(this.Port, this.ProtocolVersion, this.Community, timeout, this.Retries, this.Ver2cMaximumValuesPerRequest, this.Ver2cMaximumRequests, this.EnableCaching, this.LoginUser, this.LoginPassword, this.AllowedApis);
        }

        /// <summary>
        /// Creates a new object that is a copy of this object with modified retries count.
        /// </summary>
        /// <param name="retries">The new SNMP peer maximum retires setting. Value of 0 will result in a single request with no retries.</param>
        /// <returns>A new object that is a copy of this object with modified retries count.</returns>
        public QuerierOptions WithRetries(int retries)
        {
            return new QuerierOptions(this.Port, this.ProtocolVersion, this.Community, this.Timeout, retries, this.Ver2cMaximumValuesPerRequest, this.Ver2cMaximumRequests, this.EnableCaching, this.LoginUser, this.LoginPassword, this.AllowedApis);
        }

        /// <summary>
        /// Creates a new object that is a copy of this object with modified caching setting.
        /// </summary>
        /// <param name="enableCaching">Value indicating whether to enable caching of non-volatile data.</param>
        /// <returns>A new object that is a copy of this object with modified caching setting.</returns>
        public QuerierOptions WithCaching(bool enableCaching)
        {
            return new QuerierOptions(this.Port, this.ProtocolVersion, this.Community, this.Timeout, this.Retries, this.Ver2cMaximumValuesPerRequest, this.Ver2cMaximumRequests, enableCaching, this.LoginUser, this.LoginPassword, this.AllowedApis);
        }

        /// <summary>
        /// Creates a new object that is a copy of this object with modified login user name.
        /// </summary>
        /// <param name="user">The user to use for logins to APIs (if needed)</param>
        /// <returns>A new object that is a copy of this object with modified login user name setting.</returns>
        public QuerierOptions WithUser(string user)
        {
            return new QuerierOptions(this.Port, this.ProtocolVersion, this.Community, this.Timeout, this.Retries, this.Ver2cMaximumValuesPerRequest, this.Ver2cMaximumRequests, this.EnableCaching, user, this.LoginPassword, this.AllowedApis);
        }

        /// <summary>
        /// Creates a new object that is a copy of this object with modified login password.
        /// </summary>
        /// <param name="password">The password to use for logins to APIs (if needed)</param>
        /// <returns>A new object that is a copy of this object with modified login password setting.</returns>
        /// <remarks>We're not taking any measures to protect this password.<br/>
        /// In Hamnet we will anyway have to transfer it in plain text due to regulatory rules (Amateur Radio has to be public).<br/>
        /// So let's save the additonal effort of fiddling around with SecretString etc.
        /// </remarks>
        public QuerierOptions WithPassword(string password)
        {
            return new QuerierOptions(this.Port, this.ProtocolVersion, this.Community, this.Timeout, this.Retries, this.Ver2cMaximumValuesPerRequest, this.Ver2cMaximumRequests, this.EnableCaching, this.LoginUser, password, this.AllowedApis);
        }

        /// <summary>
        /// Creates a new object that is a copy of this object with modified allowed APIs setting.
        /// </summary>
        /// <param name="allowedApis">The allowed APIs. By default, only SNMP is allowed.</param>
        /// <returns>A new object that is a copy of this object with modified allowed APIs setting.</returns>
        public QuerierOptions WithAllowedApis(QueryApis allowedApis)
        {
            return new QuerierOptions(this.Port, this.ProtocolVersion, this.Community, this.Timeout, this.Retries, this.Ver2cMaximumValuesPerRequest, this.Ver2cMaximumRequests, this.EnableCaching, this.LoginUser, this.LoginPassword, allowedApis);
        }
    }
}