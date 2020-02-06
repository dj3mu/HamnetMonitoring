using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using RestService.DataFetchingService;

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
        [Key, Required, JsonProperty(PropertyName = "Host", Order = 1)]
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the time stamp at which the error occurred.
        /// </summary>
        [Required, JsonProperty(PropertyName = "TimeStamp", Order = 2)]
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Gets or sets the error info text.
        /// </summary>
        [Required, JsonProperty(PropertyName = "ErrorInfo", Order = 3)]
        public string ErrorInfo { get; set; }

        /// <summary>
        /// Gets or sets the information about how much penalty this errors already receives.
        /// </summary>
        [JsonProperty(PropertyName = "PenaltyInfo", Order = 4)]
        public ISingleFailureInfo PenaltyInfo { get; set; }
    }
}
