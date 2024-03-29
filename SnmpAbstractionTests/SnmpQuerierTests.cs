﻿using System;
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
            QueryAndPrintSystemData(new IpAddress("44.148.64.125"), SnmpVersion.Ver1, false);
            //QueryAndPrintSystemData(TestConstants.TestAddressUbntAirOs6side1, SnmpVersion.Ver1, true);
            //QueryAndPrintSystemData(TestConstants.TestAddressUbntAirOs6side1, SnmpVersion.Ver1, true);
        }

        /// <summary>
        /// Test for querying of system data of Ubiquiti devices.
        /// </summary>
        [Test]
        public void AlixQuerySystemDataTest()
        {
            QueryAndPrintSystemData(TestConstants.TestAddressAlix1, SnmpVersion.Ver2, false);
            QueryAndPrintSystemData(TestConstants.TestAddressAlix1, SnmpVersion.Ver2, true);
            QueryAndPrintSystemData(TestConstants.TestAddressAlix1, SnmpVersion.Ver2, true);

            QueryAndPrintSystemData(new IpAddress("44.224.90.30"), SnmpVersion.Ver2, true);
        }

        /// <summary>
        /// Test for querying of system data of MikroTik devices.
        /// </summary>
        [Test]
        public void MtikQuerySystemDataTest()
        {
            QueryAndPrintSystemData(TestConstants.TestAddressMikrotik1, SnmpVersion.Ver2, false);
            QueryAndPrintSystemData(TestConstants.TestAddressMikrotik1, SnmpVersion.Ver2, true);
            QueryAndPrintSystemData(TestConstants.TestAddressMikrotik1, SnmpVersion.Ver2, true);
        }

        /// <summary>
        /// Test for querying of interface data of MikroTik devices.
        /// </summary>
        [Test]
        public void AlixQueryInterfaceDataTest()
        {
            QueryAndPrintInterfaces(TestConstants.TestAddressAlix1, SnmpVersion.Ver2, false);
            QueryAndPrintInterfaces(TestConstants.TestAddressAlix1, SnmpVersion.Ver2, true);
            QueryAndPrintInterfaces(TestConstants.TestAddressMikrotik1, SnmpVersion.Ver2, true);
        }

        /// <summary>
        /// Test for querying of interface data of MikroTik devices.
        /// </summary>
        [Test]
        public void MtikQueryInterfaceDataTest()
        {
            QueryAndPrintInterfaces(TestConstants.TestAddressMikrotik2, SnmpVersion.Ver2, false, QueryApis.VendorSpecific);
            QueryAndPrintInterfaces(TestConstants.TestAddressMikrotik2, SnmpVersion.Ver2, false, QueryApis.Snmp);
            QueryAndPrintInterfaces(TestConstants.TestAddressMikrotik1, SnmpVersion.Ver2, true);
            QueryAndPrintInterfaces(TestConstants.TestAddressMikrotik1, SnmpVersion.Ver2, true);
        }

        /// <summary>
        /// Test for querying of interface data of MikroTik devices.
        /// </summary>
        [Test]
        public void UbntQueryInterfaceDataTest()
        {
            QueryAndPrintInterfaces(new IpAddress("44.148.68.21"), SnmpVersion.Ver1, false);
            //QueryAndPrintInterfaces(TestConstants.TestAddressUbntAirOs6side1, SnmpVersion.Ver2, false); // Ver2 should cause a fallback to V1 for UBNT
            //QueryAndPrintInterfaces(TestConstants.TestAddressUbntAirOs8side1, SnmpVersion.Ver1, false);
            //QueryAndPrintInterfaces(TestConstants.TestAddressUbntAirFiberSide2, SnmpVersion.Ver1, false);
        }

        /// <summary>
        /// Test for querying of wireless peers of Ubiquiti devices.
        /// </summary>
        [Test]
        public void UbntQueryWirelessPeersTest()
        {
            QueryAndPrintWirelessPeers(new IpAddress("44.148.68.21"), SnmpVersion.Ver1, true); // Ver2 should cause a fallback to V1 for UBNT
            //QueryAndPrintWirelessPeers(TestConstants.TestAddressUbntAirOs6side1, SnmpVersion.Ver1, false);
            //QueryAndPrintWirelessPeers(TestConstants.TestAddressUbntAirOs8side1, SnmpVersion.Ver1, false);
            //QueryAndPrintWirelessPeers(TestConstants.TestAddressUbntAirFiberSide1, SnmpVersion.Ver1, false);
            //QueryAndPrintWirelessPeers(new IpAddress("44.142.42.210"), SnmpVersion.Ver2, false); // HB9RF
        }

        /// <summary>
        /// Test for querying of wireless peers of MikroTik devices.
        /// </summary>
        [Test]
        public void AlixQueryWirelessPeersTest()
        {
            QueryAndPrintWirelessPeers(TestConstants.TestAddressAlix1, SnmpVersion.Ver2, false);
            QueryAndPrintWirelessPeers(TestConstants.TestAddressAlix1, SnmpVersion.Ver2, true);
            QueryAndPrintWirelessPeers(TestConstants.TestAddressAlix1, SnmpVersion.Ver2, true);
        }

        /// <summary>
        /// Test for querying of wireless peers of MikroTik devices.
        /// </summary>
        [Test]
        public void MtikQueryWirelessPeersTest()
        {
            QueryAndPrintWirelessPeers(TestConstants.TestAddressMikrotik2, SnmpVersion.Ver2, false, QueryApis.VendorSpecific);
            QueryAndPrintWirelessPeers(TestConstants.TestAddressMikrotik2, SnmpVersion.Ver2, false, QueryApis.Snmp);
            QueryAndPrintWirelessPeers(TestConstants.TestAddressMikrotik1, SnmpVersion.Ver2, false, QueryApis.VendorSpecific);
            QueryAndPrintWirelessPeers(TestConstants.TestAddressMikrotik1, SnmpVersion.Ver2, false, QueryApis.Snmp);
        }

        /// <summary>
        /// Test for querying link details.
        /// </summary>
        [Test]
        public void AlixFetchLinkDetailsTest()
        {
            QueryAndPrintLinkDetails(TestConstants.TestAddressAlix1, TestConstants.TestAddressAlix2, SnmpVersion.Ver2, false);
        }

        /// <summary>
        /// Test for querying link details.
        /// </summary>
        [Test]
        public void MtikFetchLinkDetailsTest()
        {
            QueryAndPrintLinkDetails(TestConstants.TestAddressMikrotik1, TestConstants.TestAddressMikrotik2, SnmpVersion.Ver2, false);
            QueryAndPrintLinkDetails(TestConstants.TestAddressMikrotik1, TestConstants.TestAddressMikrotik2, SnmpVersion.Ver2, true);
            QueryAndPrintLinkDetails(TestConstants.TestAddressMikrotik1, TestConstants.TestAddressMikrotik2, SnmpVersion.Ver2, true);

            QueryAndPrintLinkDetails(TestConstants.TestAddressMikrotik4, TestConstants.TestAddressMikrotik3, SnmpVersion.Ver2, false);
            QueryAndPrintLinkDetails(TestConstants.TestAddressMikrotik4, TestConstants.TestAddressMikrotik3, SnmpVersion.Ver2, true);
            QueryAndPrintLinkDetails(TestConstants.TestAddressMikrotik4, TestConstants.TestAddressMikrotik3, SnmpVersion.Ver2, true);
        }

        /// <summary>
        /// Test for querying link details.
        /// </summary>
        [Test]
        public void UbntFetchLinkDetailsTest()
        {
            QueryAndPrintLinkDetails(new IpAddress("44.148.68.18"), new IpAddress("44.148.68.21"), SnmpVersion.Ver1, false);
            //QueryAndPrintLinkDetails(TestConstants.TestAddressUbntAirOs6side1, TestConstants.TestAddressUbntAirOs6side2, SnmpVersion.Ver1);
            //QueryAndPrintLinkDetails(TestConstants.TestAddressUbntAirOs8side1, TestConstants.TestAddressUbntAirOs8side2, SnmpVersion.Ver1);
            //QueryAndPrintLinkDetails(TestConstants.TestAddressUbntAirFiberSide1, TestConstants.TestAddressUbntAirFiberSide2, SnmpVersion.Ver1);
        }

        /// <summary>
        /// Test for querying of BGP peers of Mikrotik devices.
        /// </summary>
        [Test]
        public void MtikQueryBgpPeersTest()
        {
            //QueryAndPrintBgpPeers(new IpAddress("44.225.20.1"), SnmpVersion.Ver2, false);
            QueryAndPrintBgpPeers(TestConstants.TestAddressMikrotikRouter1, SnmpVersion.Ver2, false);
        }

        /// <summary>
        /// Test for querying of BGP peers of Mikrotik devices.
        /// </summary>
        [Test]
        public void MtikTracerouteTest()
        {
            QueryAndPrintTraceroute(new IpAddress("44.148.90.122"), "44.148.90.125", SnmpVersion.Ver1, false);
        }

        /// <summary>
        /// Test for querying of wireless peers of Ubiquiti devices.
        /// </summary>
        [Test]
        public void AllForSingleIpTest()
        {
            var ipUnderTest = new IpAddress("44.148.64.141");
            var snmpVersion = SnmpVersion.Ver1;
            var useCache = false;

            Console.WriteLine($"{Environment.NewLine}{ipUnderTest} SYSTEM DATA:");
            QueryAndPrintSystemData(ipUnderTest, snmpVersion, useCache);

            Console.WriteLine($"{Environment.NewLine}{ipUnderTest} INTERFACES:");
            QueryAndPrintInterfaces(ipUnderTest, snmpVersion, useCache);

            Console.WriteLine($"{Environment.NewLine}{ipUnderTest} WIRELESS PEERS:");
            QueryAndPrintWirelessPeers(ipUnderTest, snmpVersion, useCache);
        }

        /// <summary>
        /// Test for querying of wireless peers of Ubiquiti devices.
        /// </summary>
        [Test]
        public void IpLinkTestTest()
        {
            var ip1UnderTest = new IpAddress("44.148.64.125");
            var ip2UnderTest = new IpAddress("44.148.64.122");
            // var ip1UnderTest = new IpAddress("44.148.10.74");
            // var ip2UnderTest = new IpAddress("44.148.10.77");
            var snmpVersion = SnmpVersion.Ver1;
            var useCache = false;

            Console.WriteLine($"{Environment.NewLine}{ip1UnderTest} (side #1) SYSTEM DATA:");
            QueryAndPrintSystemData(ip1UnderTest, snmpVersion, useCache);
            Console.WriteLine($"{Environment.NewLine}{ip2UnderTest} (side #2) SYSTEM DATA:");
            QueryAndPrintSystemData(ip2UnderTest, snmpVersion, useCache);

            Console.WriteLine($"{Environment.NewLine}{ip1UnderTest} (side #1) INTERFACES:");
            QueryAndPrintInterfaces(ip1UnderTest, snmpVersion, useCache);
            Console.WriteLine($"{Environment.NewLine}{ip2UnderTest} (side #2) INTERFACES:");
            QueryAndPrintInterfaces(ip2UnderTest, snmpVersion, useCache);

            Console.WriteLine($"{Environment.NewLine}{ip1UnderTest} (side #1) WIRELESS PEERS:");
            QueryAndPrintWirelessPeers(ip1UnderTest, snmpVersion, useCache);
            Console.WriteLine($"{Environment.NewLine}{ip2UnderTest} (side #2) WIRELESS PEERS:");
            QueryAndPrintWirelessPeers(ip2UnderTest, snmpVersion, useCache);

            Console.WriteLine($"{Environment.NewLine}{ip1UnderTest} (side #1) <-> {ip2UnderTest} (side #2) LINK DETAILS:");
            QueryAndPrintLinkDetails(ip1UnderTest, ip2UnderTest, snmpVersion, useCache);
        }

        /// <summary>
        /// Performs procedure for traceroute tests.
        /// </summary>
        /// <param name="address">The address to test with.</param>
        /// <param name="target">The traceroute target host name or IP address.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use.</param>
        /// <param name="useCache">Value indicating whether to use caching of non-volatile data.</param>
        /// <param name="allowedApis">The list of allowed APIs</param>
        private static void QueryAndPrintTraceroute(IpAddress address, string target, SnmpVersion snmpVersion, bool useCache = false, QueryApis allowedApis = QueryApis.VendorSpecific)
        {
            using var querier = SnmpQuerierFactory.Instance.Create(address.ToString(), QuerierOptions.Default.WithProtocolVersion(snmpVersion).WithCaching(useCache).WithAllowedApis(allowedApis));

            var systemData = querier.SystemData;
            systemData.ForceEvaluateAll();

            Assert.NotNull(querier, "Create(...) returned null");

            var traceroute = querier.Traceroute(target, 1, TimeSpan.FromSeconds(1), 500);

            Assert.NotNull(traceroute, "querier.BgpPeers returned null");

            Console.WriteLine("Obtained route trace:");
            Console.WriteLine(new BlockTextFormatter().Format(traceroute));
        }

        /// <summary>
        /// Performs procedure for BGP tests.
        /// </summary>
        /// <param name="address">The address to test with.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use.</param>
        /// <param name="useCache">Value indicating whether to use caching of non-volatile data.</param>
        /// <param name="allowedApis">The list of allowed APIs</param>
        private static void QueryAndPrintBgpPeers(IpAddress address, SnmpVersion snmpVersion, bool useCache = false, QueryApis allowedApis = QueryApis.VendorSpecific)
        {
            using var querier = SnmpQuerierFactory.Instance.Create(address.ToString(), QuerierOptions.Default.WithProtocolVersion(snmpVersion).WithCaching(useCache).WithAllowedApis(allowedApis));

            var systemData = querier.SystemData;
            systemData.ForceEvaluateAll();

            Assert.NotNull(querier, "Create(...) returned null");

            var bgpPeers = querier.FetchBgpPeers(null);

            Assert.NotNull(bgpPeers, "querier.BgpPeers returned null");

            bgpPeers.ForceEvaluateAll();

            Console.WriteLine("Obtained BGP peers:");
            Console.WriteLine(new BlockTextFormatter().Format(bgpPeers));
        }

        /// <summary>
        /// Performs procedure for *QuerySystemData tests.
        /// </summary>
        /// <param name="address1">The address of side #1 to test with.</param>
        /// <param name="address2">The address of side #2 to test with.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use.</param>
        /// <param name="useCache">Value indicating whether to use caching of non-volatile data.</param>
        /// <param name="allowedApis">The list of allowed APIs</param>
        private static void QueryAndPrintLinkDetails(IpAddress address1, IpAddress address2, SnmpVersion snmpVersion, bool useCache = false, QueryApis allowedApis = QueryApis.Snmp)
        {
            using var querier = SnmpQuerierFactory.Instance.Create(address1.ToString(), QuerierOptions.Default.WithProtocolVersion(snmpVersion).WithCaching(useCache).WithAllowedApis(allowedApis));

            Assert.NotNull(querier, "Create(...) returned null");

            var linkDetails = querier.FetchLinkDetails(address2.ToString());

            Assert.NotNull(linkDetails, "querier.FetchLinkDetails returned null");

            Assert.NotNull(linkDetails.Details, "querier.FetchLinkDetails(...).Details returned null");
            Assert.Greater(linkDetails.Details.Count, 0, "querier.FetchLinkDetails(...).Details.Count == 0");

            Console.WriteLine($"=== Link details from {address1} to {address2} ===");
            Console.WriteLine(new BlockTextFormatter().Format(linkDetails));
        }

        /// <summary>
        /// Performs procedure for *QuerySystemData tests.
        /// </summary>
        /// <param name="address">The address to test with.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use.</param>
        /// <param name="useCache">Value indicating whether to use caching of non-volatile data.</param>
        /// <param name="allowedApis">The list of allowed APIs</param>
        private static void QueryAndPrintSystemData(IpAddress address, SnmpVersion snmpVersion, bool useCache = false, QueryApis allowedApis = QueryApis.Snmp)
        {
            using var querier = SnmpQuerierFactory.Instance.Create(address.ToString(), QuerierOptions.Default.WithProtocolVersion(snmpVersion).WithCaching(useCache).WithAllowedApis(allowedApis));

            Assert.NotNull(querier, "Create(...) returned null");

            var systemData = querier.SystemData;

            Assert.NotNull(systemData, "querier.SystemData returned null");

            systemData.ForceEvaluateAll();

            Console.WriteLine("Obtained system data:");
            Console.WriteLine(new BlockTextFormatter().Format(systemData));
        }

        /// <summary>
        /// Performs procedure for *QueryWirelessPeers tests.
        /// </summary>
        /// <param name="address">The address to test with.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use.</param>
        /// <param name="useCache">Value indicating whether to use caching of non-volatile data.</param>
        /// <param name="allowedApis">The list of allowed APIs</param>
        private static void QueryAndPrintWirelessPeers(IpAddress address, SnmpVersion snmpVersion, bool useCache = false, QueryApis allowedApis = QueryApis.Snmp)
        {
            using var querier = SnmpQuerierFactory.Instance.Create(address.ToString(), QuerierOptions.Default.WithProtocolVersion(snmpVersion).WithCaching(useCache).WithAllowedApis(allowedApis));

            Assert.NotNull(querier, "Create(...) returned null");

            var wirelessPeerInfos = querier.WirelessPeerInfos;

            Assert.NotNull(wirelessPeerInfos, "querier.WirelessPeerInfos returned null");

            wirelessPeerInfos.ForceEvaluateAll();

            Assert.NotNull(wirelessPeerInfos.Details, "querier.WirelessPeerInfos.Details returned null");
            Assert.Greater(wirelessPeerInfos.Details.Count, 0, "querier.WirelessPeerInfos.Details.Count == 0");

            Console.WriteLine("Obtained peer infos:");
            Console.WriteLine(new BlockTextFormatter().Format(wirelessPeerInfos));
        }

        /// <summary>
        /// Performs procedure for *QueryInterfaceData tests.
        /// </summary>
        /// <param name="address">The address to test with.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use.</param>
        /// <param name="useCache">Value indicating whether to use caching of non-volatile data.</param>
        /// <param name="allowedApis">The list of allowed APIs</param>
        private static void QueryAndPrintInterfaces(IpAddress address, SnmpVersion snmpVersion, bool useCache = false, QueryApis allowedApis = QueryApis.Snmp)
        {
            using var querier = SnmpQuerierFactory.Instance.Create(address.ToString(), QuerierOptions.Default.WithProtocolVersion(snmpVersion).WithCaching(useCache).WithAllowedApis(allowedApis));

            Assert.NotNull(querier, "Create(...) returned null");

            var networkInterfaceDetails = querier.NetworkInterfaceDetails;

            Assert.NotNull(networkInterfaceDetails, "querier.NetworkInterfaceDetails returned null");

            networkInterfaceDetails.ForceEvaluateAll();

            Assert.NotNull(networkInterfaceDetails.Details, "querier.NetworkInterfaceDetails.Details returned null");
            Assert.Greater(networkInterfaceDetails.Details.Count, 0, "querier.NetworkInterfaceDetails.Details.Count == 0");

            Console.WriteLine("Obtained interface details:");
            Console.WriteLine(new BlockTextFormatter().Format(networkInterfaceDetails));
        }
    }
}
