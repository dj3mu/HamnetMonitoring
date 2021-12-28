using NUnit.Framework;
using HamnetDbAbstraction;
using System.Linq;
using System.Net;

namespace HamnetDbAbstractionTests
{
    /// <summary>
    /// Tests for the JSON HamnetDB interface
    /// </summary>
    public class JsonDbAccessorTests
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
            var accessor = new JsonHamnetDbAccessor(TestConstants.HostsUrl, TestConstants.SubnetsUrl, TestConstants.SitesUrl, null);

            Assert.NotNull(accessor, "Constructed accessor is null");
            Assert.AreEqual(TestConstants.HostsUrl, accessor.HostApiUrl);
            Assert.AreEqual(TestConstants.SubnetsUrl, accessor.SubnetsApiUrl);
        }

        /// <summary>
        /// Test for querying of router hosts (implicitly testing the provider).
        /// </summary>
        [Test]
        public void QueryRouterHostsTest()
        {
            var accessor = new JsonHamnetDbAccessor(TestConstants.HostsUrl, TestConstants.SubnetsUrl, TestConstants.SitesUrl, null);

            Assert.NotNull(accessor, "The accessor returned by provider is null");

            var routerHosts = accessor.QueryBgpRouters();

            Assert.NotNull(routerHosts, "The router hosts return data is null");
            Assert.Greater(routerHosts.Count, 0, "No hosts returned at all");
        }

        /// <summary>
        /// Test for querying of router hosts (implicitly testing the provider).
        /// </summary>
        [Test]
        public void QuerySitesTest()
        {
            var accessor = new JsonHamnetDbAccessor(TestConstants.HostsUrl, TestConstants.SitesUrl, TestConstants.SitesUrl, null);

            Assert.NotNull(accessor, "The accessor returned by provider is null");

            var sites = accessor.QuerySites();

            Assert.NotNull(sites, "The sites return data is null");
            Assert.Greater(sites.Count, 0, "No sites returned at all");
        }

        /// <summary>
        /// Test for querying of monitored hosts (implicitly testing the provider).
        /// </summary>
        [Test]
        public void QueryMonitoredHostsTest()
        {
            var accessor = new JsonHamnetDbAccessor(TestConstants.HostsUrl, TestConstants.SubnetsUrl, TestConstants.SitesUrl, null);

            Assert.NotNull(accessor, "The accessor returned by provider is null");

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
            var accessor = new JsonHamnetDbAccessor(TestConstants.HostsUrl, TestConstants.SubnetsUrl, TestConstants.SitesUrl, null);

            Assert.NotNull(accessor, "The accessor returned by provider is null");

            var subnets = accessor.QuerySubnets();

            var wantedSubnet = IPNetwork.Parse("44.148.46.48/29");
            var ciriticalNet = subnets.FirstOrDefault(s => s.Subnet == wantedSubnet);

            Assert.NotNull(subnets, "The subnets return data is null");
            Assert.Greater(subnets.Count, 0, "No subnets returned at all");
        }
    }
}
