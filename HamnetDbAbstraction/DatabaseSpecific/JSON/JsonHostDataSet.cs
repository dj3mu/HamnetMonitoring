using Newtonsoft.Json;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Container for the host data set as received from HamnetDB.
    /// </summary>
    [JsonObject]
    internal class JsonHostDataSet : JsonDataSetBase
    {
        /// <summary>
        /// Gets or sets the IP address of the host.
        /// </summary>
        [JsonProperty("ip", Required = Required.Always)]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the host name to be used for this host.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of this host.
        /// </summary>
        [JsonProperty("typ", Required = Required.Always)]
        public string Typ { get; set; }

        /// <summary>
        /// Gets or sets the site name (call sign) that this host belongs to.
        /// </summary>
        [JsonProperty("site", Required = Required.Always)]
        public string Site { get; set; }

        /// <summary>
        /// Gets or sets the alias names of this host.
        /// </summary>
        [JsonProperty("aliases", Required = Required.Default)]
        public string Aliases { get; set; }

        /// <summary>
        /// Gets or sets the comment given to this host.
        /// </summary>
        [JsonProperty("comment", Required = Required.Default)]
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the MAC address of this host.
        /// </summary>
        [JsonProperty("mac", Required = Required.Default)]
        public string MacAddress { get; set; }

        /// <summary>
        /// Gets or sets a comma-separated list of readio parameters of this this host.
        /// </summary>
        [JsonProperty("radioparam", Required = Required.Default)]
        public string RadioParameters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to monitor routing.
        /// </summary>
        [JsonProperty("routing", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool Routing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ping this host.
        /// </summary>
        [JsonProperty("no_ping", Required = Required.Default), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool NoPing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to monitor the RSSI.
        /// </summary>
        [JsonProperty("monitor", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool Monitor { get; set; }
    }
}