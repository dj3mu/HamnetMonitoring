using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Serializable container for a single wireless peer info.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut, ItemRequired = Required.Always)]
    internal class SerializableWirelessPeerInfo : IWirelessPeerInfo
    {
        /// <summary>
        /// Default construct
        /// </summary>
        public SerializableWirelessPeerInfo()
        {
        }

        /// <summary>
        /// Copy-construct
        /// </summary>
        /// <param name="peerInfo">The peer info to copy.</param>
        public SerializableWirelessPeerInfo(IWirelessPeerInfo peerInfo)
        {
            if (peerInfo is null)
            {
                throw new ArgumentNullException(nameof(peerInfo), "The IWirelessPeerInfo to make serializable is null");
            }

            // as we'll anyway need all values, we trigger immediate aquisition hoping for better performance
            peerInfo.ForceEvaluateAll();

            this.RemoteMacString = peerInfo.RemoteMacString;
            this.InterfaceId = peerInfo.InterfaceId;
            this.IsAccessPoint = peerInfo.IsAccessPoint;
            this.DeviceAddress = peerInfo.DeviceAddress;
            this.DeviceModel = peerInfo.DeviceModel;
            this.Oids = peerInfo.Oids;
            this.NumberOfClients = peerInfo.NumberOfClients;

            // we're intentionally not setting signal levels, uptime, OIDs and query duration
            // those would change continuously and require re-querying or wouldn't make sense at all
        }

        /// <inheritdoc />
        public string RemoteMacString { get; set; }

        /// <inheritdoc />
        [JsonProperty("MacAddressString", Required = Required.AllowNull)]
        public int? InterfaceId { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public double TxSignalStrength { get; set; } = double.NaN;

        /// <inheritdoc />
        [JsonIgnore]
        public double RxSignalStrength { get; set; } = double.NaN;

        /// <inheritdoc />
        [JsonIgnore]
        public TimeSpan LinkUptime { get; set; } = TimeSpan.Zero;

        /// <inheritdoc />
        [JsonIgnore]
        public double? Ccq { get; set; } = null;

        /// <inheritdoc />
        [JsonProperty(Required = Required.AllowNull)]
        public bool? IsAccessPoint { get; set; }

        /// <inheritdoc />
        public IpAddress DeviceAddress { get; set; }

        /// <inheritdoc />
        public string DeviceModel { get; set; }

        private TimeSpan queryDuration = TimeSpan.Zero;

        /// <inheritdoc />
        public TimeSpan QueryDuration => queryDuration;

        /// <inheritdoc />
        [JsonProperty("NumberOfClients", Required = Required.Default)]
        public int? NumberOfClients { get; set; }

        /// <inheritdoc />
        public void SetQueryDuration(TimeSpan value)
        {
            queryDuration = value;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<CachableValueMeanings, ICachableOid> Oids { get; set; }

        /// <inheritdoc />
        public void ForceEvaluateAll()
        {
            // NOP here - we're not lazy
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder returnBuilder = new StringBuilder(128);

            returnBuilder.Append("Peer ").Append(this.RemoteMacString).AppendLine(":");
            returnBuilder.Append("  - Mode           : ").AppendLine(this.IsAccessPoint.HasValue ? (this.IsAccessPoint.Value ? "AP" : "Client") : "not available");
            returnBuilder.Append("  - Num clients    : ").AppendLine(this.NumberOfClients.HasValue ? this.NumberOfClients.Value.ToString() : "not available");
            returnBuilder.Append("  - On interface ID: ").AppendLine(this.InterfaceId.HasValue ? this.InterfaceId.Value.ToString() : "not available");
            returnBuilder.Append("  - Link Uptime    : not available");
            returnBuilder.Append("  - RX signal [dBm]: ").AppendLine(this.RxSignalStrength.ToString("0.0 dBm"));
            returnBuilder.Append("  - TX signal [dBm]: ").Append(this.TxSignalStrength.ToString("0.0 dBm"));
            returnBuilder.Append("  - CCQ            : ").Append(this.Ccq?.ToString("0") ?? "not available");

            return returnBuilder.ToString();
        }
    }
}