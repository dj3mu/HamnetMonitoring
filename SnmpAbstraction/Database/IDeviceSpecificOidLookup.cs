using System.Collections.Generic;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to a device-specific OID lookup table.
    /// </summary>
    internal interface IDeviceSpecificOidLookup : IReadOnlyDictionary<RetrievableValuesEnum, DeviceSpecificOid>
    {
    }
}