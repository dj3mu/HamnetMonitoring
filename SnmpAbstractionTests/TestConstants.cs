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
        /// The address to use for the tests.
        /// </summary>
        public static IpAddress TestAddress1 { get; } = new IpAddress("44.224.10.222");

        /// <summary>
        /// A second address to use for the tests.
        /// </summary>
        public static IpAddress TestAddress2 { get; } = new IpAddress("44.224.10.218");

        /// <summary>
        /// Address of localhost.
        /// </summary>
        public static IpAddress LocalhostAdddress { get; } = new IpAddress("127.0.0.1");
    }
}