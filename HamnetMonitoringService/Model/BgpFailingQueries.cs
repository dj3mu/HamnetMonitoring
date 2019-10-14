using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RestService.Model
{
    /// <summary>
    /// Model container for information about failing RSSI queries.
    /// </summary>
    public class BgpFailingQuery
    {
        /// <summary>
        /// The host for which the BGP data query failed.
        /// </summary>
        [Key, Required, JsonProperty(PropertyName = "Host")]
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the time stamp at which the error occurred.
        /// </summary>
        [Required, JsonProperty(PropertyName = "TimeStamp")]
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the error info text.
        /// </summary>
        [Required, JsonProperty(PropertyName = "ErrorInfo")]
        public string ErrorInfo { get; set; }
    }
}
