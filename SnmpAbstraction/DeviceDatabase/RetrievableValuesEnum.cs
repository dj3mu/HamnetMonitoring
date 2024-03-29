﻿namespace SnmpAbstraction
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

        /// <summary>
        /// OID for a value indicating the wireless mode (i.e. AP, Station, Repeater, ...)
        /// </summary>
        WirelessMode = 14,

        /// <summary>
        /// Root OID to access with interface ID appended in order to get the interface name.
        /// </summary>
        InterfaceNameWalkRoot = 15,

        /// <summary>
        /// Root OID to get the wireless remote peer MAC address.<br/>
        /// The Interface ID needs to be appended.
        /// </summary>
        WlanRemoteMacAddressAppendInterfaceId = 16,

        /// <summary>
        /// OID to directly query RX signal strength from.<br/>
        /// Used by some P2P devices that only and exactly support one remote.
        /// </summary>
        RxSignalStrengthImmediateOid = 17,

        /// <summary>
        /// Root OID to get the overall CCQ value of the interface.<br/>
        /// The Interface ID needs to be appended.
        /// </summary>
        OverallCcqAppendInterfaceId = 18,

        /// <summary>
        /// Root OID to get the wireless remote peer MAC address from the first found sub-digit
        /// </summary>
        WlanRemoteMacAddressUseFirstSubdigit = 19,

        /// <summary>
        /// Root OID to get the RX signal strength from given OID with interface ID appended
        /// </summary>
        RxSignalStrengthCh0AppendInterfaceId = 20,

        /// <summary>
        /// RX signal strength where the chain index (e.g. for MIMOSA 1-4) is to be appended
        /// </summary>
        RxSignalStrengthAppendChainIndex = 21,

        /// <summary>
        /// Link uptime directly provided by the given OID
        /// </summary>
        LinkUptimeDirectValue = 22,

        /// <summary>
        /// TX signal strength where the chain index (e.g. for MIMOSA 1-4) is to be appended
        /// </summary>
        TxSignalStrengthAppendChainIndex = 23,
    }
}
