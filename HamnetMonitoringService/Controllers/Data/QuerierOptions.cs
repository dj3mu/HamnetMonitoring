using System;
using SnmpAbstraction;
using SnmpSharpNet;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Container for options to SNMP queriers that can be filled with the FromQuery attribute of a ASP.NET controller.
    /// </summary>
    public class FromUrlQueryQuerierOptions : IQuerierOptions
    {
        /// <summary>
        /// Construct with default values.
        /// </summary>
        public FromUrlQueryQuerierOptions()
        {
        }

        /// <inheritdoc />
        public int Port { get; set; } = 161;

        /// <inheritdoc />
        public SnmpVersion ProtocolVersion { get; set; } = SnmpVersion.Ver1;

        /// <inheritdoc />
        public OctetString Community { get; set; } = new OctetString("public");

        /// <inheritdoc />
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <inheritdoc />
        public int Retries { get; set; } = 1;

        /// <inheritdoc />
        public int Ver2cMaximumValuesPerRequest { get; set; } = 0;

        /// <inheritdoc />
        public int Ver2cMaximumRequests { get; set; } = 5;

        /// <inheritdoc />
        public bool EnableCaching { get; set; } = true;

        /// <inheritdoc />
        public string LoginUser { get; set; } = "monitoring";

        /// <inheritdoc />
        public string LoginPassword { get; set; } = null;

        /// <inheritdoc />
        public QueryApis AllowedApis { get; set; } = QueryApis.Snmp | QueryApis.VendorSpecific;

        /// <inheritdoc />
        public string QuerierClassHint { get; set; } = null;
    }
}