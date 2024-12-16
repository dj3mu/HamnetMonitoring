using System.Collections.Generic;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to a device-specific OID lookup table.
    /// </summary>
    internal interface IDeviceSpecificOidLookup : IReadOnlyDictionary<RetrievableValuesEnum, DeviceSpecificOid>
    {
        /// <summary>
        /// Tries to get the OIDs for the given list of values.
        /// </summary>
        /// <param name="oidValues">Returns the OID values</param>
        /// <param name="valuesToQuery">The list of values to query.</param>
        /// <returns><c>true</c> if AT LEAST ONE value could be successfully retrieved. Otherwise <c>false</c>.</returns>
        bool TryGetValues(out DeviceSpecificOid[] oidValues, params RetrievableValuesEnum[] valuesToQuery);

        /// <summary>
        /// Gets the device's minium supported SNMP version.
        /// </summary>
        SnmpVersion MinimumSupportedSnmpVersion { get; }

        /// <summary>
        /// Gets the device's maximum supported SNMP version.
        /// </summary>
        SnmpVersion MaximumSupportedSnmpVersion { get; }
    }
}