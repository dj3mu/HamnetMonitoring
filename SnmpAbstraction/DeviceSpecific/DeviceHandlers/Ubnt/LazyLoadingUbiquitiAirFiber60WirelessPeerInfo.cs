using System;
using System.Diagnostics;
using System.Linq;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class LazyLoadingUbiquitiAirFiber60WirelessPeerInfo : LazyLoadingGenericWirelessPeerInfo
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
        /// The OID fragment that identifies the interface that this peer info is for.
        /// </summary>
        private Oid interfaceOidFragment;

        /// <summary>
        /// Backing field for the <see cref="IsAccessPoint" /> property.
        /// </summary>
        private bool? isAccessPointBacking;

        /// <summary>
        /// Field indicating whether we've already retrieved the <see cref="isAccessPointBacking" /> field value.
        /// </summary>
        private bool isAccessPointBackingPopulated = false;

        /// <summary>
        /// Backing field storing the peer's MAC address.
        /// </summary>
        private string peerMacBacking = null;

        /// <summary>
        /// Field indicating whether we have already retieved the remote MAC address.
        /// </summary>
        private bool peerMacRetrieved = false;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="interfaceOidFragment">The interface OID fragment that this wireless peer info is for.</param>
        /// <param name="numberOfClients">The number of clients that are connected to this AP when in AP mode. null if not an AP or not available.</param>
        public LazyLoadingUbiquitiAirFiber60WirelessPeerInfo(
            ISnmpLowerLayer lowerSnmpLayer,
            IDeviceSpecificOidLookup oidLookup,
            Oid interfaceOidFragment,
            int? numberOfClients)
            : base(
                lowerSnmpLayer,
                oidLookup,
                null,
                null, // For UBNT there's no way to find which wireless interface this peer belongs to.
                null,
                numberOfClients)
        {
            this.interfaceOidFragment = interfaceOidFragment;
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration => this.localQueryDuration;

        /// <inheritdoc />
        public override bool? IsAccessPoint
        {
            get
            {
                this.PopulateIsAccessPoint();

                return this.isAccessPointBacking;
            }
        }

        /// <inheritdoc />
        protected override string PeerMac
        {
            get
            {
                this.PopulatePeerMac();

                return this.peerMacBacking;
            }
        }

        /// <inheritdoc />
        protected override bool RetrieveTxSignalStrength()
        {
            var valueToQuery = RetrievableValuesEnum.TxSignalStrengthAppendMacAndInterfaceId;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.OidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                log.Warn($"Failed to obtain OID for '{valueToQuery}'");
                this.LinkUptimeBacking = TimeSpan.Zero;
                return true;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfactTypeOid = interfaceIdRootOid.Oid + this.interfaceOidFragment;

            var queryResult = this.LowerSnmpLayer.QueryAsInt(interfactTypeOid, "wireless peer info, TX signal strength");

            // Note: As the SNMP cannot return -infinity MikroTik devices return 0.
            //       Hence we effectively skip 0 values here assuming that stream is not in use.
            this.TxSignalStrengthBacking = queryResult;

            durationWatch.Stop();

            this.RecordCachableOid(CachableValueMeanings.WirelessTxSignalStrength, interfactTypeOid);

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }

        /// <inheritdoc />
        protected override bool RetrieveRxSignalStrength()
        {
            var valueToQuery = RetrievableValuesEnum.RxSignalStrengthCh0AppendMacAndInterfaceId;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.OidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                log.Warn($"Failed to obtain OID for '{valueToQuery}'");
                this.LinkUptimeBacking = TimeSpan.Zero;
                return true;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfactTypeOid = interfaceIdRootOid.Oid + this.interfaceOidFragment;

            var queryResult = this.LowerSnmpLayer.QueryAsInt(interfactTypeOid, "wireless peer info, RX signal strength");

            this.RxSignalStrengthBacking = queryResult;

            durationWatch.Stop();

            this.RecordCachableOid(CachableValueMeanings.WirelessRxSignalStrength, interfactTypeOid);

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }

        /// <inheritdoc />
        protected override bool RetrieveLinkUptime()
        {
            var valueToQuery = RetrievableValuesEnum.LinkUptimeAppendMacAndInterfaceId;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.OidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                log.Warn($"Failed to obtain OID for '{valueToQuery}'");
                this.LinkUptimeBacking = TimeSpan.Zero;
                return true;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfactTypeOid = interfaceIdRootOid.Oid + this.interfaceOidFragment;

            var uptimeQueried = this.LowerSnmpLayer.QueryAsTimeSpan(interfactTypeOid, "wireless peer info, link uptime");
            this.LinkUptimeBacking = uptimeQueried ?? TimeSpan.Zero;

            durationWatch.Stop();

            this.RecordCachableOid(CachableValueMeanings.WirelessLinkUptime, interfactTypeOid);

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }

        /// <inheritdoc />
        protected override bool RetrieveCcq()
        {
            // not supported - there doesn't seem to be any OID for this value in the UBNT-AFLTU-MIB.txt
            this.RecordCachableOid(CachableValueMeanings.Ccq, new Oid("0"));

            return false;
        }

        /// <summary>
        /// Populates the <see cref="isAccessPointBacking" /> field.
        /// </summary>
        private void PopulateIsAccessPoint()
        {
            if (this.isAccessPointBackingPopulated)
            {
                return;
            }

            var valueToQuery = RetrievableValuesEnum.WirelessMode;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.OidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                log.Warn($"Failed to obtain OID for '{valueToQuery}'");
                this.LinkUptimeBacking = TimeSpan.Zero;
                this.isAccessPointBackingPopulated = true;
                return;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfactTypeOid = interfaceIdRootOid.Oid;

            int wirelessMode = this.LowerSnmpLayer.QueryAsInt(interfactTypeOid, "Wireless Mode");

            // from UBNT-AF60-MIB.txt:
            // afLTURole OBJECT-TYPE
            //   SYNTAX	INTEGER {
            //      ap  (0),
            //      cpe (1)
            // }
            this.isAccessPointBacking = wirelessMode == 0 ? true : false;

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            this.isAccessPointBackingPopulated = true;

            return;
        }

        private void PopulatePeerMac()
        {
            if (this.peerMacRetrieved)
            {
                return;
            }

            var valueToQuery = RetrievableValuesEnum.WlanRemoteMacAddressAppendInterfaceId;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.OidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                log.Warn($"Failed to obtain OID for '{valueToQuery}'");
                this.peerMacBacking = null;
                this.peerMacRetrieved = true;
                return;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfactTypeOid = interfaceIdRootOid.Oid + this.interfaceOidFragment;

            this.peerMacBacking = this.LowerSnmpLayer.QueryAsString(interfactTypeOid, "wireless peer info, MAC address").Replace(" ", ":");

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            this.peerMacRetrieved = true;

            return;
        }
    }
}
