using NUnit.Framework;
using SnmpAbstraction;
using SnmpSharpNet;
using System;

namespace SnmpAbstractionTests
{
    public class SnmpLowerLayerTests
    {
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Test for construction (successfull and with invalid params).
        /// </summary>
        [Test]
        public void ConstructionTest()
        {
            // no options c'tor
            using(var snmpll = new SnmpLowerLayer(TestConstants.TestAddress))
            {
                Assert.NotNull(snmpll, "Instantiated SnmpLowerLayer(address, null) is null");
            }

            // c'tor with options
            using (var snmpll = new SnmpLowerLayer(TestConstants.TestAddress, QuerierOptions.Default))
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
            using(var snmpll = new SnmpLowerLayer(TestConstants.TestAddress))
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
            using(var snmpll = new SnmpLowerLayer(TestConstants.LocalhostAdddress))
            {
                Assert.Throws<SnmpNetworkException>(
                    () => { VbCollection result = snmpll.Query(testOid); },
                    "Network error: connection reset by peer."
                );
            }
        }

        /// <summary>
        /// Test for QueryAsString extension method
        /// </summary>
        [Test]
        public void QueryAsStringTest()
        {
            // system name should be supported by most devices
            Oid testOid = new Oid("1.3.6.1.2.1.1.1.0"); 

            // test a (hopefully) successful query.
            // THIS CAN FAIL IF THE DEVICE HOLDING THE address specified by "testAddress"
            // is not available or has no SNMP service running.
            using(var snmpll = new SnmpLowerLayer(TestConstants.TestAddress))
            {
                string result = snmpll.QueryAsString(testOid, "test query");
                Assert.NotNull(result, "The query result is null");
                Assert.IsTrue(!string.IsNullOrEmpty(result), "result queries as string is null or empty");

                Console.WriteLine($"Result queried from '{snmpll.Address}' as OID '{testOid}' as string '{result}'");
            }

            // Query to a host that has (hopefully) no SNMP service running on it.
            using(var snmpll = new SnmpLowerLayer(TestConstants.LocalhostAdddress))
            {
                string result = snmpll.QueryAsString(testOid, "test query");
                Assert.IsNull(result, $"Result queried from non-existing '{snmpll.Address}' as OID '{testOid}' as string is not null but '{result}'");
            }
        }

        /// <summary>
        /// Test for QueryAsOid extension method
        /// </summary>
        [Test]
        public void QueryAsOidTest()
        {
            // system enterprise OID should be supported by most devices
            Oid testOid = new Oid(".1.3.6.1.2.1.1.2.0"); 

            // test a (hopefully) successful query.
            // THIS CAN FAIL IF THE DEVICE HOLDING THE address specified by "testAddress"
            // is not available or has no SNMP service running.
            using(var snmpll = new SnmpLowerLayer(TestConstants.TestAddress))
            {
                Oid result = snmpll.QueryAsOid(testOid, "test query");
                Assert.NotNull(result, "The query result is null");

                Console.WriteLine($"Result queried from '{snmpll.Address}' as OID '{testOid}' as OID '{result}'");
            }

            // Query to a host that has (hopefully) no SNMP service running on it.
            using(var snmpll = new SnmpLowerLayer(TestConstants.LocalhostAdddress))
            {
                Oid result = snmpll.QueryAsOid(testOid, "test query");
                Assert.IsNull(result, $"Result queried from non-existing '{snmpll.Address}' as OID '{testOid}' as string is not null but '{result}'");
            }
        }

        /// <summary>
        /// Test for QueryAsString extension method
        /// </summary>
        [Test]
        public void QueryAsTimeTicksTest()
        {
            // system uptime should be supported by most devices
            Oid testOid = new Oid(".1.3.6.1.2.1.1.3.0");

            // test a (hopefully) successful query.
            // THIS CAN FAIL IF THE DEVICE HOLDING THE address specified by "testAddress"
            // is not available or has no SNMP service running.
            using(var snmpll = new SnmpLowerLayer(TestConstants.TestAddress))
            {
                TimeTicks result = snmpll.QueryAsTimeTicks(testOid, "test query");
                Assert.NotNull(result, "The query result is null");

                Console.WriteLine($"Result queried from '{snmpll.Address}' as OID '{testOid}' as OID '{result}'");
            }

            // Query to a host that has (hopefully) no SNMP service running on it.
            using(var snmpll = new SnmpLowerLayer(TestConstants.LocalhostAdddress))
            {
                Oid result = snmpll.QueryAsOid(testOid, "test query");
                Assert.IsNull(result, $"Result queried from non-existing '{snmpll.Address}' as OID '{testOid}' as string is not null but '{result}'");
            }
        }

        /// <summary>
        /// Test for successful query operations
        /// </summary>
        [Test]
        public void QuerySystemDataTest()
        {
            // test a (hopefully) successful query.
            // THIS CAN FAIL IF THE DEVICE HOLDING THE address specified by "testAddress"
            // is not available or has no SNMP service running.
            using(var snmpll = new SnmpLowerLayer(TestConstants.TestAddress))
            {
                IDeviceSystemData systemData = snmpll.SystemData;
                Assert.NotNull(systemData, "The system data is null");

                Assert.IsNotEmpty(systemData.Name, "system name is empty");
                Assert.IsNotEmpty(systemData.Contact, "system contact is empty");
                Assert.IsNotEmpty(systemData.Location, "system location is empty");
                Assert.IsNotEmpty(systemData.Description, "system description is empty");
                Assert.IsNotNull(systemData.Uptime, "system uptime is null");
                Assert.IsNotNull(systemData.EnterpriseObjectId, "system enterprise OID is null");

                Console.WriteLine($"{Environment.NewLine}{systemData}");
            }
        }
    }
}
