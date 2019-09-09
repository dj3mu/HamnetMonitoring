using System;
using System.Diagnostics;
using System.Linq;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class LazyLoadingUbiquitiAirFiberWirelessPeerInfo : LazyLoadingGenericWirelessPeerInfo
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
        private int peerIndex;

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
        /// <param name="peerIndex">The peer's index (to use in SNMP get).</param>
        public LazyLoadingUbiquitiAirFiberWirelessPeerInfo(
            ISnmpLowerLayer lowerSnmpLayer,
            IDeviceSpecificOidLookup oidLookup,
            int peerIndex)
            : base(
                lowerSnmpLayer,
                oidLookup,
                null,
                null, // For UBNT there's no way to find which wireless interface this peer belongs to.
                null)
        {
            this.peerIndex = peerIndex;
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
            // this value is simply not available for UBNT devices, at least not via SNMP (but for some models even not in Web GUI)
            return false;
        }

        /// <inheritdoc />
        protected override bool RetrieveRxSignalStrength()
        {
            RetrievableValuesEnum[] valuesToQuery =
            {
                RetrievableValuesEnum.RxSignalStrengthCh0AppendMacAndInterfaceId,
                RetrievableValuesEnum.RxSignalStrengthCh1AppendMacAndInterfaceId
            };

            DeviceSpecificOid[] deviceSpecificOids = new DeviceSpecificOid[valuesToQuery.Length];
            DeviceSpecificOid[] oidValues;
            if (!this.OidLookup.TryGetValues(out oidValues, valuesToQuery))
            {
                log.Warn($"Not even one supported OID has been found for getting the stream-specific RX level. RxSignalStrength for device '{this.DeviceAddress}', interface ID {this.peerIndex}, cannot be retrieved.");
                this.RxSignalStrengthBacking = double.NegativeInfinity;
                return true;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var clientSpecificOids = oidValues.Where(oid => oid != null).Select(oid => {
                var interfaceTypeOid = oid.Oid + new Oid(new int[] { this.peerIndex });

                return interfaceTypeOid;
            });

            var queryResults = this.LowerSnmpLayer.QueryAsInt(clientSpecificOids, "wireless peer info, RX signal strength");

            // Note: As the SNMP cannot return -infinity MikroTik devices return 0.
            //       Hence we effectively skip 0 values here assuming that stream is not in use.
            this.RxSignalStrengthBacking = queryResults.Values.Where(v => v != 0).DecibelLogSum();

            durationWatch.Stop();

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

            var interfactTypeOid = interfaceIdRootOid.Oid + new Oid(new int[] { this.peerIndex });

            this.LinkUptimeBacking = TimeSpan.FromSeconds(this.LowerSnmpLayer.QueryAsInt(interfactTypeOid, "wireless peer info, link uptime"));

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
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

            var interfactTypeOid = interfaceIdRootOid.Oid + new Oid(new int[] { this.peerIndex });

            int wirelessMode = this.LowerSnmpLayer.QueryAsInt(interfactTypeOid, "Wireless Mode");

            // from UBNT-MIB-airfiber:
            // radioLinkMode OBJECT-TYPE
            // SYNTAX     INTEGER {
            //         master (1),
            //         slave  (2),
            //         spectral (3)
            // } 
            this.isAccessPointBacking = wirelessMode == 1;

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

            var interfactTypeOid = interfaceIdRootOid.Oid + new Oid(new int[] { this.peerIndex });

            this.peerMacBacking = this.LowerSnmpLayer.QueryAsString(interfactTypeOid, "wireless peer info, MAC address");

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            this.peerMacRetrieved = true;

            return;
        }
    }
}
