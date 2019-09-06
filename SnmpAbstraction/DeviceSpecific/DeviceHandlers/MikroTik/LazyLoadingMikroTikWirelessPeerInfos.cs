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
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The OID lookup table.
        /// </summary>
        private readonly IDeviceSpecificOidLookup oidLookup;

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
        public LazyLoadingMikroTikWirelessPeerInfos(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup)
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

            Stopwatch durationWatch = Stopwatch.StartNew();

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

            var interfaceVbs = this.LowerSnmpLayer.DoWalk(interfaceIdRootOid.Oid, 0);

            durationWatch.Stop();

            this.localQueryDuration = durationWatch.Elapsed;

            this.peerInfosBacking = new List<IWirelessPeerInfo>();

            foreach (Vb item in interfaceVbs)
            {
                int interfaceId = Convert.ToInt32(item.Oid[item.Oid.Length - 1]);
                this.peerInfosBacking.Add(
                    new LazyLoadingMikroTikWirelessPeerInfo(
                        this.LowerSnmpLayer,
                        this.oidLookup,
                        item.Value.ToString().Replace(' ', ':'),
                        interfaceId, // last element of OID contains the interface ID on which this peer is connected
                        this.CheckIsAccessPoint(interfaceId)
                    ));
            }

            this.peerInfosQueried = true;
        }

        /// <summary>
        /// Check if the interface of the given ID is an access point or a client.
        /// </summary>
        /// <param name="interfaceId">The interface ID to check.</param>
        /// <returns><c>true</c> if the interface is an access point.</returns>
        private bool? CheckIsAccessPoint(int interfaceId)
        {
            var valueToQuery = RetrievableValuesEnum.WirelessClientCount;
            DeviceSpecificOid wirelessClientCountRootOid;
            if (this.oidLookup.TryGetValue(valueToQuery, out wirelessClientCountRootOid))
            {
                // finally we need to get the count of registered clients
                // if it's 0, this must be a client (this method will only be called if the registration table
                // contains at least one entry)
                var queryOid = (Oid)wirelessClientCountRootOid.Oid.Clone();

                // need to append the interface ID to the client count OID
                queryOid.Add(interfaceId);

                var returnCollection = this.LowerSnmpLayer.Query(queryOid);
                if (returnCollection.Count == 0)
                {
                    log.Warn($"Unable to get AP / client distinction for wireless interface #{interfaceId} of device '{this.LowerSnmpLayer.Address}': Query of OID for '{valueToQuery}' returned empty result");
                    return null;
                }

                var returnValue = returnCollection[queryOid];

                return returnValue.Value.ToInt() > 0;
            }

            log.Warn($"Unable to get AP / client distinction for wireless interface #{interfaceId} of device '{this.LowerSnmpLayer.Address}': No OID for value of type '{valueToQuery}' available");

            return null;
        }
    }
}
