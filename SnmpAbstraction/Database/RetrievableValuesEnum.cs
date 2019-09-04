namespace SnmpAbstraction
{
    /// <summary>
    /// Enumeration of supported value meanings.
    /// </summary>
    internal enum RetrievableValuesEnum
    {
        // Some string representing the device model (hardware plus, if applicable, hardware version) as precise as possbile.
        Model = 1,

        // Some string representing the device's software version as precise as possbile.
        SwVersion = 2,

        // Root OID to be walked to get a list of all available interface IDs.
        InterfaceIdWalkRoot = 3,

        // Root OID to accessed with interface ID appended in order to get the interface type.
        InterfaceTypeWalkRoot = 4,

        // Root OID to accessed with interface ID appended in order to get the interface's MAC address.
        InterfaceMacAddressWalkRoot = 5
    }
}
