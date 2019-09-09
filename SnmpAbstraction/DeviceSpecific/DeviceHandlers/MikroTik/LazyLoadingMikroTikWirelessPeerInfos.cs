using System;
using System.Collections.Generic;
using System.Diagnostics;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Worker class for lazy-loading interface details.
    /// /// </summary>
    internal class LazyLoadingMikroTikWirelessPeerInfos : LazyLoadingGenericWirelessPeerInfos
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
        public LazyLoadingMikroTikWirelessPeerInfos(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup)
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

            log.Debug($"RetrievePeerInfo: Received {interfaceVbs.Count} peer MAC addresses from '{this.DeviceAddress}' using OID {interfaceIdRootOid}");

            foreach (Vb item in interfaceVbs)
            {
                int interfaceId = Convert.ToInt32(item.Oid[item.Oid.Length - 1]);
                this.PeerInfosBacking.Add(
                    new LazyLoadingMikroTikWirelessPeerInfo(
                        this.LowerSnmpLayer,
                        this.OidLookup,
                        item.Value.ToString().Replace(' ', ':'),
                        interfaceId, // last element of OID contains the interface ID on which this peer is connected
                        this.CheckIsAccessPoint(interfaceId)
                    ));
            }

            return true;
        }

        /// <inheritdoc />
        protected override bool? CheckIsAccessPoint(int interfaceId)
        {
            var valueToQuery = RetrievableValuesEnum.WirelessClientCount;
            DeviceSpecificOid wirelessClientCountRootOid;
            if (this.OidLookup.TryGetValue(valueToQuery, out wirelessClientCountRootOid))
            {
                // finally we need to get the count of registered clients
                // if it's 0, this must be a client (this method will only be called if the registration table
                // contains at least one entry)
                // need to append the interface ID to the client count OID
                var queryOid = wirelessClientCountRootOid.Oid + new Oid(new int[] { interfaceId });

                var returnCollection = this.LowerSnmpLayer.Query(queryOid);
                if (returnCollection.Count == 0)
                {
                    log.Warn($"Unable to get AP / client distinction for wireless interface #{interfaceId} of device '{this.LowerSnmpLayer.Address}': Query of OID for '{valueToQuery}' returned empty result");
                    return null;
                }

                var returnValue = returnCollection[queryOid];

                return returnValue.Value.ToInt() > 0;
            }

            log.Warn($"Unable to get AP / client distinction for wireless interface #{interfaceId} of device '{this.LowerSnmpLayer.Address}': No OID for value of type '{valueToQuery}' available");

            return null;
        }
    }
}
