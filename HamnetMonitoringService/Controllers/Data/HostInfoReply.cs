using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SnmpAbstraction;
using SnmpSharpNet;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Container for a list link details replies.
    /// </summary>
    internal class HostInfoReply : IHostInfoReply
    {
        private readonly IDeviceSystemData systemData;

        private readonly IpAddress address;

        /// <summary>
        /// Construct from system data.
        /// </summary>
        /// <param name="address">The IP address that this host info reply is for.</param>
        /// <param name="systemData">The system data.</param>
        /// <param name="defaultApi">The API that is, by default, used for talking to this device.</param>
        /// <param name="lastDataUpdate">The date and time when the data has last been updated.</param>
        public HostInfoReply(IpAddress address, IDeviceSystemData systemData, QueryApis defaultApi, DateTime? lastDataUpdate)
        {
            this.address = address ?? throw new ArgumentNullException(nameof(address), "The IP address to construct a HostInfoReply for is null");
            this.systemData = systemData ?? throw new ArgumentNullException(nameof(systemData), "The system data to construct a HostInfoReply from is null");

            this.systemData.ForceEvaluateAll();

            this.DefaultApi = defaultApi.ToString();
            this.LastDataUpdate = lastDataUpdate;

            this.SupportedFeatures = this.systemData.SupportedFeatures
                .ToString()
                .Split(',' , StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim());
        }

        /// <inheritdoc />
        public string Description => this.systemData.Description;

        /// <inheritdoc />
        public string Contact => this.systemData.Contact;

        /// <inheritdoc />
        public string Location => this.systemData.Location;

        /// <inheritdoc />
        public string Name => this.systemData.Name;

        /// <inheritdoc />
        public TimeSpan? Uptime => this.systemData.Uptime;

        /// <inheritdoc />
        [JsonConverter(typeof(TimeSpanAsTotalSecondsConverter))]
        public TimeSpan? UptimeSeconds => this.Uptime;

        /// <inheritdoc />
        public string Model => this.systemData.Model;

        /// <inheritdoc />
        public string Version => this.systemData.Version?.ToString();

        /// <inheritdoc />
        public string MaximumSnmpVersion => this.systemData.MaximumSnmpVersion.ToString();

        /// <inheritdoc />
        public string Address =>this.address?.ToString();

        /// <inheritdoc />
        public IEnumerable<string> ErrorDetails{ get; } = Enumerable.Empty<string>();

        /// <inheritdoc />
        public IEnumerable<string> SupportedFeatures { get; }

        /// <inheritdoc />
        public string DefaultApi { get; }

        /// <inheritdoc />
        public DateTime? LastDataUpdate { get; }
    }
}
