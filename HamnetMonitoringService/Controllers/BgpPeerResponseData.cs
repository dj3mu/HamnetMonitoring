using System;
using Newtonsoft.Json;
using RestService.Model;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// BGP peers data from database extended for response to outside.
    /// </summary>
    public class BgpPeerResponseData : IBgpPeerData
    {
        private BgpPeerData underlyingDatabaseData;

        /// <summary>
        /// Construct from a database peer data object.
        /// </summary>
        /// <param name="peerData">The database peer data object to construct from.</param>
        public BgpPeerResponseData(BgpPeerData peerData)
        {
            this.underlyingDatabaseData = peerData;
            
            if (TimeSpan.TryParse(peerData.Uptime, out TimeSpan parsedUptime))
            {
                this.UptimeSeconds = parsedUptime;
            }
        }

        /// <inheritdoc />
        public int Id => this.underlyingDatabaseData.Id;

        /// <inheritdoc />
        public string RemoteAddress => this.underlyingDatabaseData.RemoteAddress;

        /// <inheritdoc />
        public string LocalAddress => this.underlyingDatabaseData.LocalAddress;

        /// <inheritdoc />
        public string LocalCallsign => this.underlyingDatabaseData.LocalCallsign;

        /// <inheritdoc />
        public string PeeringName => this.underlyingDatabaseData.PeeringName;

        /// <inheritdoc />
        public string Uptime => this.underlyingDatabaseData.Uptime;

        /// <inheritdoc />
        [JsonConverter(typeof(TimeSpanAsTotalSecondsConverter))]
        public TimeSpan UptimeSeconds { get; } = TimeSpan.Zero;

        /// <inheritdoc />
        public long PrefixCount => this.underlyingDatabaseData.PrefixCount;

        /// <inheritdoc />
        public string PeeringState => this.underlyingDatabaseData.PeeringState;

        /// <inheritdoc />
        public ulong UnixTimeStamp => this.underlyingDatabaseData.UnixTimeStamp;

        /// <inheritdoc />
        public string TimeStampString => this.underlyingDatabaseData.TimeStampString;
    }
}