using System;
using NUnit.Framework;
using SnmpAbstraction;

namespace SnmpAbstractionTests
{
    /// <summary>
    /// Tests for SnmpQuerier
    /// </summary>
    public class SnmpQuerierTests
    {
        /// <summary>
        /// The test setup method.
        /// </summary>
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Test for querying of system data.
        /// </summary>
        [Test]
        public void QuerySystemDataTest()
        {
            var querier = SnmpQuerierFactory.Instance.Create(TestConstants.TestAddress1.ToString());

            Assert.NotNull(querier, "Create(...) returned null");

            var systemData = querier.SystemData;

            Assert.NotNull(systemData, "querier.SystemData returned null");

            systemData.ForceEvaluateAll();

            Console.WriteLine("Obtained system data:");
            Console.WriteLine(systemData);
        }

        /// <summary>
        /// Test for querying of interface data.
        /// </summary>
        [Test]
        public void QueryInterfaceDataTest()
        {
            var querier = SnmpQuerierFactory.Instance.Create(TestConstants.TestAddress1.ToString());

            Assert.NotNull(querier, "Create(...) returned null");

            var networkInterfaceDetails = querier.NetworkInterfaceDetails;

            Assert.NotNull(networkInterfaceDetails, "querier.NetworkInterfaceDetails returned null");

            networkInterfaceDetails.ForceEvaluateAll();

            Assert.NotNull(networkInterfaceDetails.Details, "querier.NetworkInterfaceDetails.Details returned null");
            Assert.Greater(networkInterfaceDetails.Details.Count, 0, "querier.NetworkInterfaceDetails.Details.Count == 0");

            Console.WriteLine("Obtained interface details:");
            Console.WriteLine(networkInterfaceDetails);
        }

        /// <summary>
        /// Test for querying of wireless peers.
        /// </summary>
        [Test]
        public void QueryWirelessPeersTest()
        {
            var querier = SnmpQuerierFactory.Instance.Create(TestConstants.TestAddress1.ToString());

            Assert.NotNull(querier, "Create(...) returned null");

            var wirelessPeerInfos = querier.WirelessPeerInfos;

            Assert.NotNull(wirelessPeerInfos, "querier.WirelessPeerInfos returned null");

            wirelessPeerInfos.ForceEvaluateAll();

            Assert.NotNull(wirelessPeerInfos.Details, "querier.WirelessPeerInfos.Details returned null");
            Assert.Greater(wirelessPeerInfos.Details.Count, 0, "querier.WirelessPeerInfos.Details.Count == 0");

            Console.WriteLine("Obtained peer infos:");
            Console.WriteLine(wirelessPeerInfos);
        }

        /// <summary>
        /// Test for querying link details.
        /// </summary>
        [Test]
        public void FetchLinkDetailsTest()
        {
            var querier = SnmpQuerierFactory.Instance.Create(TestConstants.TestAddress1.ToString());

            Assert.NotNull(querier, "Create(...) returned null");

            var linkDetails = querier.FetchLinkDetails(TestConstants.TestAddress2.ToString());

            Assert.NotNull(linkDetails, "querier.FetchLinkDetails returned null");

            Assert.NotNull(linkDetails.Details, "querier.FetchLinkDetails(...).Details returned null");
            Assert.Greater(linkDetails.Details.Count, 0, "querier.FetchLinkDetails(...).Details.Count == 0");

            Console.WriteLine("Obtained link details:");
            Console.WriteLine(linkDetails);
        }
    }
}
