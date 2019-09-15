using System;
using System.Linq;
using NUnit.Framework;
using HamnetDbAbstraction;

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

            Assert.NotNull(accessor, "Constructed accessor is null");
            Assert.AreEqual(TestConstants.ConnectionStringFilePath, accessor.ConnectionString);

            Assert.Throws<ArgumentException>(() => accessor.QueryMonitoredHosts());
        }

        /// <summary>
        /// Test for querying of monitored hosts (implicitly testing the provider).
        /// </summary>
        [Test]
        public void QueryMonitoredHostsTest()
        {
            var accessor = HamnetDbProvider.Instance.GetHamnetDb(TestConstants.ConnectionStringFilePath);
            
            Assert.NotNull(accessor, "The accessir returned by provider is null");

            var monitoredHosts = accessor.QueryMonitoredHosts();

            Assert.NotNull(monitoredHosts, "The monitored hosts return data is null");
            Assert.Greater(monitoredHosts.Count, 0, "No hosts returned at all");
        }

        /// <summary>
        /// Test for querying of subnets
        /// </summary>
        [Test]
        public void QuerySubnetsTest()
        {
            var accessor = HamnetDbProvider.Instance.GetHamnetDb(TestConstants.ConnectionStringFilePath);
            
            Assert.NotNull(accessor, "The accessir returned by provider is null");

            var subnets = accessor.QuerySubnets();

            Assert.NotNull(subnets, "The subnets return data is null");
            Assert.Greater(subnets.Count, 0, "No subnets returned at all");
        }
    }
}
