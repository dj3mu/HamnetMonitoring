using System;
using System.Diagnostics;
using System.Linq;

namespace SnmpAbstraction
{
    /// <summary>
    /// Worker class for lazy-loading interface details for AirOS v 4 devices.
    /// /// </summary>
    internal class LazyLoadingUbiquitiAirOsUpTo56WirelessPeerInfos : LazyLoadingGenericWirelessPeerInfos
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Field to sum up the query duration on each "Populate" call.
        /// </summary>
        private TimeSpan localQueryDuration = TimeSpan.Zero;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        public LazyLoadingUbiquitiAirOsUpTo56WirelessPeerInfos(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup)
            : base(lowerSnmpLayer, oidLookup)
        {
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration => this.localQueryDuration;

        /// <inheritdoc />
        protected override bool RetrievePeerInfo()
        {
            Stopwatch durationWatch = Stopwatch.StartNew();

            this.PeerInfosBacking.Clear();

            // Note: The MAC address serves as an index in nested OIDs for MikroTik devices.
            //       So this way, we get the amount of peers as well as an index to them.
            var valueToQuery = RetrievableValuesEnum.WlanRemoteMacAddressWalkRoot;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.OidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                return false;
            }

            var interfaceVbs = this.LowerSnmpLayer.DoWalk(interfaceIdRootOid.Oid);

            durationWatch.Stop();

            this.localQueryDuration = durationWatch.Elapsed;

            log.Debug($"RetrievePeerInfo: Received peer MAC addresses from '{this.DeviceAddress}' using OID {interfaceIdRootOid}");

            int interfaceId = Convert.ToInt32(interfaceVbs[0].Oid.Last());
            this.PeerInfosBacking.Add(
                new LazyLoadingUbiquitiAirOs4WirelessPeerInfo(
                    this.LowerSnmpLayer,
                    this.OidLookup,
                    interfaceId, // last element of OID contains the interface ID on which this peer is connected
                    interfaceVbs
                ));

            return true;
        }

        /// <inheritdoc />
        protected override bool? CheckIsAccessPoint(int interfaceId)
        {
            // NOP:
            // Method CheckIsAccessPoint is not supported for LazyLoadingUbiquitiAirOs4WirelessPeerInfos.
            // Parameter is determined internally in single LazyLoadingUbiquitiAirOs4WirelessPeerInfo.
            return null;
        }
    }
}
