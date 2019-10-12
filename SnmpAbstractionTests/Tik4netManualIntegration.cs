using System;
using NUnit.Framework;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.Ip;
using tik4net.Objects.Routing.Bgp;

namespace SnmpAbstractionTests
{
    /// <summary>
    /// Tests for all kinds of extension methods (I don't want to scatter the tests too much).
    /// </summary>
    public partial class Tik4netManualINtegration
    {
        /// <summary>
        /// The test setup method.
        /// </summary>
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Test for using ITikConnection
        /// </summary>
        [Test]
        public void TikConnectionTest()
        {
            using (ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api))
            {
                connection.Open("HOST", "USER", "PASS");
                ITikCommand cmd = connection.CreateCommand("/system/identity/print");
                Console.WriteLine(cmd.ExecuteScalar());

                var advertisements = connection.LoadAll<BgpAdvertisement>();
                var instances = connection.LoadAll<BgpInstance>();
                var peers = connection.LoadAll<BgpPeer>();
                var networks = connection.LoadAll<BgpNetwork>();
                var routes = connection.LoadAll<IpRoute>();
            }
        }
    }
}
