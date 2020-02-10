using System;
using System.IO;
using HamnetMonitoringService;
using NUnit.Framework;

namespace HamnetMonitoringServiceTests
{
    /// <summary>
    /// Tests for all kinds of extension methods (I don't want to scatter the tests too much).
    /// </summary>
    public class KmlGeneratorTests
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
        public void GenerationTest()
        {
            // var generator = new SingleLinkViewKmlGenerator(TestDataProvider.TestSiteZero1, TestDataProvider.TestSiteZero2);
            // var generator = new SingleLinkViewKmlGenerator(TestDataProvider.TestSiteDl0rus, TestDataProvider.TestSiteDb0dba);
            var generator = new SingleLinkViewKmlGenerator(TestDataProvider.TestSiteDl0rus, TestDataProvider.TestSite2);
            // var generator = new SingleLinkViewKmlGenerator(TestDataProvider.TestSite1, TestDataProvider.TestSite2);

            var text = generator.GenerateString();

            Assert.NotNull(text, "text is null");

            Console.WriteLine("Generated KML text:");
            Console.WriteLine(text);

            File.WriteAllText(Path.Combine(Path.GetTempPath(), "hamnetData.kml"), text);
        }
    }
}
