using System;
using System.Net;
using Newtonsoft.Json;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// <see cref="JsonConverter" /> for an SNMP IpAddress.
    /// </summary>
    internal class IpAddressJsonConverter : JsonConverter<IpAddress>
    {
        /// <inheritdoc />
        public override IpAddress ReadJson(JsonReader reader, Type objectType, IpAddress existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string ipString = (string)reader.Value;

            IPAddress parsedAddress = IPAddress.Parse(ipString);

            return new IpAddress(parsedAddress);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, IpAddress value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}