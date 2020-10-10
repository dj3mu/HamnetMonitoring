using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SnmpSharpNet;

namespace SnmpAbstraction
{

    /// <summary>
    /// Proxy doing multi-layered lookup for OIDs in the order given layer list. First find gets returned.
    /// </summary>
    internal class DeviceSpecificMultiLayerOidLookupProxy : IDeviceSpecificOidLookup
    {
        /// <summary>
        /// The lookup dictionary backing field.
        /// </summary>
        private readonly IReadOnlyList<IDeviceSpecificOidLookup> layers;

        /// <summary>
        /// The combined lookup dictionary.
        /// </summary>
        private readonly IReadOnlyDictionary<RetrievableValuesEnum, DeviceSpecificOid> localLookup;

        /// <summary>
        /// Construct from a list of lookup containers.
        /// </summary>
        /// <param name="layers">The layers to query in order received.</param>
        public DeviceSpecificMultiLayerOidLookupProxy(IEnumerable<IDeviceSpecificOidLookup> layers)
        {
            if (layers == null)
            {
                throw new ArgumentNullException(nameof(layers), "the layers list is null");
            }

            this.layers = layers as IReadOnlyList<IDeviceSpecificOidLookup> ?? layers?.ToList();

            if (this.layers.Count == 0)
            {
                this.localLookup = new Dictionary<RetrievableValuesEnum, DeviceSpecificOid>();
            }
            else
            {
                // flatten the lookup layers giving precendence to the value of the first table that holds the given key
                // Example: There are two lookups given in order "3,1" in the DeviceVersionMappings.
                //          If a RetrievableValuesEnum has a value in lookup of ID 3 that value will be used. Otherwise the value of lookup #1.
                var concatenatedDicts = this.layers.Aggregate(Enumerable.Empty<KeyValuePair<RetrievableValuesEnum, DeviceSpecificOid>>(), (a, c) => a.Concat(c));
                var groupedDicts = concatenatedDicts.GroupBy(e => e.Key, e => e.Value);
                this.localLookup = groupedDicts.ToDictionary(g => g.Key, v => v.First()); // this actually does the trick of taking the hierachically first entry only
                this.MinimumSupportedSnmpVersion = layers.Max(l => l.MinimumSupportedSnmpVersion);
                this.MaximumSupportedSnmpVersion = layers.Min(l => l.MaximumSupportedSnmpVersion);
            }
        }

        /// <inheritdoc />
        public DeviceSpecificOid this[RetrievableValuesEnum key] => this.localLookup[key];

        /// <inheritdoc />
        public IEnumerable<RetrievableValuesEnum> Keys => this.localLookup.Keys;

        /// <inheritdoc />
        public IEnumerable<DeviceSpecificOid> Values => this.localLookup.Values;

        /// <inheritdoc />
        public int Count => this.localLookup.Count;

        /// <inheritdoc />
        public SnmpVersion MaximumSupportedSnmpVersion { get; }

        /// <inheritdoc />
        public SnmpVersion MinimumSupportedSnmpVersion { get; }

        /// <inheritdoc />
        public bool ContainsKey(RetrievableValuesEnum key)
        {
            return this.localLookup.ContainsKey(key);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<RetrievableValuesEnum, DeviceSpecificOid>> GetEnumerator()
        {
            return this.localLookup.GetEnumerator();
        }

        /// <inheritdoc />
        public bool TryGetValue(RetrievableValuesEnum key, out DeviceSpecificOid value)
        {
            return this.localLookup.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public bool TryGetValues(out DeviceSpecificOid[] oidValues, params RetrievableValuesEnum[] valuesToQuery)
        {
            bool atLeastOneFound = false;
            oidValues = new DeviceSpecificOid[valuesToQuery.Length];
            for (int i = 0; i < valuesToQuery.Length; i++)
            {
                DeviceSpecificOid value;
                if (this.localLookup.TryGetValue(valuesToQuery[i], out value))
                {
                    atLeastOneFound = true;
                    oidValues[i] = value;
                }
                else
                {
                    oidValues[i] = null;
                }
            }

            return atLeastOneFound;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.localLookup.GetEnumerator();
        }
    }
}