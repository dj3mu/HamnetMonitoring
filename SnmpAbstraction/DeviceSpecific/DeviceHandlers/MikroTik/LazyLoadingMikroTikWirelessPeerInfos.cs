using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Worker class for lazy-loading interface details.
    /// /// </summary>
    internal class LazyLoadingMikroTikWirelessPeerInfos : LazyHamnetSnmpQuerierResultBase, IWirelessPeerInfos
    {
        /// <summary>
        /// The OID lookup table.
        /// </summary>
        private readonly DeviceSpecificOidLookup oidLookup;

        /// <summary>
        /// Backing field for the lazy access to the peer infos.
        /// </summary>
        /// <remarks><c>null</c> means not initialized, empty list means no peers found.</remarks>
        private List<IWirelessPeerInfo> peerInfosBacking = null;

        /// <summary>
        /// Field indicating whether the peer infos have been queried.<br/>
        /// The field is necessary as, by interface definition, null can be a valid
        /// return value if peer infos are not available.
        /// </summary>
        private bool peerInfosQueried = false;

        /// <summary>
        /// Field to sum up the query duration on each "Populate" call.
        /// </summary>
        private TimeSpan localQueryDuration = TimeSpan.Zero;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        public LazyLoadingMikroTikWirelessPeerInfos(ISnmpLowerLayer lowerSnmpLayer, DeviceSpecificOidLookup oidLookup)
            : base(lowerSnmpLayer)
        {
            this.oidLookup = oidLookup;
        }

        /// <inheritdoc />
        public IReadOnlyList<IWirelessPeerInfo> Details
        {
            get
            {
                this.PopulatePeerInfos();

                return this.peerInfosBacking;
            }
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration
        {
            get
            {
                if (this.peerInfosBacking == null)
                {
                    return TimeSpan.Zero;
                }

                return this.peerInfosBacking.Aggregate(this.localQueryDuration, (value, detail) => value += detail.QueryDuration);
            }
        }


        /// <inheritdoc />
        public override void ForceEvaluateAll()
        {
            this.PopulatePeerInfos();
            foreach (var item in this.peerInfosBacking)
            {
                item.ForceEvaluateAll();
            }
        }

        /// <inheritdoc />
        public IEnumerator<IWirelessPeerInfo> GetEnumerator()
        {
            return this.Details.GetEnumerator();
        }

        /// <inheritdoc />
        public override string ToTextString()
        {
            StringBuilder returnBuilder = new StringBuilder((this.peerInfosBacking?.Count ?? 1) * 128);

            returnBuilder.Append("Peer Infos:");

            if (!this.peerInfosQueried)
            {
                returnBuilder.Append(" Not yet queried");
            }
            else
            {
                if (this.peerInfosBacking == null)
                {
                    returnBuilder.Append(" Not yet retrieved");
                }
                else
                {
                    foreach (var item in this.peerInfosBacking)
                    {
                        returnBuilder.AppendLine().AppendLine(SnmpAbstraction.IndentLines(item.ToConsoleString()));
                    }
                }
            }

            return returnBuilder.ToString();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Details.GetEnumerator();
        }

        /// <summary>
        /// Populates the interface details list.
        /// </summary>
        private void PopulatePeerInfos()
        {
            if (this.peerInfosQueried)
            {
                return;
            }

            // Note: The MAC address serves as an index in nested OIDs for MikroTik devices.
            //       So this way, we get the amount of peers as well as an index to them.
            var valueToQuery = RetrievableValuesEnum.WlanRemoteMacAddressWalkRoot;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.oidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                this.peerInfosBacking = null;
                this.peerInfosQueried = true;
                return;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfaceVbs = this.LowerSnmpLayer.DoWalk(interfaceIdRootOid.Oid, 0);

            durationWatch.Stop();

            this.localQueryDuration = durationWatch.Elapsed;

            this.peerInfosBacking = new List<IWirelessPeerInfo>();

            foreach (Vb item in interfaceVbs)
            {
                this.peerInfosBacking.Add(
                    new LazyLoadingMikroTikWirelessPeerInfo(
                        this.LowerSnmpLayer,
                        this.oidLookup,
                        item.Value.ToString().Replace(' ', ':'),
                        Convert.ToInt32(item.Oid[item.Oid.Length - 1]) // last element of OID contains the interface ID on which this peer is connected
                    ));
            }

            this.peerInfosQueried = true;
        }
    }
}
