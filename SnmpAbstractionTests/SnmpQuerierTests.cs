using System;
using NUnit.Framework;
using SnmpAbstraction;

namespace SnmpAbstractionTests
{
    /// <summary>
    /// Tests for SnmpQuerier
    /// </summary>
    public class SnmpQuerierTests
    {
        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Test for construction and basic usage.
        /// </summary>
        [Test]
        public void CreateTest()
        {
            var querier = SnmpQuerierFactory.Instance.Create(TestConstants.TestAddress);

            Assert.NotNull(querier, "Create(...) returned null");

            Console.WriteLine("Obtained SnmpQuerier for device with system data:");
            Console.WriteLine(querier.SystemData);
        }
    }
}
