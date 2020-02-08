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
        public DateTime Edited { get; set; } = DateTime.MinValue;

        /// <inheritdoc />
        [JsonProperty("editor", Required = Required.Default)]
        public string Editor { get; set; } = string.Empty;

        /// <inheritdoc />
        [JsonProperty("maintainer", Required = Required.Default)]
        public string Maintainer { get; set; } = string.Empty;

        /// <inheritdoc />
        [JsonProperty("version", Required = Required.Always)]
        public int Version { get; set; }

        /// <inheritdoc />
        [JsonProperty("rw_maint", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool MaintainerEditableOnly { get; set; }

        /// <inheritdoc />
        [JsonProperty("deleted", Required = Required.Always), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool Deleted { get; set; }

        /// <inheritdoc />
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; set; }

        /// <inheritdoc />
        [JsonProperty("no_check", Required = Required.Default), JsonConverter(typeof(JsonIntToBoolConverter))]
        public bool NoCheck { get; set; } = false;
    }
}