using SnmpSharpNet;

namespace SnmpAbstractionTests
{
    /// <summary>
    /// Some constants useful for the unit tests
    /// </summary>
    /// <remarks> The address put here is one of the author's private Hamnet client hardware
    /// Feel free to use it for first tests. But please consider switching to one of your
    /// own devices when doing more extensive testind and/or development.
    /// </remarks>
    public static class TestConstants
    {
        /// <summary>
        /// The address of a ALIX device to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an ALIX device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressAlix1 { get; } = new IpAddress("44.224.90.86"); // HB9AM link DB0WBD, ALIX

        /// <summary>
        /// The address of an MTIK device to use for link-tests to Alix devices.
        /// </summary>
        /// <remarks>Obviously this might not forever remain a device linking to ALIX. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressAlix2 { get; } = new IpAddress("44.224.90.82"); // DB0WBD link HB9AM, MTik (but links to an Alix device)

        /// <summary>
        /// The address of a MikroTik BGP routing device to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an MTik device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressMikrotikRouter1 { get; } = new IpAddress("44.224.10.78"); // DB0EBE power box, MTik

        /// <summary>
        /// The address of a MikroTik device to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an MTik device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressMikrotik1 { get; } = new IpAddress("44.224.10.222"); // DB0ON link DB0EBE, MTik

        /// <summary>
        /// The address of a MikroTik device to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an MTik device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressMikrotik3 { get; } = new IpAddress("44.224.10.74"); // DB0ZM link DB0EBE, MTik

        /// <summary>
        /// A second address of a MikroTik device to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an MTik device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressMikrotik2 { get; } = new IpAddress("44.224.10.218"); // DB0EBE link DB0ON, MTik

        /// <summary>
        /// A second address of a MikroTik device to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an MTik device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressMikrotik4 { get; } = new IpAddress("44.224.10.77"); // DB0EBE link DB0ZM, MTik

        /// <summary>
        /// The address of a Ubiquiti device with AirOS 6 to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an UBNT device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressUbntAirOs6side1 { get; } = new IpAddress("44.224.12.226"); // DB0ZB (Schneeberg), AirOS 6
        
        /// <summary>
        /// The address of a Ubiquiti device with AirOS 8 to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an UBNT device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressUbntAirOs8side1 { get; } = new IpAddress("44.224.82.34"); // DB0KWE (Karftwerk Weisweiler), AirOS 8

        /// <summary>
        /// The address of a Ubiquiti device with AirOS 4 to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an UBNT device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressUbntAirOs4side1 { get; } = new IpAddress("44.224.90.106"); // DM0ZOG (HS Offenburg), AirOS 4
        
        /// <summary>
        /// The address of a Ubiquiti device with AirFiber to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an UBNT device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressUbntAirFiberSide1 { get; } = new IpAddress("44.224.28.42"); // DB0GIE, Giescheid

        /// <summary>
        /// A second address of a Ubiquiti device with AirOS 6 to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an UBNT device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressUbntAirOs6side2 { get; } = new IpAddress("44.224.12.229"); // DB0MAK (Marktredwitz), AirOS 6
        
        /// <summary>
        /// The address of a Ubiquiti device with AirOS 8 to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an UBNT device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressUbntAirOs8side2 { get; } = new IpAddress("44.224.82.37"); // DB0WA (Aachen), AirOS 8

        /// <summary>
        /// The address of a Ubiquiti device with AirOS 4 to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an UBNT device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressUbntAirOs4side2 { get; } = new IpAddress("44.224.90.109"); // DB0ORT (Schwend, Ortenau), AirOS 4
        
        /// <summary>
        /// The address of a Ubiquiti device with AirFiber to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an UBNT device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressUbntAirFiberSide2 { get; } = new IpAddress("44.224.28.45"); // ON0TB, Botrange

        /// <summary>
        /// Address of localhost.
        /// </summary>
        public static IpAddress TestAddressNotResponding { get; } = new IpAddress("127.0.0.1");
    }
}