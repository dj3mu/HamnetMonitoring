using System;
using NUnit.Framework;
using HamnetDbAbstraction;

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
        }
    }
}
