using System;
using System.Diagnostics;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class LazyLoadingMikroTikWirelessPeerInfo : LazyHamnetSnmpQuerierResultBase, IWirelessPeerInfo
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The OID lookup table.
        /// </summary>
        private readonly IDeviceSpecificOidLookup oidLookup;
        
        /// <summary>
        /// The ID of the interface (i.e. the value to append to interface-specific OIDs).
        /// </summary>
        private readonly int interfaceId;

        /// <summary>
        /// The MAC address of the peer (serves as index in OIDs for MikroTik devices).
        /// </summary>
        private readonly string peerMac;

        /// <summary>
        /// Field to sum up the query duration on each "Populate" call.
        /// </summary>
        private TimeSpan localQueryDuration = TimeSpan.Zero;

        /// <summary>
        /// Backing field for TX signal strength.
        /// </summary>
        private double txSignalStrengthBacking = double.NaN;

        /// <summary>
        /// Indicating whether <see cref="txSignalStrengthBacking" /> has already been populated.
        /// </summary>
        private bool txSignalStrengthPopulated = false;
        
        /// <summary>
        /// Backing field for RX signal strength.
        /// </summary>
        private double rxSignalStrengthBacking = double.NaN;

        /// <summary>
        /// Indicating whether <see cref="rxSignalStrengthBacking" /> has already been populated.
        /// </summary>
        private bool rxSignalStrengthPopulated = false;
        
        private TimeSpan linkUptimeBacking = TimeSpan.Zero;

        /// <summary>
        /// Indicating whether <see cref="linkUptimeBacking" /> has already been populated.
        /// </summary>
        private bool linkUptimePopulated = false;
        
        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="macAddress">The MAC address of the peer (serves as index in OIDs for MikroTik devices).</param>
        /// <param name="interfaceId">The ID of the interface (i.e. the value to append to interface-specific OIDs).</param>
        public LazyLoadingMikroTikWirelessPeerInfo(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup, string macAddress, int interfaceId)
            : base(lowerSnmpLayer)
        {
            this.oidLookup = oidLookup;
            this.peerMac = macAddress;
            this.interfaceId = interfaceId;
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration => this.localQueryDuration;

        /// <inheritdoc />
        public string RemoteMacString => this.peerMac;

        /// <inheritdoc />
        public int InterfaceId => this.interfaceId;

        /// <inheritdoc />
        public double TxSignalStrength
        {
            get
            {
                this.PopulateTxSignalStrength();

                return this.txSignalStrengthBacking;
            }
        }

        /// <inheritdoc />
        public double RxSignalStrength
        {
            get
            {
                this.PopulateRxSignalStrength();

                return this.rxSignalStrengthBacking;
            }
        }

        /// <inheritdoc />
        public TimeSpan LinkUptime
        {
            get
            {
                this.PopulateLinkUptime();

                return this.linkUptimeBacking;
            }
        }

        /// <inheritdoc />
        public override void ForceEvaluateAll()
        {
            this.PopulateLinkUptime();
            this.PopulateRxSignalStrength();
            this.PopulateTxSignalStrength();
        }

        /// <inheritdoc />
        public override string ToTextString()
        {
            StringBuilder returnBuilder = new StringBuilder(128);

            returnBuilder.Append("Peer ").Append(this.peerMac).AppendLine(":");
            returnBuilder.Append("  - On interface ID: ").AppendLine(this.interfaceId.ToString());
            returnBuilder.Append("  - Link Uptime    : ").AppendLine(this.linkUptimePopulated ? this.linkUptimeBacking.ToString() : "Not yet queried");
            returnBuilder.Append("  - RX signal [dBm]: ").AppendLine(this.rxSignalStrengthPopulated ? this.rxSignalStrengthBacking.ToString() : "Not yet queried");
            returnBuilder.Append("  - TX signal [dBm]: ").Append(this.txSignalStrengthPopulated ? this.txSignalStrengthBacking.ToString() : "Not yet queried");
            //returnBuilder.Append("  - MAC : ").Append(this.macAddressStringQueried ? this.macAddressStringBacking?.ToString() : "Not yet queried");

            return returnBuilder.ToString();
        }

        /// <summary>
        /// Layz-loading of TX signal strength.
        /// </summary>
        private void PopulateTxSignalStrength()
        {
            if (this.txSignalStrengthPopulated)
            {
                return;
            }

            var valueToQuery = RetrievableValuesEnum.TxSignalStrengthAppendMacAndInterfaceId;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.oidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                log.Warn($"Failed to obtain OID for '{valueToQuery}'");
                this.txSignalStrengthBacking = double.NegativeInfinity;
                this.txSignalStrengthPopulated = true;
                return;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfactTypeOid = (Oid)interfaceIdRootOid.Oid.Clone();
            interfactTypeOid.Add(this.RemoteMacString.HexStringToByteArray().ToDottedDecimalOid());
            interfactTypeOid.Add(this.InterfaceId);

            this.txSignalStrengthBacking = this.LowerSnmpLayer.QueryAsInt(interfactTypeOid, "wireless peer info, TX signal strength");

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            this.txSignalStrengthPopulated = true;
        }

        /// <summary>
        /// Layz-loading of RX signal strength.
        /// </summary>
        private void PopulateRxSignalStrength()
        {
            if (this.rxSignalStrengthPopulated)
            {
                return;
            }

            var valueToQuery = RetrievableValuesEnum.RxSignalStrengthAppendMacAndInterfaceId;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.oidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                log.Warn($"Failed to obtain OID for '{valueToQuery}'");
                this.rxSignalStrengthBacking = double.NegativeInfinity;
                this.rxSignalStrengthPopulated = true;
                return;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfactTypeOid = (Oid)interfaceIdRootOid.Oid.Clone();
            interfactTypeOid.Add(this.RemoteMacString.HexStringToByteArray().ToDottedDecimalOid());
            interfactTypeOid.Add(this.InterfaceId);

            this.rxSignalStrengthBacking = this.LowerSnmpLayer.QueryAsInt(interfactTypeOid, "wireless peer info, RX signal strength");

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            this.rxSignalStrengthPopulated = true;
        }

        /// <summary>
        /// Layz-loading of link uptime.
        /// </summary>
        private void PopulateLinkUptime()
        {
            if (this.txSignalStrengthPopulated)
            {
                return;
            }

            var valueToQuery = RetrievableValuesEnum.LinkUptimeAppendMacAndInterfaceId;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.oidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                log.Warn($"Failed to obtain OID for '{valueToQuery}'");
                this.linkUptimeBacking = TimeSpan.Zero;
                this.linkUptimePopulated = true;
                return;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfactTypeOid = (Oid)interfaceIdRootOid.Oid.Clone();
            interfactTypeOid.Add(this.RemoteMacString.HexStringToByteArray().ToDottedDecimalOid());
            interfactTypeOid.Add(this.InterfaceId);

            this.linkUptimeBacking = TimeSpan.FromMilliseconds(this.LowerSnmpLayer.QueryAsTimeTicks(interfactTypeOid, "wireless peer info, link uptime").Milliseconds);

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            this.linkUptimePopulated = true;
        }
    }
}
