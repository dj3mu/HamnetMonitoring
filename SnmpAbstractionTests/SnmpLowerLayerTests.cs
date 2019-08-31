using NUnit.Framework;
using SnmpAbstraction;
using SnmpSharpNet;
using System;

namespace Tests
{
    public class SnmpLowerLayerTests
    {
        /// <summary>
        /// The address to use for the tests.
        /// </summary>
        private IpAddress testAddress;

        /// <summary>
        /// Address of localhost.
        /// </summary>
        private IpAddress localhostAdddress;

        [SetUp]
        public void Setup()
        {
            // The address put here is one of the author's private Hamnet client hardware
            // Feel free to use it for first tests. But please consider switching to one of your
            // own devices when doing more extensive testind and/or development.
            this.testAddress = new IpAddress("44.225.23.222");
            this.localhostAdddress = new IpAddress("127.0.0.1");
        }

        /// <summary>
        /// Test for construction (successfull and with invalid params).
        /// </summary>
        [Test]
        public void ConstructionTest()
        {
            // no options c'tor
            using(var snmpll = new SnmpLowerLayer(this.testAddress))
            {
                Assert.NotNull(snmpll, "Instantiated SnmpLowerLayer(address, null) is null");
            }

            // c'tor with options
            using (var snmpll = new SnmpLowerLayer(this.testAddress, QuerierOptions.Default))
            {
                Assert.NotNull(snmpll, "Instantiated SnmpLowerLayer(address, options) is null");
            }

            // null for address must throw
            Assert.Throws<ArgumentNullException>(
                () => { var snmpll = new SnmpLowerLayer(null); },
                "address to talk to is null"
            );
        }

        /// <summary>
        /// Test for successful query operations
        /// </summary>
        [Test]
        public void QuerySuccessTest()
        {
            Oid testOid = new Oid("1.3.6.1.2.1.1.1.0"); 

            // test a (hopefully) successful query.
            // THIS CAN FAIL IF THE DEVICE HOLDING THE address specified by "testAddress"
            // is not available or has no SNMP service running.
            using(var snmpll = new SnmpLowerLayer(this.testAddress))
            {
                VbCollection result = snmpll.Query(new Oid("1.3.6.1.2.1.1.1.0"));
                Assert.NotNull(result, "The query result is null");
                Assert.GreaterOrEqual(result.Count, 1, "Empty result list when querying 1 OID");

                Assert.NotNull(result[0], "result[0] is null");
                Assert.AreEqual(testOid, result[0].Oid, "result[0] is of wrong OID");
                
                Console.WriteLine($"Result for OID {result[0].Oid} is of type '{SnmpConstants.GetTypeName(result[0].Value.Type)}' with value '{result[0].Value}'");
            }
        }

        /// <summary>
        /// Test for failing query operations
        /// </summary>
        [Test]
        public void QueryFailTest()
        {
            Oid testOid = new Oid("1.3.6.1.2.1.1.1.0"); 

            // Query to a host that has (hopefully) no SNMP service running on it.
            using(var snmpll = new SnmpLowerLayer(this.localhostAdddress))
            {
                Assert.Throws<SnmpNetworkException>(
                    () => { VbCollection result = snmpll.Query(testOid); },
                    "Network error: connection reset by peer."
                );
            }
        }

        /// <summary>
        /// Test for successful query operations
        /// </summary>
        [Test]
        public void QueryAsStringTest()
        {
            Oid testOid = new Oid("1.3.6.1.2.1.1.1.0"); 

            // test a (hopefully) successful query.
            // THIS CAN FAIL IF THE DEVICE HOLDING THE address specified by "testAddress"
            // is not available or has no SNMP service running.
            using(var snmpll = new SnmpLowerLayer(this.testAddress))
            {
                string result = snmpll.QueryAsString(testOid, "test query");
                Assert.NotNull(result, "The query result is null");
                Assert.IsTrue(!string.IsNullOrEmpty(result), "result queries as string is null or empty");

                Console.WriteLine($"Result queried from '{snmpll.Address}' as OID '{testOid}' as string '{result}'");
            }

            // Query to a host that has (hopefully) no SNMP service running on it.
            using(var snmpll = new SnmpLowerLayer(this.localhostAdddress))
            {
                string result = snmpll.QueryAsString(testOid, "test query");
                Assert.IsNull(result, $"Result queried from non-existing '{snmpll.Address}' as OID '{testOid}' as string is not null but '{result}'");
            }
        }
    }
}
