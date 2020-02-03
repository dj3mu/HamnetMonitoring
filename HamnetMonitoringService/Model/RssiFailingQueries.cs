using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using RestService.DataFetchingService;

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
        [Key, Required, JsonProperty(PropertyName = "Subnet", Order = 1)]
        public string Subnet { get; set; }

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
        /// Gets or sets the list of affected hosts
        /// </summary>
        [JsonProperty(PropertyName = "AffectedHosts", Order = 4)]
        public IReadOnlyCollection<string> AffectedHosts { get; set; }

        /// <summary>
        /// Gets or sets the information about how much penalty this errors already receives.
        /// </summary>
        [JsonProperty(PropertyName = "PenaltyInfo", Order = 5)]
        public ISingleFailureInfo PenaltyInfo { get; set; }
    }
}
