using System;
using System.Collections.Generic;
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
            const double ValueFactor = 1.0 / 10.0;

            Stopwatch durationWatch = Stopwatch.StartNew();

            if (!this.OidLookup.TryGetValue(RetrievableValuesEnum.TxSignalStrengthAppendChainIndex, out DeviceSpecificOid singleOid) && !singleOid.Oid.IsNull)
            {
                throw new IndexOutOfRangeException($"Cannot obtain OID for '{RetrievableValuesEnum.TxSignalStrengthAppendChainIndex}' which is needed for MIMOSA devices");
            }

            var walkresults = this.LowerSnmpLayer.DoWalk(singleOid.Oid);

            var validOids = new List<Oid>(walkresults.Count);
            var validValues = new List<double>(walkresults.Count);
            foreach (var walkresult in walkresults)
            {
                var dBvalue = walkresult.Value.ToInt() * ValueFactor; // sent as int value of unit dB * 10

                // MIMOSA SPEC: A value of -100 (dB) means this particular RxChain is not a valid entry
                if (dBvalue > -100.0)
                {
                    validOids.Add(walkresult.Oid);
                    validValues.Add(dBvalue);
                }
            }

            this.RecordCachableOids(CachableValueMeanings.WirelessTxSignalStrength, validOids, ValueFactor);

            this.TxSignalStrengthBacking = validValues.DecibelLogSum();

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }

        /// <inheritdoc />
        protected override bool RetrieveRxSignalStrength()
        {
            const double ValueFactor = 1.0 / 10.0;

            Stopwatch durationWatch = Stopwatch.StartNew();

            if (!this.OidLookup.TryGetValue(RetrievableValuesEnum.RxSignalStrengthAppendChainIndex, out DeviceSpecificOid singleOid) && !singleOid.Oid.IsNull)
            {
                throw new IndexOutOfRangeException($"Cannot obtain OID for '{RetrievableValuesEnum.RxSignalStrengthAppendChainIndex}' which is needed for MIMOSA devices");
            }

            var walkresults = this.LowerSnmpLayer.DoWalk(singleOid.Oid);

            var validOids = new List<Oid>(walkresults.Count);
            var validValues = new List<double>(walkresults.Count);
            foreach (var walkresult in walkresults)
            {
                var dBvalue = walkresult.Value.ToInt() * ValueFactor; // sent as int value of unit dB * 10

                // MIMOSA SPEC: A value of -100 (dB) means this particular RxChain is not a valid entry
                if (dBvalue > -100.0)
                {
                    validOids.Add(walkresult.Oid);
                    validValues.Add(dBvalue);
                }
            }

            this.RecordCachableOids(CachableValueMeanings.WirelessRxSignalStrength, validOids, ValueFactor);

            if (validValues.Count > 0)
            {
                this.RxSignalStrengthBacking = validValues.DecibelLogSum();
            }
            else
            {
                this.RxSignalStrengthBacking = double.NaN;
            }

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }

        /// <inheritdoc />
        protected override bool RetrieveLinkUptime()
        {
            Stopwatch durationWatch = Stopwatch.StartNew();

            if (!this.OidLookup.TryGetValue(RetrievableValuesEnum.LinkUptimeDirectValue, out DeviceSpecificOid singleOid) && !singleOid.Oid.IsNull)
            {
                throw new IndexOutOfRangeException($"Cannot obtain OID for '{RetrievableValuesEnum.LinkUptimeDirectValue}' which is needed for MIMOSA devices");
            }


            var uptime = this.LowerSnmpLayer.QueryAsTimeSpan(singleOid.Oid, "MIMOSA Uptime");

            if (uptime.HasValue)
            {
                this.LinkUptimeBacking = uptime.Value;
                this.RecordCachableOid(CachableValueMeanings.WirelessLinkUptime, singleOid.Oid);
            }
            else
            {
                log.Warn("No valid uptime could be retrieved for MIMOSA device");
            }

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }

        /// <inheritdoc />
        protected override bool RetrieveCcq()
        {
            return false;
        }
    }
}
