using HamnetMonitoringService;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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

            ClassicAssert.NotNull(placemark, "placemark is null");
            ClassicAssert.AreEqual(TestDataProvider.TestSite1.Callsign, placemark.Name, "name != callsign");
            ClassicAssert.AreEqual(TestDataProvider.TestSite1.Callsign, placemark.Id, "id != callsign");
            ClassicAssert.AreEqual(TestDataProvider.TestSite1.Comment, placemark.Description.Text, "description text != comment");
        }
    }
}
