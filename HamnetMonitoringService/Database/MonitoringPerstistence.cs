using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RestService.Database
{
    /// <summary>
    /// Database table to preserve persistency data.
    /// </summary>
    public class MonitoringPerstistence
    {
        /// <summary>
        /// Gets the row ID (key).
        /// </summary>
        [Key, JsonIgnore]
        public int Id { get; set; }

        /// <summary>
        /// Gets the start time of the last monitoring query.
        /// </summary>
        [JsonProperty(PropertyName = "LastQueryStart")]
        public DateTime LastQueryStart { get; set; }

        /// <summary>
        /// Gets the end time of the last monitoring query.
        /// </summary>   
        [JsonProperty(PropertyName = "LastQueryEnd")]
        public DateTime LastQueryEnd { get; set; }

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