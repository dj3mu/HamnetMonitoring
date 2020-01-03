using System;
using Newtonsoft.Json;

namespace RestService.Model
{
    internal class TimeSpanAsTotalSecondsConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var valueString = reader.ReadAsDouble();

            return valueString.HasValue ? TimeSpan.FromSeconds(valueString.Value) : TimeSpan.Zero;
        }

        public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
        {
            writer.WriteValue(value.TotalSeconds);
        }
    }
}
