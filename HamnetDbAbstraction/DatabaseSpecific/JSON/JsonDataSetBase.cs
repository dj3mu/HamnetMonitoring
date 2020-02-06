using System;
using Newtonsoft.Json;

namespace HamnetDbAbstraction
{

    /// <summary>
    /// Container for the base data of every(?) table in HamnetDB.
    /// </summary>
    [JsonObject]
    internal class JsonDataSetBase : IHamnetDbBaseData
    {
        /// <inheritdoc />
        [JsonProperty("edited", Required = Required.Default)]
        public DateTime Edited { get; set; }

        /// <inheritdoc />
        [JsonProperty("editor", Required = Required.Default)]
        public string Editor { get; set; }

        /// <inheritdoc />
        [JsonProperty("maintainer", Required = Required.Default)]
        public string Maintainer { get; set; }

        /// <inheritdoc />
        [JsonProperty("version", Required = Required.Default)]
        public int Version { get; set; }

        /// <inheritdoc />
        [JsonProperty("rw_maint", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool MaintainerEditableOnly { get; set; }

        /// <inheritdoc />
        [JsonProperty("deleted", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool Deleted { get; set; }

        /// <inheritdoc />
        [JsonProperty("id", Required = Required.Default)]
        public int Id { get; set; }

        /// <inheritdoc />
        [JsonProperty("no_check", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool NoCheck { get; set; }
    }
}