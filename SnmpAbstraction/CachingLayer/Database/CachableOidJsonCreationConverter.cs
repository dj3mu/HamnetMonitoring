using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SnmpAbstraction
{
    /// <summary>
    /// <see cref="JsonConverter" /> for an SNMP OID.
    /// </summary>
    internal class CachableOidJsonCreationConverter : CustomCreationConverter<ICachableOid>
    {
        /// <inheritdoc />
        public override ICachableOid Create(Type objectType)
        {
            return new SerializableCachableOid();
        }
    }
}