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
        /// Root OID to get the TX signal strength (i.e. the strength of our signal as reported by the remote side).<br/>
        /// The MAC address (in dotted decimal) as well as the Interface ID need to be appended.<br/>
        /// Example:<br/>
        ///     MAC:  02:0C:42:3A:52:C0<br/>
        ///     Interface: 6<br/>
        ///     This value: .1.3.6.1.4.1.14988.1.1.1.2.1.19<br/>
        ///     ==> Full OID: .1.3.6.1.4.1.14988.1.1.1.2.1.19.2.12.66.58.82.192.6<br/>
        /// </summary>
        TxSignalStrengthAppendMacAndInterfaceId = 7,

        /// <summary>
        /// Root OID to get the RX signal strength (i.e. the strength of our signal as reported by the remote side).<br/>
        /// The MAC address (in dotted decimal) as well as the Interface ID need to be appended.<br/>
        /// Example:<br/>
        ///     MAC:  02:0C:42:3A:52:C0<br/>
        ///     Interface: 6<br/>
        ///     This value: .1.3.6.1.4.1.14988.1.1.1.2.1.14<br/>
        ///     ==> Full OID: .1.3.6.1.4.1.14988.1.1.1.2.1.14.2.12.66.58.82.192.6<br/>
        /// </summary>
        RxSignalStrengthAppendMacAndInterfaceId = 8,

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
    }
}
