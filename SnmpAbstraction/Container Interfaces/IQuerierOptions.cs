using System;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// An enumeration of supported query APIs.
    /// </summary>
    [Flags]
    public enum QueryApis
    {
        /// <summary>
        /// Allow queries using SNMP.
        /// </summary>
        Snmp = 0x1,

        /// <summary>
        /// Allow queries using some vendor-specific API.
        /// </summary>
        /// <remarks>
        /// With vendor-specific API only try-and-error can be used for detecting the vendor. This may cause
        /// log entries for invalid login or similar on the devices.
        /// </remarks>
        VendorSpecific = 0x2
    }

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

        /// <summary>
        /// Gets a value indicating whether to use caching of data in a cache database to avoid frequent querying of non-volatile data.
        /// Volatile data (e.g. RSSI) will not be cached at all.
        /// </summary>
        bool EnableCaching { get; }

        /// <summary>
        /// Gets the user name to use when a login is required to use a specific API.
        /// </summary>
        string LoginUser { get; }

        /// <summary>
        /// Gets the password to use when a login is required to use a specific API.
        /// Can be null or empty of not required.
        /// </summary>
        /// <remarks>We're not taking any measures to protect this password.<br/>
        /// In Hamnet we will anyway have to transfer it in plain text due to regulatory rules (Amateur Radio has to be public).<br/>
        /// So let's save the additonal effort of fiddling around with SecretString etc.
        /// </remarks>
        string LoginPassword { get; }

        /// <summary>
        /// Gets the allowed APIs.<br/>
        /// If more then one is allowed, it is left up to the implementation which one to prioritize.<br/>
        /// Defaults to SNMP API only.
        /// </summary>
        /// <remarks>
        /// With vendor-specific API only try-and-error can be used for detecting the vendor. This may cause
        /// log entries for invalid login or similar on the devices.
        /// </remarks>
        QueryApis AllowedApis { get; }
    }
}
