using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Serializable version of cachable OIDs.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut, ItemRequired = Required.Always)]
    internal class SerializableCachableOid : ICachableOid
    {
        [JsonProperty("Oids")]
        private List<Oid> oidsBacking;

        public SerializableCachableOid()
        {
        }

        public SerializableCachableOid(params Oid[] oids)
        {
            if (oids is null)
            {
                throw new System.ArgumentNullException(nameof(oids), "Oids param array is null");
            }

            this.oidsBacking = oids.ToList();
        }

        public SerializableCachableOid(ICachableOid source)
        {
            if (source is null)
            {
                throw new System.ArgumentNullException(nameof(source), "source is null");
            }

            this.Address = source.Address;
            this.Meaning = source.Meaning;
            this.oidsBacking = source.Oids as List<Oid> ?? source.Oids.ToList();
        }

        /// <inheritdoc />
        public IpAddress Address  { get; set; }

        /// <inheritdoc />
        public CachableValueMeanings Meaning  { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public bool IsSingleOid => this.oidsBacking.Count > 1;

        /// <inheritdoc />
        [JsonIgnore]
        public Oid Oid => this.oidsBacking.Single();

        /// <inheritdoc />
        [JsonIgnore]
        public IEnumerable<Oid> Oids  => this.oidsBacking;
    }
}