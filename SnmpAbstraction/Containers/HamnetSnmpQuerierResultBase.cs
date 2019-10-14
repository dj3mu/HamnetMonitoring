using System;
using System.Collections.Generic;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Base class for all <see cref="IHamnetSnmpQuerierResult" />.
    /// /// </summary>
    internal abstract class HamnetSnmpQuerierResultBase : IHamnetSnmpQuerierResult, ICachableOids
    {
        /// <summary>
        /// Lookup for cachable OIDs.
        /// </summary>
        private readonly Dictionary<CachableValueMeanings, ICachableOid> cachableOidLookup = new Dictionary<CachableValueMeanings, ICachableOid>();

        private static readonly BlockTextFormatter formatter = new BlockTextFormatter();

        /// <summary>
        /// Construct for the given device address and query duration.
        /// </summary>
        /// <param name="address">The IP address of the device that produced this result.</param>
        /// <param name="deviceModel">The model and version of the device.</param>
        /// <param name="queryDuration">The duration of the query.</param>
        protected HamnetSnmpQuerierResultBase(IpAddress address, string deviceModel, TimeSpan queryDuration)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address), "The IP address of the device producing this result is null");
            }

            this.DeviceAddress = address;
            this.queryDuration = queryDuration;
            this.DeviceModel = deviceModel;
        }

        /// <inheritdoc />
        public IpAddress DeviceAddress { get; }

        private readonly TimeSpan queryDuration;

        /// <inheritdoc />
        public virtual TimeSpan QueryDuration => queryDuration;

        /// <inheritdoc />
        public virtual string DeviceModel { get; }

        /// <inheritdoc />
        public IEnumerable<CachableValueMeanings> Keys => this.cachableOidLookup.Keys;

        /// <inheritdoc />
        public IEnumerable<ICachableOid> Values => this.cachableOidLookup.Values;

        /// <inheritdoc />
        public int Count => this.cachableOidLookup.Count;

        /// <inheritdoc />
        public IReadOnlyDictionary<CachableValueMeanings, ICachableOid> Oids => this.cachableOidLookup;

        /// <inheritdoc />
        public ICachableOid this[CachableValueMeanings key] => this.cachableOidLookup[key];

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder returnBuilder = new StringBuilder(128);
            returnBuilder.Append("Device ").Append(this.DeviceAddress).Append(" (").Append(this.DeviceModel).AppendLine("):");
            returnBuilder.AppendLine(SnmpAbstraction.IndentLines(formatter.Format(this)));
            returnBuilder.Append(SnmpAbstraction.IndentLines("--> Query took ")).Append(this.QueryDuration.TotalMilliseconds).Append(" ms");

            return returnBuilder.ToString();
        }

        /// <inheritdoc />
        /// <summary>
        /// Adds the given <see paramref="oid" /> as OID for the given value meaning.
        /// </summary>
        /// <param name="meaning">The value meaning that is requested by the given OID.</param>
        /// <param name="oid">The OID that can be used to request the given value.</param>
        protected void RecordCachableOid(CachableValueMeanings meaning, Oid oid)
        {
            // intentionally add or silently replace an existing value
            this.cachableOidLookup[meaning] = new CachableOid(this.DeviceAddress, meaning, oid);
        }

        /// <summary>
        /// Adds the given <see paramref="oid" /> as OID for the given value meaning.
        /// </summary>
        /// <param name="meaning">The value meaning that is requested by the given OID.</param>
        /// <param name="oids">The OID that can be used to request the given value.</param>
        protected void RecordCachableOids(CachableValueMeanings meaning, IEnumerable<Oid> oids)
        {
            // intentionally add or silently replace an existing value
            this.cachableOidLookup[meaning] = new CachableOid(this.DeviceAddress, meaning, oids);
        }
    }
}
