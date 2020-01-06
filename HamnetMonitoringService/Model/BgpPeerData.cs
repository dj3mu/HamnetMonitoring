using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RestService.Model
{
    /// <summary>
    /// Container for a single BGP peer result.
    /// </summary>
    public class BgpPeerData
    {
        /// <summary>
        /// Gets the row ID (key).
        /// </summary>
        [Key, JsonIgnore, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the remote (i.e. the other side) of the BGP peering.
        /// </summary>
        [Required, JsonProperty]
        public string RemoteAddress { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the local (i.e. the queried side) of the BGP peering.
        /// </summary>
        [Required, JsonProperty]
        public string LocalAddress { get; set; }

        /// <summary>
        /// Gets or sets the callsign of the local (i.e. the queried side) of the BGP peering.
        /// </summary>
        [Required, JsonProperty]
        public string LocalCallsign { get; set; }

        /// <summary>
        /// Gets or sets the name of the peering as assigned by Sysop.
        /// </summary>
        [JsonProperty]
        public string PeeringName { get; set; }

        /// <summary>
        /// Gets or sets the uptime of the BGP peering.
        /// </summary>
        [JsonProperty]
        public string Uptime { get; set; }

        /// <summary>
        /// Gets or sets the number of prefixes routed to the remote side.
        /// </summary>
        [JsonProperty]
        public long PrefixCount { get; set; }

        /// <summary>
        /// Gets or sets the state of the peering.
        /// </summary>
        [Required, JsonProperty]
        public string PeeringState { get; set; }

        /// <summary>
        /// Gets or sets the unix time stamp.
        /// </summary>
        [Required, JsonProperty(PropertyName = "unixtimestamp")]
        public ulong UnixTimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the time stamp as human-readable string.
        /// </summary>
        [Required, JsonProperty(PropertyName = "stamp")]
        public string TimeStampString { get; set; }
    }
}
