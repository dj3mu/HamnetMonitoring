using System;
using System.Collections.Generic;
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

            HashSet<string> handledMacs = new HashSet<string>(interfaceVbs.Count);

            foreach (var interfaceVb in interfaceVbs)
            {
                if (interfaceVb.Oid.Length != 20)
                {
                    // the OIDs resulting from SNMPWalk are not only those with MAC-address inside
                    // but we can safely detect the correct ones by its length (20 vs 14 segments)
                    continue;
                }

                IEnumerable<uint> macOidFragments = interfaceVb.Oid.Skip(interfaceVb.Oid.Length - 7).Take(6);
                var macAddress = macOidFragments.ToHexString();

                if (handledMacs.Contains(macAddress))
                {
                    continue;
                }

                int interfaceId = Convert.ToInt32(interfaceVb.Oid.Last());
                this.PeerInfosBacking.Add(
                    new LazyLoadingUbiquitiAirOs4WirelessPeerInfo(
                        this.LowerSnmpLayer,
                        this.OidLookup,
                        macAddress,
                        interfaceId, // last element of OID contains the interface ID on which this peer is connected
                        interfaceVbs,
                        interfaceVbs.Count
                    ));

                handledMacs.Add(macAddress);
            }

            return true;
        }

        /// <inheritdoc />
        protected override bool? CheckIsAccessPoint(int interfaceId, out int? numberOfClients)
        {
            // NOP:
            // Method CheckIsAccessPoint is not supported for LazyLoadingUbiquitiAirOs4WirelessPeerInfos.
            // Parameter is determined internally in single LazyLoadingUbiquitiAirOs4WirelessPeerInfo.
            numberOfClients = null;
            return null;
        }
    }
}
