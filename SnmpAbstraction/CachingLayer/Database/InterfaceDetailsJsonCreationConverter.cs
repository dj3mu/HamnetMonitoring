using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SnmpAbstraction
{
    /// <summary>
    /// <see cref="JsonConverter" /> for a interface details.
    /// </summary>
    internal class InterfaceDetailsJsonCreationConverter : CustomCreationConverter<IInterfaceDetails>
    {
        /// <inheritdoc />
        public override IInterfaceDetails Create(Type objectType)
        {
            return new SerializableInterfaceDetails();
        }
    }
}
