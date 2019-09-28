using System;
using System.Net;
using Newtonsoft.Json;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// <see cref="JsonConverter" /> for a Semantic Version
    /// </summary>
    internal class SemanticVersionJsonConverter : JsonConverter<SemanticVersion>
    {
        /// <inheritdoc />
        public override SemanticVersion ReadJson(JsonReader reader, Type objectType, SemanticVersion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string versionString = (string)reader.Value;

            return SemanticVersion.Parse(versionString);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, SemanticVersion value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}