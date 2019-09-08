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
        /// The address of a MikroTik device to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an MTik device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressMikrotik1 { get; } = new IpAddress("44.224.10.222");

        /// <summary>
        /// A second address of a MikroTik device to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an MTik device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressMikrotik2 { get; } = new IpAddress("44.224.10.218");

        /// <summary>
        /// The address of a Ubiquiti device to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an UBNT device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressUbnt1 { get; } = new IpAddress("44.224.12.226");

        /// <summary>
        /// A second address of a Ubiquiti device to use for the tests.
        /// </summary>
        /// <remarks>Obviously this might not forever remain an UBNT device. Feel free to adjust if needed.</remarks>
        public static IpAddress TestAddressUbnt2 { get; } = new IpAddress("44.224.12.229");

        /// <summary>
        /// Address of localhost.
        /// </summary>
        public static IpAddress TestAddressNotResponding { get; } = new IpAddress("127.0.0.1");
    }
}