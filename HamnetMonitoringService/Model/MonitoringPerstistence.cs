using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RestService.Model
{
    /// <summary>
    /// Database table to preserve persistency data.
    /// </summary>
    public class MonitoringPerstistence
    {
        /// <summary>
        /// Gets the row ID (key).
        /// </summary>
        [Key, JsonIgnore, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Gets the start time of the last RSSI monitoring query.
        /// </summary>
        [JsonProperty(PropertyName = "LastQueryStart")]
        public DateTime LastRssiQueryStart { get; set; }

        /// <summary>
        /// Gets the end time of the last RSSI monitoring query.
        /// </summary>   
        [JsonProperty(PropertyName = "LastQueryEnd")]
        public DateTime LastRssiQueryEnd { get; set; }

        /// <summary>
        /// Gets the start time of the last BGP monitoring query.
        /// </summary>
        [JsonProperty(PropertyName = "LastBgpQueryStart")]
        public DateTime LastBgpQueryStart { get; set; }

        /// <summary>
        /// Gets the end time of the last BGP monitoring query.
        /// </summary>   
        [JsonProperty(PropertyName = "LastBgpQueryEnd")]
        public DateTime LastBgpQueryEnd { get; set; }

        /// <summary>
        /// Gets the start time of the last database maintenance run.
        /// </summary>   
        [JsonProperty(PropertyName = "LastMaintenanceStart")]
        public DateTime LastMaintenanceStart { get; set; }

        /// <summary>
        /// Gets the end time of the last database maintenance run.
        /// </summary>   
        [JsonProperty(PropertyName = "LastMaintenanceEnd")]
        public DateTime LastMaintenanceEnd { get; set; }
    }
}