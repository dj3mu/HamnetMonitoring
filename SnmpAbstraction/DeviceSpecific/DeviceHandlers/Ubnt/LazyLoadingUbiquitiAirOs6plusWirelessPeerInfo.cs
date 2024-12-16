using System;
using System.Diagnostics;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class LazyLoadingUbiquitiAirOs6plusWirelessPeerInfo : LazyLoadingGenericWirelessPeerInfo
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Field to sum up the query duration on each "Populate" call.
        /// </summary>
        private TimeSpan localQueryDuration = TimeSpan.Zero;

        /// <summary>
        /// The peer index (to use in SNMP get).
        /// </summary>
        private readonly int peerIndex;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="macAddress">The MAC address of the peer (serves as index in OIDs for MikroTik devices).</param>
        /// <param name="peerIndex">The peer's index (to use in SNMP get).</param>
        /// <param name="isAccessPoint">Value indicating whether the device proving the peer info is an access point or a client.</param>
        /// <param name="numberOfClients">The number of clients that are connected to this AP when in AP mode. null if not an AP or not available.</param>
        public LazyLoadingUbiquitiAirOs6plusWirelessPeerInfo(
            ISnmpLowerLayer lowerSnmpLayer,
            IDeviceSpecificOidLookup oidLookup,
            string macAddress,
            int peerIndex,
            bool? isAccessPoint,
            int? numberOfClients)
            : base(
                lowerSnmpLayer,
                oidLookup,
                macAddress,
                null, // For UBNT there's no way to find which wireless interface this peer belongs to.
                isAccessPoint,
                numberOfClients)
        {
            this.peerIndex = peerIndex;
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration => this.localQueryDuration;

        /// <inheritdoc />
        protected override bool RetrieveTxSignalStrength()
        {
            this.RecordCachableOid(CachableValueMeanings.WirelessTxSignalStrength, new Oid("0"));

            // this value is simply not available for UBNT devices, at least not via SNMP (but for some models even not in Web GUI)
            return false;
        }

        /// <inheritdoc />
        protected override bool RetrieveRxSignalStrength()
        {
            var valueToQuery = RetrievableValuesEnum.RxSignalStrengthApAppendMacAndInterfaceId;
            if (!this.OidLookup.TryGetValue(valueToQuery, out DeviceSpecificOid interfaceIdRootOid))
            {
                log.Warn($"Failed to obtain OID for '{valueToQuery}'");
                this.RxSignalStrengthBacking = double.NegativeInfinity;
                return true;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfactTypeOid = interfaceIdRootOid.Oid + new Oid(new int[] { this.peerIndex }) + this.RemoteMacString.HexStringToByteArray().ToDottedDecimalOid();

            this.RxSignalStrengthBacking = this.LowerSnmpLayer.QueryAsInt(interfactTypeOid, "wireless peer info, RX signal strength");

            durationWatch.Stop();

            this.RecordCachableOid(CachableValueMeanings.WirelessRxSignalStrength, interfactTypeOid);

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }

        /// <inheritdoc />
        protected override bool RetrieveLinkUptime()
        {
            var valueToQuery = RetrievableValuesEnum.LinkUptimeAppendMacAndInterfaceId;
            if (!this.OidLookup.TryGetValue(valueToQuery, out DeviceSpecificOid interfaceIdRootOid))
            {
                log.Warn($"Failed to obtain OID for '{valueToQuery}'");
                this.LinkUptimeBacking = TimeSpan.Zero;
                return true;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfactTypeOid = interfaceIdRootOid.Oid + new Oid(new int[] { this.peerIndex }) + this.RemoteMacString.HexStringToByteArray().ToDottedDecimalOid();

            this.LinkUptimeBacking = this.LowerSnmpLayer.QueryAsTimeSpan(interfactTypeOid, "wireless peer info, link uptime") ?? TimeSpan.Zero;

            durationWatch.Stop();

            this.RecordCachableOid(CachableValueMeanings.WirelessLinkUptime, interfactTypeOid);

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }

        /// <inheritdoc />
        protected override bool RetrieveCcq()
        {
            var valueToQuery = RetrievableValuesEnum.OverallCcqAppendInterfaceId;
            if (!this.OidLookup.TryGetValue(valueToQuery, out DeviceSpecificOid interfaceIdRootOid))
            {
                log.Warn($"Failed to obtain OID for '{valueToQuery}'");
                this.CcqBacking = null;
                return true;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfactTypeOid = interfaceIdRootOid.Oid + new Oid(new int[] { this.peerIndex });

            try
            {
                var ccqQueried = this.LowerSnmpLayer.QueryAsInt(interfactTypeOid, "wireless peer info, CCQ");
                this.CcqBacking = Convert.ToDouble(ccqQueried);

                this.RecordCachableOid(CachableValueMeanings.Ccq, interfactTypeOid);
            }
            catch(HamnetSnmpException)
            {
                log.Debug($"Ignoring HamnetSnmpException during CCQ retrieval");
                this.RecordCachableOid(CachableValueMeanings.Ccq, new Oid("0"));
                this.CcqBacking = null;
            }

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }
    }
}
