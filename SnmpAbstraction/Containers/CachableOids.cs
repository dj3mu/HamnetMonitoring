using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Container for a cachable OID.
    /// </summary>
    internal class CachableOid : ICachableOid
    {

        /// <summary>
        /// OID list backing field.
        /// </summary>
        [JsonProperty]
        private readonly IList<Oid> oids = null;

        /// <summary>
        /// Constructs from an enumeration of OIDs.
        /// </summary>
        /// <param name="deviceAddress">The device's IP address.</param>
        /// <param name="meaning">The meaning of the OID.</param>
        /// <param name="oids">The enumeration of OIDs to add.</param>
        /// <param name="factor">The factor to apply to the values (if applicable: before summing)</param>
        public CachableOid(IpAddress deviceAddress, CachableValueMeanings meaning, IEnumerable<Oid> oids, double factor)
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
            this.Factor = factor;
        }

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
        /// <param name="factor">The factor to apply to the values (if applicable: before summing)</param>
        public CachableOid(IpAddress deviceAddress, CachableValueMeanings meaning, Oid oid, double factor)
        {
            this.oids = new List<Oid>
                {
                    oid
                };

            this.Address = deviceAddress ?? throw new ArgumentNullException(nameof(deviceAddress), "IP address of the cachable OID is null");
            this.Meaning = meaning;
            this.Factor = factor;
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
        [JsonIgnore]
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
        [JsonIgnore]
        public bool IsSingleOid => this.oids.Count == 1;

        /// <inheritdoc />
        [JsonIgnore]
        public IEnumerable<Oid> Oids => this.oids;

        /// <inheritdoc />
        public double Factor { get; set; } = 1.0;
    }
}