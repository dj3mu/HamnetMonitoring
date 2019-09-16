using NUnit.Framework;
using HamnetDbAbstraction;
using System;
using System.Linq;

namespace HamnetDbAbstractionTests
{
    /// <summary>
    /// Tests for all kinds of extension methods (I don't want to scatter the tests too much).
    /// </summary>
    public class ExtensionTests
    {
        /// <summary>
        /// The test setup method.
        /// </summary>
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Test for <see cref="HamnetDbSubnetExtensions.AssociateHosts" /> method.
        /// </summary>
        [Test]
        public void AssociateHostsTests()
        {
            var accessor = HamnetDbProvider.Instance.GetHamnetDb(TestConstants.ConnectionStringFilePath);

            var hosts = accessor.QueryMonitoredHosts();
            var subnets = accessor.QuerySubnets();

            var association = subnets.AssociateHosts(hosts);

            Assert.NotNull(association, "Returned association is null");
            Assert.Greater(association.Count, 0, "0 associations returned");

            Console.WriteLine($"Received {association.Count} associations");

            var pairs = association.Where(a => a.Value.Count == 2);

            Console.WriteLine($"Received {pairs.Count()} unique pairs");
        }

        /// <summary>
        /// Test for <see cref="HamnetDbAccessExtensions.UniqueMonitoredHostPairsInSameSubnet" /> method.
        /// </summary>
        [Test]
        public void UniqueMonitoredHostPairsInSameSubnet()
        {
            var accessor = HamnetDbProvider.Instance.GetHamnetDb(TestConstants.ConnectionStringFilePath);

            var uniquePairs = accessor.UniqueMonitoredHostPairsInSameSubnet();

            Assert.NotNull(uniquePairs, "Unique pairs received is null");
            Assert.Greater(uniquePairs.Count, 0, "0 unique pairs received");

            Console.WriteLine($"Received {uniquePairs.Count} unique pairs");
        }
    }
}
