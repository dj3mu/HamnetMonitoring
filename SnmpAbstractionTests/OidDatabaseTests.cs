using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using SemVersion;
using SnmpAbstraction;
using System.IO;
using System.Linq;

namespace Tests
{
    /// <summary>
    /// Tests for the OID database.
    /// </summary>
    public class OidDatabaseTests
    {
        /// <summary>
        /// The address to use for the tests.
        /// </summary>
        private readonly string database = Path.Combine("Config", "DeviceDatabase.sqlite");

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
            using var context = new DeviceDatabaseContext(this.database);
            ClassicAssert.NotNull(context, "The database context is null");

            var result = context.DeviceSpecificOids
                .Where(d => (d.RetrievableValue == RetrievableValuesEnum.Model))
                .Include(d => d.RetrievableValue)
                .Include(d => d.DataType)
                .ToList();

            ClassicAssert.AreEqual(1, result.Count, "Returned result does not have exactly one row");
        }

        /// <summary>
        /// Test of TryFindDeviceId extension method.
        /// </summary>
        [Test]
        public void TryFindDeviceIdTest()
        {
            string goodDeviceName = "UnitTestDevice";
            string badDeviceName = "wrzlbrmft";

            using var context = new DeviceDatabaseContext(this.database);
            int foundDeviceId = int.MinValue;
            ClassicAssert.IsTrue(context.TryFindDeviceId(goodDeviceName, out foundDeviceId), $"Cannot find 'good' device '{goodDeviceName}'");
            ClassicAssert.AreEqual(1, foundDeviceId, $"Wrong device ID found for 'good' device '{goodDeviceName}'");

            ClassicAssert.IsFalse(context.TryFindDeviceId(badDeviceName, out foundDeviceId), $"Found 'bad' device '{badDeviceName}'");
        }

        /// <summary>
        /// Test of TryFindDeviceVersionId extension method.
        /// </summary>
        [Test]
        public void TryFindDeviceVersionId()
        {
            int goodDeviceId = 1;
            int badDeviceId = int.MaxValue;
            SemanticVersion inside1stBlockVersion = SemanticVersion.Parse("3.0.0");
            SemanticVersion inside2ndBlockVersion = SemanticVersion.Parse("100.43.0");
            SemanticVersion outsideAllVersion = SemanticVersion.Parse("10.43.0");

            using var context = new DeviceDatabaseContext(this.database);

            ClassicAssert.IsTrue(context.TryFindDeviceVersionId(goodDeviceId, inside1stBlockVersion, out DeviceVersion foundDeviceVersion), $"Cannot find 'good' device-version ID for device '{goodDeviceId}', inside-block-1 version {inside1stBlockVersion}");
            ClassicAssert.AreEqual(1, foundDeviceVersion, $"Wrong device-version ID found for 'good' device ID '{goodDeviceId}', inside-block-1 version {inside1stBlockVersion}");

            ClassicAssert.IsTrue(context.TryFindDeviceVersionId(goodDeviceId, inside2ndBlockVersion, out foundDeviceVersion), $"Cannot find 'good' device-version ID for device '{goodDeviceId}', inside-block-2 version {inside2ndBlockVersion}");
            ClassicAssert.AreEqual(2, foundDeviceVersion, $"Wrong device-version ID found for 'good' device ID '{goodDeviceId}', inside-block-2 version {inside2ndBlockVersion}");

            ClassicAssert.IsFalse(context.TryFindDeviceVersionId(goodDeviceId, outsideAllVersion, out foundDeviceVersion), $"Found 'good' device-version ID for device '{goodDeviceId}' using version outside all blocks.");

            ClassicAssert.IsTrue(context.TryFindDeviceVersionId(goodDeviceId, null, out foundDeviceVersion), $"Cannot find 'good' device-version ID for device '{goodDeviceId}', null version (i.e. highest available minimum version)");
            ClassicAssert.AreEqual(2, foundDeviceVersion, $"Wrong device-version ID found for 'good' device ID '{goodDeviceId}', null version (i.e. highest available minimum version)");

            ClassicAssert.IsFalse(context.TryFindDeviceVersionId(badDeviceId, null, out foundDeviceVersion), $"Found 'bad' device ID '{badDeviceId}'");
        }

        /// <summary>
        /// Test of TryFindDeviceVersionId extension method.
        /// </summary>
        [Test]
        public void TryFindOidMappingId()
        {
            int goodDeviceVersionId = 2;
            int badDeviceVersionId = int.MaxValue;

            using var context = new DeviceDatabaseContext(this.database);
            string foundOidMappingIds = string.Empty;

            ClassicAssert.IsTrue(context.TryFindOidLookupId(goodDeviceVersionId, out foundOidMappingIds), $"Cannot find OID mapping ID for 'good' device-version ID '{goodDeviceVersionId}'");
            ClassicAssert.AreEqual("2,1", foundOidMappingIds, $"Wrong OID mapping ID found for 'good' device-version ID '{goodDeviceVersionId}'");

            ClassicAssert.IsFalse(context.TryFindOidLookupId(badDeviceVersionId, out foundOidMappingIds), $"Found OID mapping ID for 'bad' device-version ID '{badDeviceVersionId}'");
        }

        /// <summary>
        /// Test of TryFindDeviceVersionId extension method.
        /// </summary>
        [Test]
        public void TryFindOids()
        {
            int goodOidLookupId = 1;
            int badOidLookupId = int.MaxValue;

            using var context = new DeviceDatabaseContext(this.database);

            ClassicAssert.IsTrue(context.TryFindDeviceSpecificOidLookup(goodOidLookupId, SnmpSharpNet.SnmpVersion.Ver1, SnmpSharpNet.SnmpVersion.Ver1, out IDeviceSpecificOidLookup foundLookup), $"Cannot find OID lookup ID for 'good' lookup ID '{goodOidLookupId}'");
            ClassicAssert.GreaterOrEqual(foundLookup.Count, 1, $"Empty lookup for for 'good' lookup ID '{goodOidLookupId}'");

            ClassicAssert.IsFalse(context.TryFindDeviceSpecificOidLookup(badOidLookupId, SnmpSharpNet.SnmpVersion.Ver1, SnmpSharpNet.SnmpVersion.Ver1, out foundLookup), $"Found lookup for 'bad' lookup ID '{badOidLookupId}'");
        }


        /// <summary>
        /// Test of TryFindDeviceVersionId extension method.
        /// </summary>
        [Test]
        public void DbConsistencyCheckTest()
        {
            using var context = new DeviceDatabaseContext(this.database);
            ClassicAssert.IsTrue(context.DataTypeConsistencyCheck(), "DataTypeConsistencyCheck failure");
            ClassicAssert.IsTrue(context.RetrievableValueConsistencyCheck(), "RetrievableValueConsistencyCheck failure");
        }
    }
}
