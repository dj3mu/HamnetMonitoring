using System;
using Newtonsoft.Json;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Container for the subnet data set as received from HamnetDB.
    /// </summary>
    [JsonObject]
    internal class JsonSubnetDataSet
    {
        /// <summary>
        /// Gets or sets the date and time when the entry has last been edited.
        /// </summary>
        [JsonProperty("edited", Required = Required.Default)]
        public DateTime Edited { get; set; }

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
        /// Gets or sets the last editor of this subnet.
        /// </summary>
        [JsonProperty("editor", Required = Required.Default)]
        public string Editor { get; set; }

        /// <summary>
        /// Gets or sets the maintainer of this subnet.
        /// </summary>
        [JsonProperty("maintainer", Required = Required.Default)]
        public string Maintainer { get; set; }

        /// <summary>
        /// Gets or sets a comma-separated list of readio parameters of this this host.
        /// </summary>
        [JsonProperty("radioparam", Required = Required.Default)]
        public string RadioParameters { get; set; }

        /// <summary>
        /// Gets or sets the version of this data set.
        /// </summary>
        [JsonProperty("version", Required = Required.Default)]
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this subnet is editable by maintainer only.
        /// </summary>
        [JsonProperty("rw_maint", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool MaintainerEditableOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this subnet entry has been deleted and shall not be considered any more.
        /// </summary>
        /// <value>
        /// <c>1</c> means true, <c>0</c> false.
        /// </value>
        [JsonProperty("deleted", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the ID of this subnet.
        /// </summary>
        [JsonProperty("id", Required = Required.Default)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to check this subnet.
        /// </summary>
        [JsonProperty("no_check", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool NoCheck { get; set; }

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