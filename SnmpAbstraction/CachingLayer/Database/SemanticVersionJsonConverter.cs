using System;
using Newtonsoft.Json;
using SemVersion;

namespace SnmpAbstraction
{
    /// <summary>
    /// <see cref="JsonConverter" /> for a Semantic Version
    /// </summary>
    public class SemanticVersionJsonConverter : JsonConverter<SemanticVersion>
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