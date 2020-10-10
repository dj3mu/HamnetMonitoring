using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SnmpSharpNet;

namespace SnmpAbstraction
{

    /// <summary>
    /// Data container representing the OID lookup for the values according to <see cref="RetrievableValuesEnum" />.
    /// </summary>
    internal class DeviceSpecificOidLookup : IDeviceSpecificOidLookup
    {
        /// <summary>
        /// The lookup dictionary backing field.
        /// </summary>
        private readonly Dictionary<RetrievableValuesEnum, DeviceSpecificOid> localLookup;

        /// <summary>
        /// Construct from a list of lookup containers.
        /// </summary>
        /// <param name="deviceSpecificOids">The list to construct from.</param>
        /// <param name="minimumSupportedSnmpVersion">The minimum supported SNMP version.</param>
        /// <param name="maximumSupportedSnmpVersion">The maximum supported SNMP version.</param>
        public DeviceSpecificOidLookup(IEnumerable<DeviceSpecificOid> deviceSpecificOids, SnmpVersion minimumSupportedSnmpVersion, SnmpVersion maximumSupportedSnmpVersion)
        {
            if (deviceSpecificOids == null)
            {
                throw new ArgumentNullException(nameof(deviceSpecificOids), "device specific OIDs enumeration is null");
            }

            this.localLookup = deviceSpecificOids.ToDictionary(doid => (RetrievableValuesEnum)doid.RetrievableValueId, doid => doid);
            this.MaximumSupportedSnmpVersion = maximumSupportedSnmpVersion;
            this.MinimumSupportedSnmpVersion = minimumSupportedSnmpVersion;
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