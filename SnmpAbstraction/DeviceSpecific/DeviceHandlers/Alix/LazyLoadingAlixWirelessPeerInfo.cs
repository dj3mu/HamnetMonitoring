using System;
using System.Diagnostics;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class LazyLoadingAlixWirelessPeerInfo : LazyLoadingGenericWirelessPeerInfo
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
#pragma warning disable IDE0052 // for future use
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#pragma warning restore

        /// <summary>
        /// Field to sum up the query duration on each "Populate" call.
        /// </summary>
        private TimeSpan localQueryDuration = TimeSpan.Zero;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="macAddress">The MAC address of the peer (serves as index in OIDs for MikroTik devices).</param>
        /// <param name="interfaceId">The ID of the interface (i.e. the value to append to interface-specific OIDs).</param>
        /// <param name="isAccessPoint">Value indicating whether the device proving the peer info is an access point or a client.</param>
        /// <param name="numberOfClients">The number of clients that are connected to this AP when in AP mode. null if not an AP or not available.</param>
        public LazyLoadingAlixWirelessPeerInfo(
            ISnmpLowerLayer lowerSnmpLayer,
            IDeviceSpecificOidLookup oidLookup,
            string macAddress,
            int? interfaceId,
            bool? isAccessPoint,
            int? numberOfClients)
            : base(lowerSnmpLayer, oidLookup, macAddress, interfaceId, isAccessPoint, numberOfClients)
        {
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration => this.localQueryDuration;

        /// <inheritdoc />
        protected override bool RetrieveTxSignalStrength()
        {
            this.RecordCachableOid(CachableValueMeanings.WirelessTxSignalStrength, new Oid("0"));

            // this value is simply not available for ALIX devices, at least not via SNMP (but for some models even not in Web GUI)
            return false;
        }

        /// <inheritdoc />
        protected override bool RetrieveRxSignalStrength()
        {
            Stopwatch durationWatch = Stopwatch.StartNew();

            if (!this.OidLookup.TryGetValue(RetrievableValuesEnum.RxSignalStrengthCh0AppendMacAndInterfaceId, out DeviceSpecificOid rxLevelRootOid) || rxLevelRootOid.Oid.IsNull)
            {
                return false;
            }

            var rxLevelOid = rxLevelRootOid.Oid + new Oid(new int[] { this.InterfaceId.Value });

            this.RxSignalStrengthBacking = this.LowerSnmpLayer.QueryAsInt(rxLevelOid, "RSSI single value");

            this.RecordCachableOid(CachableValueMeanings.WirelessRxSignalStrength, rxLevelOid);

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }

        /// <inheritdoc />
        protected override bool RetrieveLinkUptime()
        {
            this.RecordCachableOid(CachableValueMeanings.WirelessLinkUptime, new Oid("0"));

            // this value is simply not available for ALIX devices, at least not via SNMP (but for some models even not in Web GUI)
            return false;
        }

        /// <inheritdoc />
        protected override bool RetrieveCcq()
        {
            this.RecordCachableOid(CachableValueMeanings.Ccq, new Oid("0"));

            // this value is simply not available for ALIX devices, at least not via SNMP (but for some models even not in Web GUI)
            return false;
        }
    }
}
