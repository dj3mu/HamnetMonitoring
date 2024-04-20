using NUnit.Framework;
using HamnetDbAbstraction;
using System;
using System.Linq;
using NUnit.Framework.Legacy;

namespace HamnetDbAbstractionTests
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
        /// Test for <see cref="HamnetDbSubnetExtensions.AssociateHosts" /> method.
        /// </summary>
        [Test]
        public void AssociateHostsTests()
        {
            var accessor = HamnetDbProvider.Instance.GetHamnetDb(TestConstants.ConnectionStringFilePath);

            var hosts = accessor.QueryMonitoredHosts();
            var subnets = accessor.QuerySubnets();

            var association = subnets.AssociateHosts(hosts);

            ClassicAssert.NotNull(association, "Returned association is null");
            ClassicAssert.Greater(association.Count, 0, "0 associations returned");

            Console.WriteLine($"Received {association.Count} associations");

            var pairs = association.Where(a => a.Value.Count == 2);

            Console.WriteLine($"Received {pairs.Count()} unique pairs");
        }

        /// <summary>
        /// Test for <see cref="HamnetDbAccessExtensions.UniqueMonitoredHostPairsInSameSubnet" /> method.
        /// </summary>
        [Test]
        public void UniqueMonitoredHostPairsInSameSubnet()
        {
            var accessor = HamnetDbProvider.Instance.GetHamnetDb(TestConstants.ConnectionStringFilePath);

            var uniquePairs = accessor.UniqueMonitoredHostPairsInSameSubnet();

            ClassicAssert.NotNull(uniquePairs, "Unique pairs received is null");
            ClassicAssert.Greater(uniquePairs.Count, 0, "0 unique pairs received");

            Console.WriteLine($"Received {uniquePairs.Count} unique pairs");
        }

        /// <summary>
        /// Test for <see cref="LocationExtensions.DirectionTo" /> method.
        /// </summary>
        [Test]
        public void LocationDirectionToTests()
        {
            var from = new LocationMock(48.090109, 11.961273, 622.0 + 30.0);
            var to = new LocationMock(48.184086, 11.611249, 499.0 + 65.0);

            var direction = from.DirectionTo(to);

            ClassicAssert.NotNull(direction, "direction is null");

            ClassicAssert.AreEqual(Math.Round(28028.59701576899, 9), Math.Round(direction.Distance, 9), "wrong distance");
            ClassicAssert.AreEqual(Math.Round(291.91583231225843, 9), Math.Round(direction.Bearing, 9), "wrong bearing");
            ClassicAssert.AreEqual(Math.Round(-0.179888135, 9), Math.Round(direction.Elevation, 9), "wrong elevation");
            ClassicAssert.AreSame(from, direction.From, "from not same");
            ClassicAssert.AreSame(to, direction.To, "to not same");
        }

        /// <summary>
        /// Test for <see cref="LocationExtensions.GreatCircleBearingTo" /> method.
        /// </summary>
        [Test]
        public void BearingToTests()
        {
            var from = new LocationMock(48.090109, 11.961273, 622.0 + 30.0);
            var to = new LocationMock(48.184086, 11.611249, 499.0 + 65.0);

            var bearing = from.GreatCircleBearingTo(to);

            ClassicAssert.AreEqual(Math.Round(291.91583231225843, 9), Math.Round(bearing, 9), "wrong bearing");
        }

        /// <summary>
        /// Test for <see cref="LocationExtensions.ElevationTo(ILocation, ILocation)" /> method.
        /// </summary>
        [Test]
        public void ElevationToTests()
        {
            var from = new LocationMock(48.090109, 11.961273, 622.0 + 30.0);
            var to = new LocationMock(48.184086, 11.611249, 499.0 + 65.0);

            var elevation = from.ElevationTo(to);

            ClassicAssert.AreEqual(Math.Round(-0.179888135, 9), Math.Round(elevation, 9), "wrong elevation");
        }

        /// <summary>
        /// Test for <see cref="LocationExtensions.ElevationTo(ILocation, ILocation)" /> method.
        /// </summary>
        [Test]
        public void FreeSpacePathlossTests()
        {
            var from = new LocationMock(48.090109, 11.961273, 622.0 + 30.0);
            var to = new LocationMock(48.184086, 11.611249, 499.0 + 65.0);
            var frequency = 5.8e9;

            var distance = from.HaversineDistanceTo(to);

            var fspl = from.FreeSpacePathloss(to, frequency);
            ClassicAssert.AreEqual(Math.Round(136.668370283, 9), Math.Round(fspl, 9), "wrong path loss w/o distance");

            fspl = distance.FreeSpacePathloss(frequency);
            ClassicAssert.AreEqual(Math.Round(136.668370283, 9), Math.Round(fspl, 9), "wrong path loss w/ distance");
        }

        /// <summary>
        /// Test for <see cref="LocationExtensions.DirectionTo" /> method.
        /// </summary>
        [Test]
        public void ToRadianTests()
        {
            var radian = 45.0.ToRadian();

            ClassicAssert.AreEqual(Math.Round(0.785398163397, 9), Math.Round(radian, 9), "wrong radian");
        }

        /// <summary>
        /// Test for <see cref="LocationExtensions.DirectionTo" /> method.
        /// </summary>
        [Test]
        public void ToDegreesTests()
        {
            var degrees = 0.785398163397.ToDegrees();

            ClassicAssert.AreEqual(Math.Round(45.0, 9), Math.Round(degrees, 9), "wrong degrees");
        }
    }
}
