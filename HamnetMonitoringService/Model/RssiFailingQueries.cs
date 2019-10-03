using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace RestService.Model
{
    /// <summary>
    /// Model container for information about failing RSSI queries.
    /// </summary>
    public class RssiFailingQuery
    {
        /// <summary>
        /// The subnet for which the link RSSI query failed.
        /// </summary>
        [Key, Required, JsonProperty(PropertyName = "Subnet")]
        public string Subnet { get; set; }

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

        /// <summary>
        /// Gets or sets the list of affected hosts
        /// </summary>
        [JsonProperty(PropertyName = "AffectedHosts")]
        public IReadOnlyCollection<string> AffectedHosts { get; set; }
    }
}
