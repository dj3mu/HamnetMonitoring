using System;
using NUnit.Framework;
using HamnetDbAbstraction;
using NUnit.Framework.Legacy;

namespace HamnetDbAbstractionTests
{
    /// <summary>
    /// Tests for all kinds of extension methods (I don't want to scatter the tests too much).
    /// </summary>
    public class MySqlDbAccessorTests
    {
        /// <summary>
        /// The test setup method.
        /// </summary>
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Test for construction and proper error in case of invalid connection string.
        /// </summary>
        [Test]
        public void ConstructionTests()
        {
            // the passed param is intentionally NOT the connection string as we want to test
            // error behaviour !
            var accessor = new MySqlHamnetDbAccessor(TestConstants.ConnectionStringFilePath, null);

            ClassicAssert.NotNull(accessor, "Constructed accessor is null");
            ClassicAssert.AreEqual(TestConstants.ConnectionStringFilePath, accessor.ConnectionString);

            Assert.Throws<ArgumentException>(() => accessor.QueryMonitoredHosts());
        }

        /// <summary>
        /// Test for querying of router hosts (implicitly testing the provider).
        /// </summary>
        [Test]
        public void QueryRouterHostsTest()
        {
            var accessor = HamnetDbProvider.Instance.GetHamnetDb(TestConstants.ConnectionStringFilePath);

            ClassicAssert.NotNull(accessor, "The accessor returned by provider is null");

            var routerHosts = accessor.QueryBgpRouters();

            ClassicAssert.NotNull(routerHosts, "The router hosts return data is null");
            ClassicAssert.Greater(routerHosts.Count, 0, "No hosts returned at all");
        }

        /// <summary>
        /// Test for querying of monitored hosts (implicitly testing the provider).
        /// </summary>
        [Test]
        public void QueryMonitoredHostsTest()
        {
            var accessor = HamnetDbProvider.Instance.GetHamnetDb(TestConstants.ConnectionStringFilePath);

            ClassicAssert.NotNull(accessor, "The accessor returned by provider is null");

            var monitoredHosts = accessor.QueryMonitoredHosts();

            ClassicAssert.NotNull(monitoredHosts, "The monitored hosts return data is null");
            ClassicAssert.Greater(monitoredHosts.Count, 0, "No hosts returned at all");
        }

        /// <summary>
        /// Test for querying of subnets
        /// </summary>
        [Test]
        public void QuerySubnetsTest()
        {
            var accessor = HamnetDbProvider.Instance.GetHamnetDb(TestConstants.ConnectionStringFilePath);

            ClassicAssert.NotNull(accessor, "The accessor returned by provider is null");

            var subnets = accessor.QuerySubnets();

            ClassicAssert.NotNull(subnets, "The subnets return data is null");
            ClassicAssert.Greater(subnets.Count, 0, "No subnets returned at all");
        }
    }
}
