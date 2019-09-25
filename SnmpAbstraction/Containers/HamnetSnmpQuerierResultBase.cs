using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public virtual TimeSpan GetQueryDuration()
        {
            return queryDuration;
        }

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
        public string ToConsoleString()
        {
            StringBuilder returnBuilder = new StringBuilder(128);
            returnBuilder.Append("Device ").Append(this.DeviceAddress).Append(" (").Append(this.DeviceModel).AppendLine("):");
            returnBuilder.AppendLine(SnmpAbstraction.IndentLines(this.ToTextString()));
            returnBuilder.Append(SnmpAbstraction.IndentLines("--> Query took ")).Append(this.GetQueryDuration().TotalMilliseconds).Append(" ms");

            return returnBuilder.ToString();
        }
        
        /// <summary>
        /// Prints the result body to the given <see cref="TextWriter" /> using
        /// human-readable formatting.
        /// </summary>
        /// <returns>A string with the human readable data.</returns>
        public abstract string ToTextString();
        
        /// <inheritdoc />
        public override string ToString()
        {
            return this.ToConsoleString();
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

        /// <summary>
        /// Container for a cachable OID.
        /// </summary>
        private class CachableOid : ICachableOid
        {
            
            /// <summary>
            /// OID list backing field.
            /// </summary>
            private readonly IList<Oid> oids = null;

            /// <summary>
            /// Constructs from an enumeration of OIDs.
            /// </summary>
            /// <param name="deviceAddress">The device's IP address.</param>
            /// <param name="meaning">The meaning of the OID.</param>
            /// <param name="oids">The enumeration of OIDs to add.</param>
            public CachableOid(IpAddress deviceAddress, CachableValueMeanings meaning, IEnumerable<Oid> oids)
            {
                if (oids is null)
                {
                    throw new ArgumentNullException(nameof(oids), "Enumeration of OIDs is null");
                }

                this.oids = oids as IList<Oid> ?? oids.ToList();
                if (this.oids.Count == 0)
                {
                    throw new ArgumentException("Enumeration of OIDs does not contain any element", nameof(oids));
                }

                this.Address = deviceAddress ?? throw new ArgumentNullException(nameof(deviceAddress), "IP address of the cachable OID is null");
                this.Meaning = meaning;
            }

            /// <summary>
            /// Constructs for a single OID.
            /// </summary>
            /// <param name="deviceAddress">The device's IP address.</param>
            /// <param name="meaning">The meaning of the OID.</param>
            /// <param name="oid">The OID.</param>
            public CachableOid(IpAddress deviceAddress, CachableValueMeanings meaning, Oid oid)
            {
                this.oids = new List<Oid>
                {
                    oid
                };

                this.Address = deviceAddress ?? throw new ArgumentNullException(nameof(deviceAddress), "IP address of the cachable OID is null");
                this.Meaning = meaning;
            }

            /// <inheritdoc />
            public IpAddress Address { get; }

            /// <inheritdoc />
            public CachableValueMeanings Meaning { get; }

            /// <inheritdoc />
            public Oid Oid
            {
                get
                {
                    if (this.oids.Count > 1)
                    {
                        throw new InvalidOperationException($"Cannot return a single OID because this instance of ICacheableOid has {this.oids.Count} elements: Returning a single OID would be ambiguous");
                    }

                    if (this.oids.Count == 0)
                    {
                        throw new InvalidOperationException("Cannot return a single OID because this instance of ICachableOid does not have any element");
                    }

                    return this.oids[0];
                }
            }

            /// <inheritdoc />
            public bool IsSingleOid => this.oids.Count == 1;

            /// <inheritdoc />
            public IEnumerable<Oid> Oids => this.oids;
        }
    }
}
