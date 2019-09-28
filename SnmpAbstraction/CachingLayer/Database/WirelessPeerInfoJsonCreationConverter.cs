using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SnmpAbstraction
{
    /// <summary>
    /// <see cref="JsonConverter" /> for a wireless peer info.
    /// </summary>
    internal class WirelessPeerInfoJsonCreationConverter : CustomCreationConverter<IWirelessPeerInfo>
    {
        /// <inheritdoc />
        public override IWirelessPeerInfo Create(Type objectType)
        {
            return new SerializableWirelessPeerInfo();
        }
    }
}
