using System;
using Newtonsoft.Json;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// <see cref="JsonConverter" /> for an SNMP OID.
    /// </summary>
    internal class OidJsonConverter : JsonConverter<Oid>
    {
        /// <inheritdoc />
        public override Oid ReadJson(JsonReader reader, Type objectType, Oid existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string oidString = (string)reader.Value;

            return new Oid(oidString);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, Oid value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}