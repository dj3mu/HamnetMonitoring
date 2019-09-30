using System;
using System.Collections.Generic;
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
        /// Equality comparer to get wifi interfaces with distince MAC address.
        /// </summary>
        private readonly IEqualityComparer<IInterfaceDetail> InterfaceByMacAddressEqualityComparer = new InterfaceByMacAddressEqualityComparer();

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
            // we're only interested in the Wireless interfaces (not in ethernet ports or similar) and hence filter
            // for interface type 71 (ieee80211)
            // Note: We need to make the result distinct on MAC as some devices tend to return mulitple IEEE 802.11 interfaces with same MAC address.
            var wifiInterfaces1 = this.querier1.NetworkInterfaceDetails.Where(i => i.InterfaceType == IanaInterfaceType.Ieee80211).Distinct(this.InterfaceByMacAddressEqualityComparer).ToList();
            var wifiInterfaces2 = this.querier2.NetworkInterfaceDetails.Where(i => i.InterfaceType == IanaInterfaceType.Ieee80211).Distinct(this.InterfaceByMacAddressEqualityComparer).ToList();

            IWirelessPeerInfos wlPeerInfo1 = this.querier1.WirelessPeerInfos;
            IWirelessPeerInfos wlPeerInfo2 = this.querier2.WirelessPeerInfos;

            // see if side #2 has peers with MAC address of side #1
            var peeringWithSide1 = (from wlp2 in wlPeerInfo2
                                   from wi1 in wifiInterfaces1
                                   where (wlp2.RemoteMacString?.ToLowerInvariant() == wi1.MacAddressString?.ToLowerInvariant())
                                   select new Tuple<IInterfaceDetail, IWirelessPeerInfo>(wi1, wlp2)).SingleOrDefault();

            // see if side #1 has peers with MAC address of side #2
            var peeringWithSide2 = (from wlp1 in wlPeerInfo1
                                   from wi2 in wifiInterfaces2
                                   where (wlp1.RemoteMacString?.ToLowerInvariant() == wi2.MacAddressString?.ToLowerInvariant())
                                   select new Tuple<IInterfaceDetail, IWirelessPeerInfo>(wi2, wlp1)).SingleOrDefault();

            if (peeringWithSide1 == null)
            {
                throw new HamnetSnmpException($"Side #2 ({this.querier2}) seems to have no peerings with side #1 ({this.querier1})");
            }

            if (peeringWithSide2 == null)
            {
                throw new HamnetSnmpException($"Side #1 ({this.querier1}) seems to have no peerings with side #2 ({this.querier2})");
            }

            var returnDetails = new LinkDetails(
                new LinkDetail[] { new LinkDetail(
                    this.querier1.Address,
                    new LinkRelatedResultCollection(
                        peeringWithSide1.Item1,
                        peeringWithSide2.Item1,
                        peeringWithSide2.Item2,
                        peeringWithSide1.Item2))},
                    this.querier1.Address,
                    this.querier1.SystemData.DeviceModel);

            var lazyContainerSum = wifiInterfaces1.Aggregate(TimeSpan.Zero, (a, c) => a += c.QueryDuration);
            lazyContainerSum += wifiInterfaces2.Aggregate(TimeSpan.Zero, (a, c) => a += c.QueryDuration);
            lazyContainerSum += wlPeerInfo1.Aggregate(TimeSpan.Zero, (a, c) => a += c.QueryDuration);
            lazyContainerSum += wlPeerInfo2.Aggregate(TimeSpan.Zero, (a, c) => a += c.QueryDuration);

            log.Debug($"LinkDetectionQuery: Lazy container sum = {lazyContainerSum.TotalMilliseconds} ms");

            return returnDetails;
        }
    }
}
