using System;
using System.Linq;
using NUnit.Framework;
using SnmpAbstraction;
using SnmpSharpNet;

namespace SnmpAbstractionTests
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
        /// Test for <see cref="ByteExtensions.ToDottedDecimalString" />
        /// </summary>
        [Test]
        public void ByteArrayToDottedDecimalStringTest()
        {
            var testArray = new byte[] { 0xb8, 0x27, 0xeb, 0x97, 0xb6, 0x39 };

            var result = testArray.ToDottedDecimalString();

            Assert.AreEqual("184.39.235.151.182.57", result, "Dotted Decimal representation not as expected");

            Assert.Throws<ArgumentNullException>(() =>
            {
                ByteExtensions.ToDottedDecimalString(null);
            }, "Not seen ArgumentNullException even though null has been passed for input");
        }

        /// <summary>
        /// Test for <see cref="ByteExtensions.ToDottedDecimalOid" />
        /// </summary>
        [Test]
        public void ByteArrayToDottedDecimalOidTest()
        {
            var testArray = new byte[] { 0xb8, 0x27, 0xeb, 0x97, 0xb6, 0x39 };
            var expectedOid = new Oid("184.39.235.151.182.57");

            var result = testArray.ToDottedDecimalOid();

            Assert.AreEqual(expectedOid, result, "returned OID representation not as expected");

            var rootOid = new Oid(".1.2.3.4.5");
            result = testArray.ToDottedDecimalOid(rootOid);
            var expectedConcateOid = rootOid + expectedOid;

            Assert.AreEqual(expectedConcateOid, result, "returned concatenated OID representation not as expected");

            Assert.Throws<ArgumentNullException>(() =>
            {
                ByteExtensions.ToDottedDecimalOid(null);
            }, "Not seen ArgumentNullException even though null has been passed for input");
        }

        /// <summary>
        /// Test for <see cref="StringExtensions.HexStringToByteArray" />
        /// </summary>
        [Test]
        public void HexStringToByteArrayTest()
        {
            var expectedArray = new byte[] { 0xb8, 0x27, 0xeb, 0x97, 0xb6, 0x39 };

            var testString = "    b8: 27 : eb:    97    :    b6 :39";
            var result = testString.HexStringToByteArray();
            Assert.IsTrue(result.SequenceEqual(expectedArray), "Byte array is not as expected");

            testString = "    b8- 27 - eb-    97    -    b6 -39";
            Assert.Throws<FormatException>(() => result = testString.HexStringToByteArray()); // non-default separator not provided
            result = testString.HexStringToByteArray('-');
            Assert.IsTrue(result.SequenceEqual(expectedArray), "Byte array is not as expected");

            Assert.Throws<ArgumentNullException>(() =>
            {
                StringExtensions.HexStringToByteArray(null);
            }, "Not seen ArgumentNullException even though null has been passed for input");
        }

        /// <summary>
        /// Test for <see cref="EnumerableExtensions.DecibelLogSum(System.Collections.Generic.IEnumerable{double})" />
        /// </summary>
        [Test]
        public void DecibelLogSumTest()
        {
            var sourceValues = new double[] { -100.0, double.PositiveInfinity, double.NaN, -100.0, double.NegativeInfinity };

            var logsum = sourceValues.DecibelLogSum();

            double minSum = -97.02;
            double maxSum = -96.98;
            Assert.IsTrue((minSum < logsum) && (logsum < maxSum), "Logsum {logsum} is not in range [{minSum}; {maxSum}]");
        }
    }
}
