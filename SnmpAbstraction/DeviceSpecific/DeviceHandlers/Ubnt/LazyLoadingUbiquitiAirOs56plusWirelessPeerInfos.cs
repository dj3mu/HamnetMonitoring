using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Worker class for lazy-loading interface details for AirOs v 6 and higher.
    /// /// </summary>
    internal class LazyLoadingUbiquitiAirOs56plusWirelessPeerInfos : LazyLoadingGenericWirelessPeerInfos
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
        public LazyLoadingUbiquitiAirOs56plusWirelessPeerInfos(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup)
            : base(lowerSnmpLayer, oidLookup)
        {
        }

        /// <inheritdoc />
        public override TimeSpan GetQueryDuration()
        {
            return this.localQueryDuration;
        }

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

            log.Debug($"RetrievePeerInfo: Received {interfaceVbs.Count} peer MAC addresses from '{this.DeviceAddress}' using OID {interfaceIdRootOid}");

            foreach (Vb item in interfaceVbs)
            {
                int interfaceId = Convert.ToInt32(item.Oid[item.Oid.Length - 7]);
                IEnumerable<uint> macOidFragments = item.Oid.Skip(item.Oid.Length - 6).Take(6);

                this.PeerInfosBacking.Add(
                    new LazyLoadingUbiquitiAirOs6plusWirelessPeerInfo(
                        this.LowerSnmpLayer,
                        this.OidLookup,
                        macOidFragments.ToHexString(),
                        interfaceId, // last element of OID contains the interface ID on which this peer is connected
                        this.CheckIsAccessPoint(interfaceId)
                    ));
            }

            return true;
        }

        /// <inheritdoc />
        protected override bool? CheckIsAccessPoint(int interfaceId)
        {
            var valueToQuery = RetrievableValuesEnum.WirelessMode;
            DeviceSpecificOid wirelessModeOid;
            if (this.OidLookup.TryGetValue(valueToQuery, out wirelessModeOid))
            {
                // finally we need to get the count of registered clients
                // if it's 0, this must be a client (this method will only be called if the registration table
                // contains at least one entry)
                // need to append the interface ID to the client count OID
                var queryOid = wirelessModeOid.Oid + new Oid(new int[] { interfaceId });

                var returnCollection = this.LowerSnmpLayer.Query(queryOid);
                if (returnCollection.Count == 0)
                {
                    log.Warn($"Unable to get AP / client distinction for wireless interface #{interfaceId} of device '{this.LowerSnmpLayer.Address}': Query of OID for '{valueToQuery}' returned empty result");
                    return null;
                }

                int stationModeInt = returnCollection[queryOid].Value.ToInt();

                // from UBNT AirMAX MIB:
                // ubntRadioMode OBJECT-TYPE
                //     SYNTAX     INTEGER {
                //     sta(1),
                //     ap(2),
                //     aprepeater(3),
                //     apwds(4)
                // }
                return stationModeInt != 1;
            }

            log.Warn($"Unable to get AP / client distinction for wireless interface #{interfaceId} of device '{this.LowerSnmpLayer.Address}': No OID for value of type '{valueToQuery}' available");

            return null;
        }
    }
}
