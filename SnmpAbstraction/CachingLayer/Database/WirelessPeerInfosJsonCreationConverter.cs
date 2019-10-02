using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SnmpAbstraction
{
    /// <summary>
    /// <see cref="JsonConverter" /> for a wireless peer infos.
    /// </summary>
    internal class WirelessPeerInfosJsonCreationConverter : CustomCreationConverter<IWirelessPeerInfos>
    {
        /// <inheritdoc />
        public override IWirelessPeerInfos Create(Type objectType)
        {
            return new SerializableWirelessPeerInfos();
        }
    }
}
