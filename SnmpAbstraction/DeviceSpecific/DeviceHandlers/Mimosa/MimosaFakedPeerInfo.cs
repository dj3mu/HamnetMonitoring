using System;
using System.Diagnostics;
using System.Linq;
using SnmpSharpNet;
 
namespace SnmpAbstraction
{
    internal class MimosaFakedPeerInfo : LazyLoadingGenericWirelessPeerInfo
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
        public MimosaFakedPeerInfo(
            ISnmpLowerLayer lowerSnmpLayer,
            IDeviceSpecificOidLookup oidLookup)
            : base(lowerSnmpLayer, oidLookup, "00:00:00:00:00:00", null, null, 1)
        {
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration => this.localQueryDuration;

        /// <inheritdoc />
        protected override bool RetrieveTxSignalStrength()
        {
            this.TxSignalStrengthBacking = -99.9;

            return true;

            // var valueToQuery = RetrievableValuesEnum.TxSignalStrengthAppendMacAndInterfaceId;
            // if (!this.OidLookup.TryGetValue(valueToQuery, out DeviceSpecificOid interfaceIdRootOid) || interfaceIdRootOid.Oid.IsNull)
            // {
            //     log.Warn($"Failed to obtain OID for '{valueToQuery}'");
            //     this.TxSignalStrengthBacking = double.NaN;
            //     return true;
            // }

            // Stopwatch durationWatch = Stopwatch.StartNew();

            // var interfactTypeOid = interfaceIdRootOid.Oid + this.RemoteMacString.HexStringToByteArray().ToDottedDecimalOid() + new Oid(new int[] { this.InterfaceId.Value });

            // this.TxSignalStrengthBacking = this.LowerSnmpLayer.QueryAsInt(interfactTypeOid, "wireless peer info, TX signal strength");

            // durationWatch.Stop();

            // this.RecordCachableOid(CachableValueMeanings.WirelessTxSignalStrength, interfactTypeOid);

            // this.localQueryDuration += durationWatch.Elapsed;

            // return true;
        }

        /// <inheritdoc />
        protected override bool RetrieveRxSignalStrength()
        {
            this.RxSignalStrengthBacking = -99.8;

            return true;

            // Stopwatch durationWatch = Stopwatch.StartNew();

            // if (this.OidLookup.TryGetValue(RetrievableValuesEnum.RxSignalStrengthImmediateOid, out DeviceSpecificOid singleOid) && !singleOid.Oid.IsNull)
            // {
            //     this.RxSignalStrengthBacking = this.LowerSnmpLayer.QueryAsInt(singleOid.Oid, "RSSI single value (RxSignalStrengthImmediateOid)");
            //     this.RecordCachableOid(CachableValueMeanings.WirelessRxSignalStrength, singleOid.Oid);
            // }
            // else if (this.OidLookup.TryGetValue(RetrievableValuesEnum.RxSignalStrengthCh0AppendInterfaceId, out singleOid) && !singleOid.Oid.IsNull)
            // {
            //     var queryOid = singleOid.Oid + new Oid(new int[] { this.InterfaceId.Value });
            //     this.RxSignalStrengthBacking = this.LowerSnmpLayer.QueryAsInt(queryOid, "RSSI single value with Interface ID (RxSignalStrengthCh0AppendInterfaceId)");
            //     this.RecordCachableOid(CachableValueMeanings.WirelessRxSignalStrength, queryOid);
            // }
            // else
            // {
            //     var valuesToQuery = new[]
            //     {
            //         RetrievableValuesEnum.RxSignalStrengthCh0AppendMacAndInterfaceId,
            //         RetrievableValuesEnum.RxSignalStrengthCh1AppendMacAndInterfaceId,
            //         RetrievableValuesEnum.RxSignalStrengthCh2AppendMacAndInterfaceId
            //     };

            //     if (!this.OidLookup.TryGetValues(out DeviceSpecificOid[] oidValues, valuesToQuery))
            //     {

            //         log.Warn($"Not even one supported OID has been found for getting the stream-specific RX level. RxSignalStrength for device '{this.DeviceAddress}', interface ID {this.InterfaceId}, cannot be retrieved.");
            //         this.RxSignalStrengthBacking = double.NegativeInfinity;
            //         return true;
            //     }

            //     var clientSpecificOids = oidValues.Where(oid => oid != null).Select(oid =>
            //     {
            //         var interfaceTypeOid = oid.Oid + this.RemoteMacString.HexStringToByteArray().ToDottedDecimalOid() + new Oid(new int[] { this.InterfaceId.Value });

            //         return interfaceTypeOid;
            //     });

            //     var queryResults = this.LowerSnmpLayer.QueryAsInt(clientSpecificOids, "wireless peer info, RX signal strength");

            //     // Note: As the SNMP cannot return -infinity MikroTik devices return 0.
            //     //       Hence we effectively skip 0 values here assuming that stream is not in use.
            //     var valueToSet = queryResults.Values.Where(v => v != 0).DecibelLogSum();

            //     if (double.IsNegativeInfinity(valueToSet))
            //     {
            //         // if the device didn't return anyhting useful until here, we do a final try with RxSignalStrengthApAppendMacAndInterfaceId
            //         if (this.OidLookup.TryGetValue(RetrievableValuesEnum.RxSignalStrengthApAppendMacAndInterfaceId, out singleOid) && !singleOid.Oid.IsNull)
            //         {
            //             var interfaceTypeOid = singleOid.Oid + this.RemoteMacString.HexStringToByteArray().ToDottedDecimalOid() + new Oid(new int[] { this.InterfaceId.Value });

            //             valueToSet = this.LowerSnmpLayer.QueryAsInt(interfaceTypeOid, "RSSI single value (SignalStrengthApAppendMacAndInterfaceId)");
            //             this.RecordCachableOid(CachableValueMeanings.WirelessRxSignalStrength, interfaceTypeOid);
            //         }
            //     }
            //     else
            //     {
            //         // we have valid per-stream values
            //         this.RecordCachableOids(CachableValueMeanings.WirelessRxSignalStrength, clientSpecificOids);
            //     }

            //     this.RxSignalStrengthBacking = valueToSet;
            // }

            // durationWatch.Stop();

            // this.localQueryDuration += durationWatch.Elapsed;

            // return true;
        }

        /// <inheritdoc />
        protected override bool RetrieveLinkUptime()
        {
            this.LinkUptimeBacking = TimeSpan.Zero;

            return true;

            // var valueToQuery = RetrievableValuesEnum.LinkUptimeAppendMacAndInterfaceId;
            // if (!this.OidLookup.TryGetValue(valueToQuery, out DeviceSpecificOid interfaceIdRootOid) || interfaceIdRootOid.Oid.IsNull)
            // {
            //     log.Warn($"Failed to obtain OID for '{valueToQuery}'");
            //     this.LinkUptimeBacking = TimeSpan.Zero;
            //     return true;
            // }

            // Stopwatch durationWatch = Stopwatch.StartNew();

            // var interfactTypeOid = interfaceIdRootOid.Oid + this.RemoteMacString.HexStringToByteArray().ToDottedDecimalOid() + new Oid(new int[] { this.InterfaceId.Value });

            // this.LinkUptimeBacking = this.LowerSnmpLayer.QueryAsTimeSpan(interfactTypeOid, "wireless peer info, link uptime") ?? TimeSpan.Zero;

            // durationWatch.Stop();

            // this.localQueryDuration += durationWatch.Elapsed;

            // this.RecordCachableOid(CachableValueMeanings.WirelessLinkUptime, interfactTypeOid);

            // return true;
        }

        /// <inheritdoc />
        protected override bool RetrieveCcq()
        {
            this.CcqBacking = null;

            return true;

            // if (this.NumberOfClients.HasValue && (this.NumberOfClients.Value > 1))
            // {
            //     // MTik special behaviour: If we have a valid number of clients but it is more than one,
            //     // we know that we cannot retrieve any meaningful CCQ value.
            //     // Hence we behave as if CCQ is not available at all.
            //     log.Info($"MiktoTik Device has {this.DeviceAddress} has {this.NumberOfClients.Value} clients and hence does not provide any meaningful CCQ");
            //     this.RecordCachableOid(CachableValueMeanings.Ccq, new Oid("0"));
            //     this.CcqBacking = null;
            //     return true;
            // }

            // var valueToQuery = RetrievableValuesEnum.OverallCcqAppendInterfaceId;
            // if (!this.OidLookup.TryGetValue(valueToQuery, out DeviceSpecificOid interfaceIdRootOid) || interfaceIdRootOid.Oid.IsNull)
            // {
            //     log.Warn($"Failed to obtain OID for '{valueToQuery}'");
            //     this.RecordCachableOid(CachableValueMeanings.Ccq, new Oid("0"));
            //     this.CcqBacking = null;
            //     return true;
            // }

            // Stopwatch durationWatch = Stopwatch.StartNew();

            // var interfactTypeOid = interfaceIdRootOid.Oid + new Oid(new int[] { this.InterfaceId.Value });

            // try
            // {
            //     this.CcqBacking = Convert.ToDouble(this.LowerSnmpLayer.QueryAsInt(interfactTypeOid, "wireless peer info, CCQ"));

            //     this.RecordCachableOid(CachableValueMeanings.Ccq, interfactTypeOid);
            // }
            // catch(HamnetSnmpException)
            // {
            //     log.Debug($"Ignoring HamnetSnmpException during CCQ retrieval");
            //     this.RecordCachableOid(CachableValueMeanings.Ccq, new Oid("0"));
            //     this.CcqBacking = null;
            // }

            // durationWatch.Stop();

            // this.localQueryDuration += durationWatch.Elapsed;

            // return true;
        }
    }
}
