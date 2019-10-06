using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RestService.Model
{
    /// <summary>
    /// Model container for RSSI reports similar to <see href="http://opennms.hc.r1.ampr.org:3001/vw_rest_rssi" />.
    /// </summary>
    public class Rssi
    {
        /// <summary>
        /// Gets or sets the foreign ID (i.e. IP address).
        /// </summary>
        [Key, Required, JsonProperty(PropertyName = "foreignid")]
        public string ForeignId { get; set; }

        /// <summary>
        /// Gets or sets the metric ID.
        /// </summary>
        [Required, JsonProperty(PropertyName = "metricid")]
        public int MetricId { get; set; }

        /// <summary>
        /// Gets or sets the metric (name).
        /// </summary>
        [Required, JsonProperty(PropertyName = "metric")]
        public string Metric { get; set; }

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

        /// <summary>
        /// Gets or sets the RSSI value.
        /// </summary>
        [Required, JsonProperty(PropertyName = "value")]
        public string RssiValue { get; set; }

        /// <summary>
        /// Gets or sets the parent subnet (in CIDR notation x.x.x.x/y)
        /// </summary>
        [Required, JsonProperty(PropertyName = "parentSubnet")]
        public string ParentSubnet { get; set; }
    }
}
