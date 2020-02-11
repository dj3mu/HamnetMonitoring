using Newtonsoft.Json;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Container for the site data set as received from HamnetDB.
    /// </summary>
    [JsonObject]
    internal class JsonSiteDataSet : JsonDataSetBase, IHamnetDbSite
    {
        /// <inheritdoc />
        [JsonProperty("latitude", Required = Required.Always)]
        public double Latitude { get; set; } = double.NaN;

        /// <inheritdoc />
        [JsonProperty("longitude", Required = Required.Always)]
        public double Longitude { get; set; } = double.NaN;

        /// <inheritdoc />
        [JsonProperty("ground_asl", Required = Required.Always)]
        public double GroundAboveSeaLevel { get; set; } = double.NaN;

        /// <inheritdoc />
        [JsonProperty("elevation", Required = Required.Always)]
        public double Elevation { get; set; } = double.NaN;

        /// <inheritdoc />
        [JsonProperty("callsign", Required = Required.Always)]
        public string Callsign { get; set; }

        /// <inheritdoc />
        [JsonProperty("name", Required = Required.Default)]
        public string Name { get; set; }

        /// <inheritdoc />
        [JsonProperty("comment", Required = Required.Default)]
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this site has a cover (? whatever that is ?).
        /// </summary>
        [JsonProperty("hasCover", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool HasCover { get; set; }

        /// <inheritdoc />
        [JsonProperty("inactive", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool Inactive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this site is NewCover (? whatever that is ?).
        /// </summary>
        [JsonProperty("newCover", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool NewCover { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public double Altitude => (double.IsNaN(this.GroundAboveSeaLevel) || double.IsNaN(this.Elevation)) 
            ? double.NaN
            : this.GroundAboveSeaLevel + this.Elevation;
    }
}
