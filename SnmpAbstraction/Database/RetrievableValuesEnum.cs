namespace SnmpAbstraction
{
    /// <summary>
    /// Enumeration of supported value meanings.
    /// </summary>
    internal enum RetrievableValuesEnum
    {
        /// <summary>
        /// Some string representing the device model (hardware plus, if applicable, hardware version) as precise as possbile.
        /// </summary>
        Model = 1,

        /// <summary>
        /// Some string representing the device's software version as precise as possbile.
        /// </summary>
        SwVersion = 2,

        /// <summary>
        /// Root OID to be walked to get a list of all available interface IDs.
        /// </summary>
        InterfaceIdWalkRoot = 3,

        /// <summary>
        /// Root OID to access with interface ID appended in order to get the interface type.
        /// </summary>
        InterfaceTypeWalkRoot = 4,

        /// <summary>
        /// Root OID to access with interface ID appended in order to get the interface's MAC address.
        /// </summary>
        InterfaceMacAddressWalkRoot = 5,

        /// <summary>
        /// Root OID to get the MAC addresses of all remote devices that this device is registered with
        /// </summary>
        WlanRemoteMacAddressWalkRoot = 6,

        /// <summary>
        /// Root OID to get the combined TX signal strength (i.e. the strength of our signal as reported by the remote side) of all MIMO streams.<br/>
        /// The MAC address (in dotted decimal) as well as the Interface ID need to be appended.<br/>
        /// Example:<br/>
        ///     MAC:  02:0C:42:3A:52:C0<br/>
        ///     Interface: 6<br/>
        ///     This value: .1.3.6.1.4.1.14988.1.1.1.2.1.19<br/>
        ///     ==> Full OID: .1.3.6.1.4.1.14988.1.1.1.2.1.19.2.12.66.58.82.192.6<br/>
        /// </summary>
        TxSignalStrengthAppendMacAndInterfaceId = 7,

        /// <summary>
        /// Root OID to get the combined RX signal strength of all MIMO streams.<br/>
        /// The MAC address (in dotted decimal) as well as the Interface ID need to be appended.<br/>
        /// Example:<br/>
        ///     MAC:  02:0C:42:3A:52:C0<br/>
        ///     Interface: 6<br/>
        ///     This value: .1.3.6.1.4.1.14988.1.1.1.2.1.14<br/>
        ///     ==> Full OID: .1.3.6.1.4.1.14988.1.1.1.2.1.14.2.12.66.58.82.192.6<br/>
        /// </summary>
        RxSignalStrengthApAppendMacAndInterfaceId = 8,

        /// <summary>
        /// Root OID to get the linkuptime.<br/>
        /// The MAC address (in dotted decimal) as well as the Interface ID need to be appended.<br/>
        /// Example:<br/>
        ///     MAC:  02:0C:42:3A:52:C0<br/>
        ///     Interface: 6<br/>
        ///     This value: .1.3.6.1.4.1.14988.1.1.1.2.1.11<br/>
        ///     ==> Full OID: .1.3.6.1.4.1.14988.1.1.1.2.1.11.2.12.66.58.82.192.6<br/>
        /// </summary>
        LinkUptimeAppendMacAndInterfaceId = 9,

        /// <summary>
        /// Root OID to get the number of currently connected wireless clients.<br/>
        /// Usually, the ID of the wireless interface will be appended.<br/>
        /// A value of 0 together with a non-empty registration table can also serve the purpose of distinguishing a clients from access points.
        /// </summary>
        WirelessClientCount = 10,

        /// <summary>
        /// Root OID to get the RX signal strength of MIMO stream 0.<br/>
        /// The MAC address (in dotted decimal) as well as the Interface ID need to be appended.<br/>
        /// Example:<br/>
        ///     MAC:  02:0C:42:3A:52:C0<br/>
        ///     Interface: 6<br/>
        ///     This value: .1.3.6.1.4.1.14988.1.1.1.2.1.14<br/>
        ///     ==> Full OID: .1.3.6.1.4.1.14988.1.1.1.2.1.14.2.12.66.58.82.192.6<br/>
        /// </summary>
        RxSignalStrengthCh0AppendMacAndInterfaceId = 11,

        /// <summary>
        /// Root OID to get the RX signal strength of MIMO stream 1.<br/>
        /// The MAC address (in dotted decimal) as well as the Interface ID need to be appended.<br/>
        /// Example:<br/>
        ///     MAC:  02:0C:42:3A:52:C0<br/>
        ///     Interface: 6<br/>
        ///     This value: .1.3.6.1.4.1.14988.1.1.1.2.1.16<br/>
        ///     ==> Full OID: .1.3.6.1.4.1.14988.1.1.1.2.1.16.2.12.66.58.82.192.6<br/>
        /// </summary>
        RxSignalStrengthCh1AppendMacAndInterfaceId = 12,

        /// <summary>
        /// Root OID to get the RX signal strength of MIMO stream 2.<br/>
        /// The MAC address (in dotted decimal) as well as the Interface ID need to be appended.<br/>
        /// Example:<br/>
        ///     MAC:  02:0C:42:3A:52:C0<br/>
        ///     Interface: 6<br/>
        ///     This value: .1.3.6.1.4.1.14988.1.1.1.2.1.18<br/>
        ///     ==> Full OID: .1.3.6.1.4.1.14988.1.1.1.2.1.18.2.12.66.58.82.192.6<br/>
        /// </summary>
        RxSignalStrengthCh2AppendMacAndInterfaceId = 13,
    }
}
