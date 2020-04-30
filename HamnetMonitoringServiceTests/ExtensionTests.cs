using HamnetMonitoringService;
using NUnit.Framework;

namespace HamnetMonitoringServiceTests
{
    /// <summary>
    /// Tests for all kinds of extension methods (I don't want to scatter the tests too much).
    /// </summary>
    public class ExtensionTests
    {
        /// <summary>
        /// The test setup method.
        /// </summary>
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Test 
        /// </summary>
        [Test]
        public void ToKmlPlacemarkTest()
        {
            var placemark = TestDataProvider.TestSite1.ToKmlPlacemark();

            Assert.NotNull(placemark, "placemark is null");
            Assert.AreEqual(TestDataProvider.TestSite1.Callsign, placemark.Name, "name != callsign");
            Assert.AreEqual(TestDataProvider.TestSite1.Callsign, placemark.Id, "id != callsign");
            Assert.AreEqual(TestDataProvider.TestSite1.Comment, placemark.Description.Text, "description text != comment");
        }
    }
}
