using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SnmpAbstraction
{
    /// <summary>
    /// <see cref="JsonConverter" /> for a wireless peer info.
    /// </summary>
    internal class InterfaceDetailJsonCreationConverter : CustomCreationConverter<IInterfaceDetail>
    {
        /// <inheritdoc />
        public override IInterfaceDetail Create(Type objectType)
        {
            return new SerializableInterfaceDetail();
        }
    }
}
