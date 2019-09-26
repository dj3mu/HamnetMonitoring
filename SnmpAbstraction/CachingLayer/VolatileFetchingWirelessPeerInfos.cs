using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class VolatileFetchingWirelessPeerInfos : IWirelessPeerInfos
    {
        private readonly IWirelessPeerInfos underlyingWirelessPeerInfo;
        
        private readonly ISnmpLowerLayer lowerLayer;

        private readonly IReadOnlyDictionary<CachableValueMeanings, ICachableOid> cachableOids;

        private TimeSpan queryDurationBacking = TimeSpan.Zero;

        /// <summary>
        /// Construct for a lower layer and a cache data set (from which we return the non-volatile data).
        /// </summary>
        public VolatileFetchingWirelessPeerInfos(IWirelessPeerInfos wirelessPeerInfos, IReadOnlyDictionary<CachableValueMeanings, ICachableOid> cachableOids, ISnmpLowerLayer lowerLayer, object syncRoot)
        {
            this.lowerLayer = lowerLayer ?? throw new System.ArgumentNullException(nameof(lowerLayer), "lower layer is null");
            this.SyncRoot = syncRoot ?? throw new ArgumentNullException(nameof(syncRoot));
            this.underlyingWirelessPeerInfo = wirelessPeerInfos ?? throw new System.ArgumentNullException(nameof(wirelessPeerInfos), "underlying wireless peer info is null");
            this.cachableOids = cachableOids ?? throw new ArgumentNullException(nameof(cachableOids), "Cachable OIDs lookup is null");

            this.Details = wirelessPeerInfos.Details.Select(underlyingPeerInfo => new VolatileFetchingWirelessPeerInfo(underlyingPeerInfo, this.cachableOids, this.lowerLayer, this.SyncRoot)).ToList();
        }

        /// <inheritdoc />
        public IReadOnlyList<IWirelessPeerInfo> Details { get; }

        /// <inheritdoc />
        public IpAddress DeviceAddress => this.underlyingWirelessPeerInfo.DeviceAddress;

        /// <inheritdoc />
        public string DeviceModel => this.underlyingWirelessPeerInfo.DeviceModel;

        /// <summary>
        /// Gets the thread synchronization object.
        /// </summary>
        public object SyncRoot { get; }

        /// <inheritdoc />
        public TimeSpan QueryDuration => this.queryDurationBacking;

        /// <inheritdoc />
        public void ForceEvaluateAll()
        {
            // TODO: Trigger fetching of volatile data
        }

        /// <inheritdoc />
        public IEnumerator<IWirelessPeerInfo> GetEnumerator()
        {
            return this.Details.GetEnumerator();
        }

        /// <inheritdoc />
        public string ToConsoleString()
        {
            StringBuilder returnBuilder = new StringBuilder((this.Details?.Count ?? 1) * 128);

            returnBuilder.Append("Peer Infos:");

            if (this.Details == null)
            {
                returnBuilder.Append(" Not yet retrieved");
            }
            else
            {
                foreach (var item in this.Details)
                {
                    returnBuilder.AppendLine().AppendLine(SnmpAbstraction.IndentLines(item.ToConsoleString()));
                }
            }

            return returnBuilder.ToString();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Details.GetEnumerator();
        }
    }
}