using System;
using NUnit.Framework;
using SnmpAbstraction;
using SnmpSharpNet;

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
        /// Test for querying of system data of Ubiquiti devices.
        /// </summary>
        [Test]
        public void UbntQuerySystemDataTest()
        {
            QueryAndPrintSystemData(TestConstants.TestAddressUbntAirOs4side1, SnmpVersion.Ver1);
        }

        /// <summary>
        /// Test for querying of system data of MikroTik devices.
        /// </summary>
        [Test]
        public void MtikQuerySystemDataTest()
        {
            QueryAndPrintSystemData(TestConstants.TestAddressMikrotik1, SnmpVersion.Ver2);
        }

        /// <summary>
        /// Test for querying of interface data of MikroTik devices.
        /// </summary>
        [Test]
        public void MtikQueryInterfaceDataTest()
        {
            QueryAndPrintInterfaces(new IpAddress("44.224.10.109"), SnmpVersion.Ver2);
            QueryAndPrintInterfaces(TestConstants.TestAddressMikrotik1, SnmpVersion.Ver2);
        }

        /// <summary>
        /// Test for querying of interface data of MikroTik devices.
        /// </summary>
        [Test]
        public void UbntQueryInterfaceDataTest()
        {
            QueryAndPrintInterfaces(TestConstants.TestAddressUbntAirOs4side1, SnmpVersion.Ver1);
            QueryAndPrintInterfaces(TestConstants.TestAddressUbntAirOs6side1, SnmpVersion.Ver2); // Ver2 should cause a fallback to V1 for UBNT
            QueryAndPrintInterfaces(TestConstants.TestAddressUbntAirOs8side1, SnmpVersion.Ver1);
            QueryAndPrintInterfaces(TestConstants.TestAddressUbntAirFiberSide2, SnmpVersion.Ver1);
        }

        /// <summary>
        /// Test for querying of wireless peers of Ubiquiti devices.
        /// </summary>
        [Test]
        public void UbntQueryWirelessPeersTest()
        {
            QueryAndPrintWirelessPeers(TestConstants.TestAddressUbntAirOs4side1, SnmpVersion.Ver2); // Ver2 should cause a fallback to V1 for UBNT
            QueryAndPrintWirelessPeers(TestConstants.TestAddressUbntAirOs6side1, SnmpVersion.Ver1);
            QueryAndPrintWirelessPeers(TestConstants.TestAddressUbntAirOs8side1, SnmpVersion.Ver1);
            QueryAndPrintWirelessPeers(TestConstants.TestAddressUbntAirFiberSide1, SnmpVersion.Ver1);
        }

        /// <summary>
        /// Test for querying of wireless peers of MikroTik devices.
        /// </summary>
        [Test]
        public void MtikQueryWirelessPeersTest()
        {
            QueryAndPrintWirelessPeers(new IpAddress("44.224.10.106"), SnmpVersion.Ver2);
            QueryAndPrintWirelessPeers(TestConstants.TestAddressMikrotik1, SnmpVersion.Ver2);
        }

        /// <summary>
        /// Test for querying link details.
        /// </summary>
        [Test]
        public void MtikFetchLinkDetailsTest()
        {
            QueryAndPrintLinkDetails(new IpAddress("44.224.10.106"), new IpAddress("44.224.10.109"), SnmpVersion.Ver2);
            QueryAndPrintLinkDetails(TestConstants.TestAddressMikrotik1, TestConstants.TestAddressMikrotik2, SnmpVersion.Ver2);
            QueryAndPrintLinkDetails(TestConstants.TestAddressMikrotik4, TestConstants.TestAddressMikrotik3, SnmpVersion.Ver2);
        }

        /// <summary>
        /// Test for querying link details.
        /// </summary>
        [Test]
        public void UbntFetchLinkDetailsTest()
        {
            QueryAndPrintLinkDetails(TestConstants.TestAddressUbntAirOs4side1, TestConstants.TestAddressUbntAirOs4side2, SnmpVersion.Ver1);
            QueryAndPrintLinkDetails(TestConstants.TestAddressUbntAirOs6side1, TestConstants.TestAddressUbntAirOs6side2, SnmpVersion.Ver1);
            QueryAndPrintLinkDetails(TestConstants.TestAddressUbntAirOs8side1, TestConstants.TestAddressUbntAirOs8side2, SnmpVersion.Ver1);
            QueryAndPrintLinkDetails(TestConstants.TestAddressUbntAirFiberSide1, TestConstants.TestAddressUbntAirFiberSide2, SnmpVersion.Ver1);
        }

        /// <summary>
        /// Performs procedure for *QuerySystemData tests.
        /// </summary>
        /// <param name="address1">The address of side #1 to test with.</param>
        /// <param name="address2">The address of side #2 to test with.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use.</param>
        private static void QueryAndPrintLinkDetails(IpAddress address1, IpAddress address2, SnmpVersion snmpVersion)
        {
            var querier = SnmpQuerierFactory.Instance.Create(address1.ToString(), QuerierOptions.Default.WithProtocolVersion(snmpVersion));

            Assert.NotNull(querier, "Create(...) returned null");

            var linkDetails = querier.FetchLinkDetails(address2.ToString());

            Assert.NotNull(linkDetails, "querier.FetchLinkDetails returned null");

            Assert.NotNull(linkDetails.Details, "querier.FetchLinkDetails(...).Details returned null");
            Assert.Greater(linkDetails.Details.Count, 0, "querier.FetchLinkDetails(...).Details.Count == 0");

            Console.WriteLine($"=== Link details from {address1} to {address2} ===");
            Console.WriteLine(linkDetails);
        }

        /// <summary>
        /// Performs procedure for *QuerySystemData tests.
        /// </summary>
        /// <param name="address">The address to test with.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use.</param>
        private static void QueryAndPrintSystemData(IpAddress address, SnmpVersion snmpVersion)
        {
            var querier = SnmpQuerierFactory.Instance.Create(address.ToString(), QuerierOptions.Default.WithProtocolVersion(snmpVersion));

            Assert.NotNull(querier, "Create(...) returned null");

            var systemData = querier.SystemData;

            Assert.NotNull(systemData, "querier.SystemData returned null");

            systemData.ForceEvaluateAll();

            Console.WriteLine("Obtained system data:");
            Console.WriteLine(systemData);
        }

        /// <summary>
        /// Performs procedure for *QueryWirelessPeers tests.
        /// </summary>
        /// <param name="address">The address to test with.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use.</param>
        private static void QueryAndPrintWirelessPeers(IpAddress address, SnmpVersion snmpVersion)
        {
            var querier = SnmpQuerierFactory.Instance.Create(address.ToString(), QuerierOptions.Default.WithProtocolVersion(snmpVersion));

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
        /// Performs procedure for *QueryInterfaceData tests.
        /// </summary>
        /// <param name="address">The address to test with.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use.</param>
        private static void QueryAndPrintInterfaces(IpAddress address, SnmpVersion snmpVersion)
        {
            var querier = SnmpQuerierFactory.Instance.Create(address.ToString(), QuerierOptions.Default.WithProtocolVersion(snmpVersion));

            Assert.NotNull(querier, "Create(...) returned null");

            var networkInterfaceDetails = querier.NetworkInterfaceDetails;

            Assert.NotNull(networkInterfaceDetails, "querier.NetworkInterfaceDetails returned null");

            networkInterfaceDetails.ForceEvaluateAll();

            Assert.NotNull(networkInterfaceDetails.Details, "querier.NetworkInterfaceDetails.Details returned null");
            Assert.Greater(networkInterfaceDetails.Details.Count, 0, "querier.NetworkInterfaceDetails.Details.Count == 0");

            Console.WriteLine("Obtained interface details:");
            Console.WriteLine(networkInterfaceDetails);
        }
    }
}
