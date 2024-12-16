using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly List<Oid> oidsBacking;

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
        public bool IsSingleOid => this.oidsBacking.Count == 1;

        /// <inheritdoc />
        [JsonIgnore]
        public Oid Oid => this.oidsBacking.Single();

        /// <inheritdoc />
        [JsonIgnore]
        public IEnumerable<Oid> Oids  => this.oidsBacking;

        /// <inheritdoc />
        [JsonProperty(Required = Required.Default)]
        public double Factor { get; set; } = 1.0;

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder returnBuilder = new StringBuilder();

            returnBuilder.Append("Cachable OIDs for ").Append(this.Address).Append(", meaning '").Append(this.Meaning).Append("', Factor ").Append(this.Factor).Append(": ");

            bool isFirst = true;
            foreach (var item in this.oidsBacking)
            {
                if (!isFirst)
                {
                    returnBuilder.Append(", ");
                }
                else
                {
                    isFirst = false;
                }

                returnBuilder.Append(item);
            }

            return returnBuilder.ToString();
        }
    }
}
