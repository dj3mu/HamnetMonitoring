﻿using System.Text;
using NUnit.Framework;
using SnmpAbstraction;
using tik4net;
using tik4net.Objects.Tool;

namespace SnmpAbstractionTests
{
    /// <summary>
    /// Tests for all kinds of extension methods (I don't want to scatter the tests too much).
    /// </summary>
    public partial class Tik4netManualIntegration
    {
        /// <summary>
        /// The test setup method.
        /// </summary>
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// A test for hex-string to string conversion (the hex bytes are actually ASCII/UTF-8 codes)
        /// </summary>
        [Test]
        public void HexStringTest()
        {
            var input = "4D F6 6E 63 68 65 6E 67 6C 61 64 62 61 63 68";

            bool couldBeHexString = true;
            for (int i = 2; i < input.Length; i+=3)
            {
                if (input[i] != ' ')
                {
                    couldBeHexString = false;
                    break;
                }
            }

            if (couldBeHexString)
            {
                input = Encoding.UTF8.GetString(input.HexStringToByteArray(' '));
            }
        }

        /// <summary>
        /// Test for using ITikConnection
        /// </summary>
        [Test]
        public void TikConnectionTest()
        {
            using ITikConnection connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open("44.224.10.78", "monitoring", "");
            var traceroute = connection.HamnetTraceroute("44.137.62.114");
            //var advertisements = connection.LoadAll<BgpAdvertisement>();
            //var instances = connection.LoadAll<BgpInstance>();
            //var peers = connection.LoadAll<BgpPeer>();
            //var networks = connection.LoadAll<BgpNetwork>();
            //var routes = connection.LoadAll<IpRoute>();
        }
    }
}
