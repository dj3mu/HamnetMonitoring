using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SnmpAbstraction
{
    /// <summary>
    /// <see cref="JsonConverter" /> for an SNMP OID.
    /// </summary>
    internal class SystemDataJsonCreationConverter : CustomCreationConverter<IDeviceSystemData>
    {
        /// <inheritdoc />
        public override IDeviceSystemData Create(Type objectType)
        {
            return new SerializableSystemData();
        }
    }
}