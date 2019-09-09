using System;
using System.Diagnostics;
using System.Linq;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class LazyLoadingMikroTikWirelessPeerInfo : LazyLoadingGenericWirelessPeerInfo
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
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="macAddress">The MAC address of the peer (serves as index in OIDs for MikroTik devices).</param>
        /// <param name="interfaceId">The ID of the interface (i.e. the value to append to interface-specific OIDs).</param>
        /// <param name="isAccessPoint">Value indicating whether the device proving the peer info is an access point or a client.</param>
        public LazyLoadingMikroTikWirelessPeerInfo(
            ISnmpLowerLayer lowerSnmpLayer,
            IDeviceSpecificOidLookup oidLookup,
            string macAddress,
            int? interfaceId,
            bool? isAccessPoint)
            : base(lowerSnmpLayer, oidLookup, macAddress, interfaceId, isAccessPoint)
        {
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration => this.localQueryDuration;

        /// <inheritdoc />
        protected override bool RetrieveTxSignalStrength()
        {
            var valueToQuery = RetrievableValuesEnum.TxSignalStrengthAppendMacAndInterfaceId;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.OidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                log.Warn($"Failed to obtain OID for '{valueToQuery}'");
                this.TxSignalStrengthBacking = double.NegativeInfinity;
                return true;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfactTypeOid = interfaceIdRootOid.Oid + this.RemoteMacString.HexStringToByteArray().ToDottedDecimalOid() + new Oid(new int[] { this.InterfaceId.Value });

            this.TxSignalStrengthBacking = this.LowerSnmpLayer.QueryAsInt(interfactTypeOid, "wireless peer info, TX signal strength");

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }

        /// <inheritdoc />
        protected override bool RetrieveRxSignalStrength()
        {
            RetrievableValuesEnum[] valuesToQuery =
            {
                RetrievableValuesEnum.RxSignalStrengthCh0AppendMacAndInterfaceId,
                RetrievableValuesEnum.RxSignalStrengthCh1AppendMacAndInterfaceId,
                RetrievableValuesEnum.RxSignalStrengthCh2AppendMacAndInterfaceId
            };

            DeviceSpecificOid[] deviceSpecificOids = new DeviceSpecificOid[valuesToQuery.Length];
            DeviceSpecificOid[] oidValues;
            if (!this.OidLookup.TryGetValues(out oidValues, valuesToQuery))
            {
                log.Warn($"Not even one supported OID has been found for getting the stream-specific RX level. RxSignalStrength for device '{this.DeviceAddress}', interface ID {this.InterfaceId}, cannot be retrieved.");
                this.RxSignalStrengthBacking = double.NegativeInfinity;
                return true;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var clientSpecificOids = oidValues.Where(oid => oid != null).Select(oid => {
                var interfaceTypeOid = oid.Oid + this.RemoteMacString.HexStringToByteArray().ToDottedDecimalOid() + new Oid(new int[] { this.InterfaceId.Value });

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

            var interfactTypeOid = interfaceIdRootOid.Oid + this.RemoteMacString.HexStringToByteArray().ToDottedDecimalOid() + new Oid(new int[] { this.InterfaceId.Value });

            this.LinkUptimeBacking = this.LowerSnmpLayer.QueryAsTimeSpan(interfactTypeOid, "wireless peer info, link uptime").Value;

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }
    }
}
