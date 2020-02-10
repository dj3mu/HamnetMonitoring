using Newtonsoft.Json;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Container for the subnet data set as received from HamnetDB.
    /// </summary>
    [JsonObject]
    internal class JsonSubnetDataSet : JsonDataSetBase
    {
        /// <summary>
        /// Gets or sets the IP address of the subnet.
        /// </summary>
        [JsonProperty("ip", Required = Required.Always)]
        public string Subnet { get; set; }

        /// <summary>
        /// Gets or sets the type of this subnet.
        /// </summary>
        [JsonProperty("typ", Required = Required.Always)]
        public string Typ { get; set; }

        /// <summary>
        /// Gets or sets a comma-separated list of readio parameters of this this host.
        /// </summary>
        [JsonProperty("radioparam", Required = Required.Default)]
        public string RadioParameters { get; set; }

        /// <summary>
        /// Gets or sets the BGP AS number of this subnet.
        /// </summary>
        [JsonProperty("as_num", Required = Required.Default)]
        public ulong AsNumber { get; set; }

        /// <summary>
        /// Gets or sets the BGP AS number of this subnet's parent AS.
        /// </summary>
        [JsonProperty("as_parent", Required = Required.Default)]
        public ulong AsParent { get; set; }
    }
}