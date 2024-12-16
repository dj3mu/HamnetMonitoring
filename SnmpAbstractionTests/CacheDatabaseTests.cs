using NUnit.Framework;
using NUnit.Framework.Legacy;
using SemVersion;
using SnmpAbstraction;
using SnmpSharpNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tests
{
    /// <summary>
    /// Tests for the cache database.
    /// </summary>
    [SingleThreaded]
    public class CacheDatabaseTests
    {
        /// <summary>
        /// The address to use for the tests.
        /// </summary>
        private readonly string database = Path.Combine("Config", "TestCacheDatabase.sqlite");

        /// <summary>
        /// The test setup method.
        /// </summary>
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Test for construction and basic usage.
        /// </summary>
        [Test]
        public void DbContextTest()
        {
            using var context = new CacheDatabaseContext(this.database);
            ClassicAssert.NotNull(context, "The database context is null");
        }

        /// <summary>
        /// Test for storing system data
        /// </summary>
        [Test]
        public void SystemDataRoundTripTest()
        {
            var testSystemData = new SerializableSystemData
            {
                Contact = "Test-Contact",
                Description = "Test-Description",
                DeviceAddress = new IpAddress("1.2.3.4"),
                DeviceModel = "Test-DeviceMode",
                EnterpriseObjectId = new Oid("5.6.7.8"),
                Location = "Test-Location",
                Model = "Test-Model",
                Name = "Test-Name",
                QueryDuration = TimeSpan.FromSeconds(2),
                Uptime = TimeSpan.FromMinutes(2),
                Version = new SemanticVersion(1, 3, 5, "p7", "b9")
            };

            using (var context = new CacheDatabaseContext(this.database))
            {
                context.Database.EnsureDeleted();

                context.Database.EnsureCreated();

                ClassicAssert.NotNull(context, "The database context is null");

                context.CacheData.Add(new CacheData
                {
                    Address = testSystemData.DeviceAddress,
                    InterfaceDetails = null,
                    WirelessPeerInfos = null,
                    SystemData = testSystemData
                });

                context.SaveChanges();
            }

            using (var context = new CacheDatabaseContext(this.database))
            {
                ClassicAssert.NotNull(context, "The database context is null");

                var retrievedSystemData = context.CacheData.First().SystemData;

                ClassicAssert.NotNull(retrievedSystemData, "Retrieved system data is null");

                ClassicAssert.AreEqual(testSystemData.Contact, retrievedSystemData.Contact, "Error in Contact");
                ClassicAssert.AreEqual(testSystemData.Description, retrievedSystemData.Description, "Error in Description");
                ClassicAssert.AreEqual(testSystemData.DeviceAddress, retrievedSystemData.DeviceAddress, "Error in DeviceAddress");
                ClassicAssert.AreEqual(testSystemData.DeviceModel, retrievedSystemData.DeviceModel, "Error in DeviceModel");
                ClassicAssert.AreEqual(testSystemData.EnterpriseObjectId, retrievedSystemData.EnterpriseObjectId, "Error in EnterpriseObjectId");
                ClassicAssert.AreEqual(testSystemData.Location, retrievedSystemData.Location, "Error in Location");
                ClassicAssert.AreEqual(testSystemData.Model, retrievedSystemData.Model, "Error in Model");
                ClassicAssert.AreEqual(testSystemData.Name, retrievedSystemData.Name, "Error in Name");
                ClassicAssert.AreNotEqual(testSystemData.QueryDuration, retrievedSystemData.QueryDuration, "Error in QueryDuration: Seems it has been serialized even though marked as ignore");
                ClassicAssert.AreNotEqual(testSystemData.Uptime, retrievedSystemData.Uptime, "Error in Uptime: Seems it has been serialized even though marked as ignore");
                ClassicAssert.AreEqual(testSystemData.Version, retrievedSystemData.Version, "Error in Version");
            }
        }

        /// <summary>
        /// Test for storing wireless peer info
        /// </summary>
        [Test]
        public void WirelessPeerInfoRoundTripTest()
        {
            var testPeerInfos = new SerializableWirelessPeerInfos
            {
                DeviceAddress = new IpAddress("1.2.3.4"),
                DeviceModel = "Test-DeviceModel",
                Details = new List<IWirelessPeerInfo>
                {
                    new SerializableWirelessPeerInfo
                    {
                        DeviceAddress = new IpAddress("9.8.7.6"),
                        DeviceModel = "Test-DeviceModel-PeerInfo",
                        InterfaceId = 815,
                        IsAccessPoint = true,
                        LinkUptime = TimeSpan.FromMinutes(5),
                        RemoteMacString = "Test-RemoteMacString",
                        RxSignalStrength = 8.15,
                        TxSignalStrength = 5.18,
                        Oids = new Dictionary<CachableValueMeanings, ICachableOid>
                        {
                            { CachableValueMeanings.WirelessLinkUptime, new CachableOid(new IpAddress("9.8.7.6"), CachableValueMeanings.WirelessLinkUptime, new [] { new Oid("1.2.3.4.5.6"), new Oid("9.8.7.6.5.4") })},
                            { CachableValueMeanings.WirelessRxSignalStrength, new CachableOid(new IpAddress("4.5.6.7"), CachableValueMeanings.WirelessRxSignalStrength, new [] { new Oid("73.74.75.76") })}
                        }
                    } as IWirelessPeerInfo
                }
            };

            using (var context = new CacheDatabaseContext(this.database))
            {
                context.Database.EnsureDeleted();

                context.Database.EnsureCreated();

                ClassicAssert.NotNull(context, "The database context is null");

                context.CacheData.Add(new CacheData
                {
                    Address = testPeerInfos.DeviceAddress,
                    InterfaceDetails = null,
                    WirelessPeerInfos = testPeerInfos,
                    SystemData = null
                });

                context.SaveChanges();
            }

            using (var context = new CacheDatabaseContext(this.database))
            {
                ClassicAssert.NotNull(context, "The database context is null");

                var retrievedPeerInfos = context.CacheData.First().WirelessPeerInfos;

                ClassicAssert.NotNull(retrievedPeerInfos, "Retrieved peers infos is null");

                ClassicAssert.NotNull(retrievedPeerInfos.Details, "Retrieved peer 'Details' property is null");

                ClassicAssert.AreEqual(testPeerInfos.Details.Count, retrievedPeerInfos.Details.Count, "Wrong count of peer infos");

                var firstPeerExpected = testPeerInfos.Details[0];
                var firstPeerRetrieved = retrievedPeerInfos.Details[0];

                ClassicAssert.AreEqual(firstPeerExpected.DeviceAddress, firstPeerRetrieved.DeviceAddress, "Error in DeviceAddress");
                ClassicAssert.AreEqual(firstPeerExpected.DeviceModel, firstPeerRetrieved.DeviceModel, "Error in DeviceModel");
                ClassicAssert.AreEqual(firstPeerExpected.InterfaceId, firstPeerRetrieved.InterfaceId, "Error in InterfaceId");
                ClassicAssert.AreEqual(firstPeerExpected.IsAccessPoint, firstPeerRetrieved.IsAccessPoint, "Error in IsAccessPoint");
                ClassicAssert.AreNotEqual(firstPeerExpected.LinkUptime, firstPeerRetrieved.LinkUptime, "Error in LinkUptime: Seems it has been serialized even though marked as ignore");
                ClassicAssert.AreEqual(firstPeerExpected.RemoteMacString, firstPeerRetrieved.RemoteMacString, "Error in RemoteMacString");
                ClassicAssert.AreNotEqual(firstPeerExpected.RxSignalStrength, firstPeerRetrieved.RxSignalStrength, "Error in RxSignalStrength: Seems it has been serialized even though marked as ignore");
                ClassicAssert.AreNotEqual(firstPeerExpected.TxSignalStrength, firstPeerRetrieved.TxSignalStrength, "Error in TxSignalStrength: Seems it has been serialized even though marked as ignore");

                ClassicAssert.AreEqual(firstPeerExpected.Oids.Count, firstPeerRetrieved.Oids.Count, "Error in Oids");

                var firstOidExpected = firstPeerRetrieved.Oids.First();
                var firstOidRetrieved = firstPeerRetrieved.Oids.First();
                ClassicAssert.AreEqual(firstOidExpected.Value.Address, firstOidRetrieved.Value.Address, "Error in Oids");
            }
        }

        /// <summary>
        /// Test for storing interface details
        /// </summary>
        [Test]
        public void InterfaceDetailsRoundTripTest()
        {
            var testInterfaceDetails = new SerializableInterfaceDetails
            {
                DeviceAddress = new IpAddress("1.2.3.4"),
                DeviceModel = "Test-DeviceModel",
                Details = new List<IInterfaceDetail>
                {
                    new SerializableInterfaceDetail
                    {
                        DeviceAddress = new IpAddress("9.8.7.6"),
                        DeviceModel = "Test-DeviceModel-InterfaceDetail",
                        InterfaceId = 815,
                        InterfaceName = "Test-InterfaceName",
                        InterfaceType = IanaInterfaceType.A12MppSwitch,
                        MacAddressString = "Test-MacAddressString"
                    } as IInterfaceDetail
                }
            };

            using (var context = new CacheDatabaseContext(this.database))
            {
                context.Database.EnsureDeleted();

                context.Database.EnsureCreated();

                ClassicAssert.NotNull(context, "The database context is null");

                context.CacheData.Add(new CacheData
                {
                    Address = testInterfaceDetails.DeviceAddress,
                    InterfaceDetails = testInterfaceDetails,
                    WirelessPeerInfos = null,
                    SystemData = null
                });

                context.SaveChanges();
            }

            using (var context = new CacheDatabaseContext(this.database))
            {
                ClassicAssert.NotNull(context, "The database context is null");

                var retrievedInterfaceDetails = context.CacheData.First().InterfaceDetails;

                ClassicAssert.NotNull(retrievedInterfaceDetails, "Retrieved interface details is null");

                ClassicAssert.NotNull(retrievedInterfaceDetails.Details, "Retrieved interface 'Details' property is null");

                ClassicAssert.AreEqual(testInterfaceDetails.Details.Count, retrievedInterfaceDetails.Details.Count, "Wrong count of peer infos");

                var firstPeerExpected = testInterfaceDetails.Details[0];
                var firstPeerRetrieved = retrievedInterfaceDetails.Details[0];

                ClassicAssert.AreEqual(firstPeerExpected.DeviceAddress, firstPeerRetrieved.DeviceAddress, "Error in DeviceAddress");
                ClassicAssert.AreEqual(firstPeerExpected.DeviceModel, firstPeerRetrieved.DeviceModel, "Error in DeviceModel");
                ClassicAssert.AreEqual(firstPeerExpected.InterfaceId, firstPeerRetrieved.InterfaceId, "Error in InterfaceId");
                ClassicAssert.AreEqual(firstPeerExpected.MacAddressString, firstPeerRetrieved.MacAddressString, "Error in MacAddressString");
                ClassicAssert.AreNotEqual(firstPeerExpected.QueryDuration, firstPeerRetrieved.QueryDuration, "Error in QueryDuration: Seems it has been serialized even though marked as ignore");
                ClassicAssert.AreEqual(firstPeerExpected.InterfaceName, firstPeerRetrieved.InterfaceName, "Error in InterfaceName");
                ClassicAssert.AreEqual(firstPeerExpected.InterfaceType, firstPeerRetrieved.InterfaceType, "Error in InterfaceType");
            }
        }
    }
}
