using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class VolatileFetchingWirelessPeerInfo : IWirelessPeerInfo
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IWirelessPeerInfo underlyingPeerInfo;

        private readonly IReadOnlyDictionary<CachableValueMeanings, ICachableOid> cachableOids;

        private readonly ISnmpLowerLayer lowerLayer;

        private TimeSpan queryDurationBacking = TimeSpan.Zero;

        public VolatileFetchingWirelessPeerInfo(IWirelessPeerInfo underlyingPeerInfo, IReadOnlyDictionary<CachableValueMeanings, ICachableOid> cachableOids, ISnmpLowerLayer lowerLayer, object syncRoot)
        {
            this.underlyingPeerInfo = underlyingPeerInfo ?? throw new System.ArgumentNullException(nameof(underlyingPeerInfo));
            this.cachableOids = cachableOids ?? throw new System.ArgumentNullException(nameof(cachableOids));
            this.lowerLayer = lowerLayer ?? throw new System.ArgumentNullException(nameof(lowerLayer));
            this.SyncRoot = syncRoot ?? throw new ArgumentNullException(nameof(syncRoot));
        }

        /// <inheritdoc />
        public string RemoteMacString => this.underlyingPeerInfo.RemoteMacString;

        /// <inheritdoc />
        public int? InterfaceId => this.underlyingPeerInfo.InterfaceId;

        /// <inheritdoc />
        public double TxSignalStrength
        {
            get
            {
                var neededValue = CachableValueMeanings.WirelessTxSignalStrength;
                ICachableOid queryOid = null;
                lock(this.SyncRoot)
                {
                    if (!this.cachableOids.TryGetValue(neededValue, out queryOid))
                    {
                        throw new HamnetSnmpException($"Cannot obtain an OID for querying {neededValue} from {this.DeviceAddress} ({this.DeviceModel})");
                    }
                }
                
                if (queryOid.IsSingleOid && (queryOid.Oid.First() == 0))
                {
                    // value is not available for this device
                    return double.NaN;
                }

                var queryResults = this.lowerLayer.QueryAsInt(queryOid.Oids, "TX Signal Strength");

                // Note: As the SNMP cannot return -infinity MikroTik devices return 0.
                //       Hence we effectively skip 0 values here assuming that stream is not in use.
                var txSignalStrength = queryResults.Values.Where(v => v != 0).DecibelLogSum();

                return txSignalStrength;
            }
        }

        /// <inheritdoc />
        public double RxSignalStrength
        {
            get
            {
                var neededValue = CachableValueMeanings.WirelessRxSignalStrength;
                ICachableOid queryOid = null;
                lock(this.SyncRoot)
                {
                    if (!this.cachableOids.TryGetValue(neededValue, out queryOid))
                    {
                        throw new HamnetSnmpException($"Cannot obtain an OID for querying {neededValue} from {this.DeviceAddress} ({this.DeviceModel})");
                    }
                }
                
                if (queryOid.IsSingleOid && (queryOid.Oid.First() == 0))
                {
                    // value is not available for this device
                    return double.NaN;
                }

                var queryResults = this.lowerLayer.QueryAsInt(queryOid.Oids, "RX RSSI");

                // Note: As the SNMP cannot return -infinity MikroTik devices return 0.
                //       Hence we effectively skip 0 values here assuming that stream is not in use.
                var rxSignalStrength = queryResults.Values.Where(v => v != 0).DecibelLogSum();

                return rxSignalStrength;
            }
        }

        /// <inheritdoc />
        public TimeSpan LinkUptime
        {
            get
            {
                var neededValue = CachableValueMeanings.WirelessLinkUptime;
                ICachableOid queryOid = null;
                lock(this.SyncRoot)
                {
                    if (!this.cachableOids.TryGetValue(neededValue, out queryOid))
                    {
                        throw new HamnetSnmpException($"Cannot obtain an OID for querying {neededValue} from {this.DeviceAddress} ({this.DeviceModel})");
                    }
                }
                
                if (queryOid.IsSingleOid && (queryOid.Oid.First() == 0))
                {
                    // value is not available for this device
                    return TimeSpan.MinValue;
                }

                var queryResult = this.lowerLayer.QueryAsTimeSpan(queryOid.Oid, "Link Uptime");

                return queryResult ?? TimeSpan.MinValue;
            }
        }

        /// <inheritdoc />
        public bool? IsAccessPoint => this.underlyingPeerInfo.IsAccessPoint;

        /// <inheritdoc />
        public IpAddress DeviceAddress => this.underlyingPeerInfo.DeviceAddress;

        /// <inheritdoc />
        public string DeviceModel => this.underlyingPeerInfo.DeviceModel;

        /// <inheritdoc />
        public TimeSpan QueryDuration => this.queryDurationBacking;

        /// <inheritdoc />
        public IReadOnlyDictionary<CachableValueMeanings, ICachableOid> Oids => this.underlyingPeerInfo.Oids;

        /// <summary>
        /// Gets the thread synchronization object.
        /// </summary>
        public object SyncRoot { get; }

        /// <inheritdoc />
        public void ForceEvaluateAll()
        {
            // NOP here - the volatile values are supposed to be queried every time
        }

        /// <inheritdoc />
        public TimeSpan GetQueryDuration()
        {
            return this.queryDurationBacking;
        }

        /// <inheritdoc />
        public string ToConsoleString()
        {
            StringBuilder returnBuilder = new StringBuilder(128);

            returnBuilder.Append("Peer ").Append(this.RemoteMacString).AppendLine(":");
            returnBuilder.Append("  - Mode           : ").AppendLine(this.IsAccessPoint.HasValue ? (this.IsAccessPoint.Value ? "AP" : "Client") : "not available");
            returnBuilder.Append("  - On interface ID: ").AppendLine(this.InterfaceId.HasValue ? this.InterfaceId.Value.ToString() : "not available");
            returnBuilder.Append("  - Link Uptime    : not available");
            returnBuilder.Append("  - RX signal [dBm]: ").AppendLine(this.RxSignalStrength.ToString("0.0 dBm"));
            returnBuilder.Append("  - TX signal [dBm]: ").Append(this.TxSignalStrength.ToString("0.0 dBm"));

            return returnBuilder.ToString();
        }
    }
}