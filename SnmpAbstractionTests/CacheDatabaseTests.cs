using NUnit.Framework;
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
            using (var context = new CacheDatabaseContext(this.database))
            {
                Assert.NotNull(context, "The database context is null");
            }
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

                Assert.NotNull(context, "The database context is null");

                context.CacheData.Add(new CacheData
                {
                    Address = testSystemData.DeviceAddress,
                    CachableOids = null,
                    InterfaceDetails = null,
                    WirelessPeerInfos = null,
                    SystemData = testSystemData
                });

                context.SaveChanges();
            }

            using (var context = new CacheDatabaseContext(this.database))
            {
                Assert.NotNull(context, "The database context is null");

                var retrievedSystemData = context.CacheData.First().SystemData;

                Assert.NotNull(retrievedSystemData, "Retrieved system data is null");

                Assert.AreEqual(testSystemData.Contact, retrievedSystemData.Contact, "Error in Contact");
                Assert.AreEqual(testSystemData.Description, retrievedSystemData.Description, "Error in Description");
                Assert.AreEqual(testSystemData.DeviceAddress, retrievedSystemData.DeviceAddress, "Error in DeviceAddress");
                Assert.AreEqual(testSystemData.DeviceModel, retrievedSystemData.DeviceModel, "Error in DeviceModel");
                Assert.AreEqual(testSystemData.EnterpriseObjectId, retrievedSystemData.EnterpriseObjectId, "Error in EnterpriseObjectId");
                Assert.AreEqual(testSystemData.Location, retrievedSystemData.Location, "Error in Location");
                Assert.AreEqual(testSystemData.Model, retrievedSystemData.Model, "Error in Model");
                Assert.AreEqual(testSystemData.Name, retrievedSystemData.Name, "Error in Name");
                Assert.AreNotEqual(testSystemData.QueryDuration, retrievedSystemData.QueryDuration, "Error in QueryDuration: Seems it has been serialized even though marked as ignore");
                Assert.AreNotEqual(testSystemData.Uptime, retrievedSystemData.Uptime, "Error in Uptime: Seems it has been serialized even though marked as ignore");
                Assert.AreEqual(testSystemData.Version, retrievedSystemData.Version, "Error in Version");
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
                        TxSignalStrength = 5.18
                    } as IWirelessPeerInfo
                }
            };

            using (var context = new CacheDatabaseContext(this.database))
            {
                context.Database.EnsureDeleted();

                context.Database.EnsureCreated();

                Assert.NotNull(context, "The database context is null");

                context.CacheData.Add(new CacheData
                {
                    Address = testPeerInfos.DeviceAddress,
                    CachableOids = null,
                    InterfaceDetails = null,
                    WirelessPeerInfos = testPeerInfos,
                    SystemData = null
                });

                context.SaveChanges();
            }

            using (var context = new CacheDatabaseContext(this.database))
            {
                Assert.NotNull(context, "The database context is null");

                var retrievedPeerInfos = context.CacheData.First().WirelessPeerInfos;

                Assert.NotNull(retrievedPeerInfos, "Retrieved peers infos is null");

                Assert.NotNull(retrievedPeerInfos.Details, "Retrieved peer 'Details' property is null");

                Assert.AreEqual(testPeerInfos.Details.Count, retrievedPeerInfos.Details.Count, "Wrong count of peer infos");

                var firstPeerExpected = testPeerInfos.Details.First();
                var firstPeerRetrieved = retrievedPeerInfos.Details.First();

                Assert.AreEqual(firstPeerExpected.DeviceAddress, firstPeerRetrieved.DeviceAddress, "Error in DeviceAddress");
                Assert.AreEqual(firstPeerExpected.DeviceModel, firstPeerRetrieved.DeviceModel, "Error in DeviceModel");
                Assert.AreEqual(firstPeerExpected.InterfaceId, firstPeerRetrieved.InterfaceId, "Error in InterfaceId");
                Assert.AreEqual(firstPeerExpected.IsAccessPoint, firstPeerRetrieved.IsAccessPoint, "Error in IsAccessPoint");
                Assert.AreNotEqual(firstPeerExpected.LinkUptime, firstPeerRetrieved.LinkUptime, "Error in LinkUptime: Seems it has been serialized even though marked as ignore");
                Assert.AreNotEqual(firstPeerExpected.QueryDuration, firstPeerRetrieved.QueryDuration, "Error in QueryDuration: Seems it has been serialized even though marked as ignore");
                Assert.AreEqual(firstPeerExpected.RemoteMacString, firstPeerRetrieved.RemoteMacString, "Error in RemoteMacString");
                Assert.AreNotEqual(firstPeerExpected.RxSignalStrength, firstPeerRetrieved.RxSignalStrength, "Error in RxSignalStrength: Seems it has been serialized even though marked as ignore");
                Assert.AreNotEqual(firstPeerExpected.TxSignalStrength, firstPeerRetrieved.TxSignalStrength, "Error in TxSignalStrength: Seems it has been serialized even though marked as ignore");

                Assert.IsTrue(firstPeerExpected.Oids.All(o => firstPeerRetrieved.Oids.Contains(o)), "Error in Oids");
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

                Assert.NotNull(context, "The database context is null");

                context.CacheData.Add(new CacheData
                {
                    Address = testInterfaceDetails.DeviceAddress,
                    CachableOids = null,
                    InterfaceDetails = testInterfaceDetails,
                    WirelessPeerInfos = null,
                    SystemData = null
                });

                context.SaveChanges();
            }

            using (var context = new CacheDatabaseContext(this.database))
            {
                Assert.NotNull(context, "The database context is null");

                var retrievedInterfaceDetails = context.CacheData.First().InterfaceDetails;

                Assert.NotNull(retrievedInterfaceDetails, "Retrieved interface details is null");

                Assert.NotNull(retrievedInterfaceDetails.Details, "Retrieved interface 'Details' property is null");

                Assert.AreEqual(testInterfaceDetails.Details.Count, retrievedInterfaceDetails.Details.Count, "Wrong count of peer infos");

                var firstPeerExpected = testInterfaceDetails.Details.First();
                var firstPeerRetrieved = retrievedInterfaceDetails.Details.First();

                Assert.AreEqual(firstPeerExpected.DeviceAddress, firstPeerRetrieved.DeviceAddress, "Error in DeviceAddress");
                Assert.AreEqual(firstPeerExpected.DeviceModel, firstPeerRetrieved.DeviceModel, "Error in DeviceModel");
                Assert.AreEqual(firstPeerExpected.InterfaceId, firstPeerRetrieved.InterfaceId, "Error in InterfaceId");
                Assert.AreEqual(firstPeerExpected.MacAddressString, firstPeerRetrieved.MacAddressString, "Error in MacAddressString");
                Assert.AreNotEqual(firstPeerExpected.QueryDuration, firstPeerRetrieved.QueryDuration, "Error in QueryDuration: Seems it has been serialized even though marked as ignore");
                Assert.AreEqual(firstPeerExpected.InterfaceName, firstPeerRetrieved.InterfaceName, "Error in InterfaceName");
                Assert.AreEqual(firstPeerExpected.InterfaceType, firstPeerRetrieved.InterfaceType, "Error in InterfaceType");
            }
        }

        /// <summary>
        /// Test for storing system data
        /// </summary>
        [Test]
        public void CachableOidsRoundTripTest()
        {
            var cachableOids = new List<ICachableOid>
            {
                new SerializableCachableOid(new Oid("2.3.4.5.6.7.8.9.0"), new Oid("17.3.4.5.6.7.8.9.0"))
                {
                    Address = new IpAddress("1.2.3.4"),
                    Meaning = CachableValueMeanings.WirelessRxSignalStrength
                }
            };

            using (var context = new CacheDatabaseContext(this.database))
            {
                context.Database.EnsureDeleted();

                context.Database.EnsureCreated();

                Assert.NotNull(context, "The database context is null");

                context.CacheData.Add(new CacheData
                {
                    Address = cachableOids.First().Address,
                    CachableOids = cachableOids,
                    InterfaceDetails = null,
                    WirelessPeerInfos = null,
                    SystemData = null
                });

                context.SaveChanges();
            }

            using (var context = new CacheDatabaseContext(this.database))
            {
                Assert.NotNull(context, "The database context is null");

                var retrievedCachableOids = context.CacheData.First().CachableOids;

                Assert.NotNull(retrievedCachableOids, "Retrieved cachable OIDs is null");

                Assert.AreEqual(cachableOids.Count(), retrievedCachableOids.Count(), "Wrong count of OIDs");

                var firstOidExpected = cachableOids.First();
                var firstOidRetrieved = retrievedCachableOids.First();

                Assert.AreEqual(firstOidExpected.Address, firstOidRetrieved.Address, "Error in Value.Address");
                Assert.AreEqual(firstOidExpected.IsSingleOid, firstOidRetrieved.IsSingleOid, "Error in Value.IsSingleOid");
                Assert.AreEqual(firstOidExpected.Meaning, firstOidRetrieved.Meaning, "Error in Value.Meaning");
                Assert.Throws<InvalidOperationException>(() => { firstOidExpected.Oid.Equals(firstOidRetrieved.Oid); }, "No exception when accessing Oid property on an object containing more than one OID");

                Assert.IsTrue(firstOidExpected.Oids.All(o => firstOidRetrieved.Oids.Contains(o)), "Error in Value.Oids");
            }
        }
    }
}
