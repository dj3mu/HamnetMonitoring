using System;
using Newtonsoft.Json;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Container for the host data set as received from HamnetDB.
    /// </summary>
    [JsonObject]
    internal class JsonHostDataSet
    {
        /// <summary>
        /// Gets or sets the date and time when the entry has last been edited.
        /// </summary>
        [JsonProperty("edited", Required = Required.Default)]
        public DateTime Edited { get; set; }

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
        /// Gets or sets the last editor of this host.
        /// </summary>
        [JsonProperty("editor", Required = Required.Default)]
        public string Editor { get; set; }

        /// <summary>
        /// Gets or sets the maintainer of this host.
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
        /// Gets or sets a value indicating whether to monitor routing.
        /// </summary>
        [JsonProperty("routing", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool Routing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this host is editable by maintainer only.
        /// </summary>
        [JsonProperty("rw_maint", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool MaintainerEditableOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to ping this host.
        /// </summary>
        [JsonProperty("no_ping", Required = Required.Default), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool NoPing { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this host entry has been deleted and shall not be considered any more.
        /// </summary>
        /// <value>
        /// <c>1</c> means true, <c>0</c> false.
        /// </value>
        [JsonProperty("deleted", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the ID of this host.
        /// </summary>
        [JsonProperty("id", Required = Required.Default)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to monitor the RSSI.
        /// </summary>
        [JsonProperty("monitor", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool Monitor { get; set; }
    }
}