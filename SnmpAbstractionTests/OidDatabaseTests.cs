using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SnmpAbstraction;
using System.IO;
using System.Linq;

namespace Tests
{
    public class OidDatabaseTests
    {
        /// <summary>
        /// The address to use for the tests.
        /// </summary>
        private readonly string database = Path.Combine("Config", "DeviceDatabase.sqlite");

        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Test for construction (successfull and with invalid params).
        /// </summary>
        [Test]
        public void DbContextTest()
        {
            using (var context = new DeviceDatabaseContext(this.database))
            {
                Assert.NotNull(context, "The database context is null");

                context.Database.EnsureCreated();
                
                var result = context.DeviceSpecificOids
                    .Where(d => (d.RetrievableValue == RetrievableValuesEnum.Model) && (d.DeviceVersion.Device.Vendor.VendorName == "MikroTik") && (d.DeviceVersion.MinimumVersion == "0"))
                    .ToList();
            }
        }
    }
}
