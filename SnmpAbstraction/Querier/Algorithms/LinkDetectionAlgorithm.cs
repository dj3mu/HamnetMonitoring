using System;
using System.Diagnostics;
using System.Linq;

namespace SnmpAbstraction
{
    /// <summary>
    /// Algorithm that tries to detect links between listed devices and returns the link's details.
    /// </summary>
    internal class LinkDetectionAlgorithm : IQueryAlgorithm<ILinkDetails>
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The device handler to use for obtaining SNMP data for side #1 of the link.
        /// </summary>
        private IHamnetQuerier querier1;

        /// <summary>
        /// The device handler to use for obtaining SNMP data for side #2 of the link.
        /// </summary>
        private IHamnetQuerier querier2;

        /// <summary>
        /// Creates a new instance using the specified device handler.
        /// </summary>
        /// <param name="querier1">The querier to use for obtaining data of link side #1.</param>
        /// <param name="querier2">The querier to use for obtaining data of link side #2.</param>
        public LinkDetectionAlgorithm(IHamnetQuerier querier1, IHamnetQuerier querier2)
        {
            this.querier1 = querier1;
            this.querier2 = querier2;
        }

        /// <inheritdoc />
        public ILinkDetails DoQuery()
        {
            Stopwatch durationWatch = Stopwatch.StartNew();
            
            // we're only interested in the Wireless interfaces (not in ethernet ports or similar) and hence filter
            // for interface type 71 (ieee80211)
            var wifiInterfaces1 = this.querier1.NetworkInterfaceDetails.Where(i => i.InterfaceType == 71);
            var interfaces2 = this.querier2.NetworkInterfaceDetails;

            IWirelessPeerInfos wlPeerInfo2 = this.querier2.WirelessPeerInfos;

            // see if side #2 has peers with MAC address of side #1
            var peeringWithSide1 = from wlp2 in wlPeerInfo2
                                   from wm1 in wifiInterfaces1
                                   where (wlp2.RemoteMacString.ToLowerInvariant() == wm1.MacAddressString.ToLowerInvariant())
                                   select new Tuple<IInterfaceDetail, IWirelessPeerInfo, IInterfaceDetail>(wm1, wlp2, interfaces2.First(if2 => if2.InterfaceId == wlp2.InterfaceId));

            var returnDetails = new LinkDetails(peeringWithSide1.Select(ps1 => new LinkDetail(this.querier1.Address, ps1)), this.querier1.Address, durationWatch.Elapsed);

            durationWatch.Stop();

            log.Info($"LinkDetectionQuery: DurationWatch = {durationWatch.ElapsedMilliseconds} ms");

            var lazyContainerSum = wifiInterfaces1.Aggregate(TimeSpan.Zero, (a, c) => a += c.QueryDuration);
            lazyContainerSum += interfaces2.Aggregate(TimeSpan.Zero, (a, c) => a += c.QueryDuration);
            lazyContainerSum += wlPeerInfo2.Aggregate(TimeSpan.Zero, (a, c) => a += c.QueryDuration);
            log.Info($"LinkDetectionQuery: Lazy container sum = {lazyContainerSum.TotalMilliseconds} ms");

            return returnDetails;
        }
    }
}
