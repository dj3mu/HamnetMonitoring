using System;
using System.Text;

namespace SnmpAbstraction
{
    internal abstract class LazyLoadingGenericWirelessPeerInfo : LazyHamnetSnmpQuerierResultBase, IWirelessPeerInfo
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Indicating whether <see cref="TxSignalStrengthBacking" /> has already been populated.
        /// </summary>
        private bool txSignalStrengthPopulated = false;
        
        /// <summary>
        /// Indicating whether <see cref="RxSignalStrengthBacking" /> has already been populated.
        /// </summary>
        private bool rxSignalStrengthPopulated = false;
        
        /// <summary>
        /// Indicating whether <see cref="LinkUptimeBacking" /> has already been populated.
        /// </summary>
        private bool linkUptimePopulated = false;

        /// <summary>
        /// Indicating whether <see cref="CcqBacking" /> has already been populated.
        /// </summary>
        private bool ccqPopulated = false;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="macAddress">The MAC address of the peer (serves as index in OIDs for MikroTik devices).</param>
        /// <param name="interfaceId">The ID of the interface (i.e. the value to append to interface-specific OIDs).</param>
        /// <param name="isAccessPoint">Value indicating whether the device proving the peer info is an access point or a client.</param>
        /// <param name="numberOfClients">The number of clients that are connected to this AP when in AP mode. null if not an AP or not available.</param>
        protected LazyLoadingGenericWirelessPeerInfo(
            ISnmpLowerLayer lowerSnmpLayer,
            IDeviceSpecificOidLookup oidLookup,
            string macAddress,
            int? interfaceId,
            bool? isAccessPoint,
            int? numberOfClients)
            : base(lowerSnmpLayer)
        {
            this.OidLookup = oidLookup;
            this.PeerMac = macAddress;
            this.InterfaceId = interfaceId;
            this.IsAccessPoint = isAccessPoint;
            this.NumberOfClients = numberOfClients;
        }

        /// <inheritdoc />
        public string RemoteMacString => this.PeerMac;

        /// <inheritdoc />
        public int? InterfaceId { get; }

        /// <inheritdoc />
        public double TxSignalStrength
        {
            get
            {
                this.PopulateTxSignalStrength();

                return this.TxSignalStrengthBacking;
            }
        }

        /// <inheritdoc />
        public double RxSignalStrength
        {
            get
            {
                this.PopulateRxSignalStrength();

                return this.RxSignalStrengthBacking;
            }
        }

        /// <inheritdoc />
        public TimeSpan LinkUptime
        {
            get
            {
                this.PopulateLinkUptime();

                return this.LinkUptimeBacking;
            }
        }

        /// <inheritdoc />
        public double? Ccq
        {
            get
            {
                this.PopulateCcq();

                return this.CcqBacking;
            }
        }

        /// <inheritdoc />
        public virtual bool? IsAccessPoint { get; }

        /// <inheritdoc />
        public override void ForceEvaluateAll()
        {
            this.PopulateLinkUptime();
            this.PopulateRxSignalStrength();
            this.PopulateTxSignalStrength();
            this.PopulateCcq();
        }

        /// <inheritdoc />
        public int? NumberOfClients { get; }

        /// <summary>
        /// Gets or sets the backing property (accessible by inheriting classes) for TX signal strength.
        /// </summary>
        protected double TxSignalStrengthBacking { get; set; } = double.NaN;

        /// <summary>
        /// Gets or sets the backing property (accessible by inheriting classes) for RX signal strength.
        /// </summary>
        protected double RxSignalStrengthBacking { get; set; } = double.NaN;

        /// <summary>
        /// Gets or sets the backing property (accessible by inheriting classes) for link uptime.
        /// </summary>
        protected TimeSpan LinkUptimeBacking { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Gets or sets the backing property (accessible by inheriting classes) for CCQ.
        /// </summary>
        protected double? CcqBacking { get; set; } = null;

        /// <summary>
        /// Gets the OID lookup table.
        /// </summary>
        /// <value></value>
        protected IDeviceSpecificOidLookup OidLookup { get; }

        /// <summary>
        /// Gets the peer's MAC address.
        /// </summary>
        protected virtual string PeerMac { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder returnBuilder = new StringBuilder(128);

            returnBuilder.Append("Peer ").Append(this.PeerMac).AppendLine(":");
            returnBuilder.Append("  - Mode           : ").AppendLine(this.IsAccessPoint.HasValue ? (this.IsAccessPoint.Value ? "AP" : "Client") : "not available");
            returnBuilder.Append("  - Num clients    : ").AppendLine(this.NumberOfClients.HasValue ? this.NumberOfClients.Value.ToString() : "not available");
            returnBuilder.Append("  - On interface ID: ").AppendLine(this.InterfaceId.HasValue ? this.InterfaceId.Value.ToString() : "not available");
            returnBuilder.Append("  - Link Uptime    : ").AppendLine(this.linkUptimePopulated ? this.LinkUptimeBacking.ToString() : "not available");
            returnBuilder.Append("  - RX signal [dBm]: ").AppendLine(this.rxSignalStrengthPopulated ? this.RxSignalStrengthBacking.ToString("0.0 dBm") : "not available");
            returnBuilder.Append("  - TX signal [dBm]: ").Append(this.txSignalStrengthPopulated ? this.TxSignalStrengthBacking.ToString("0.0 dBm") : "not available");
            returnBuilder.Append("  - CCQ            : ").Append(this.ccqPopulated ? this.Ccq?.ToString("0.0 dBm") ?? "not reported" : "not available");

            return returnBuilder.ToString();
        }

        /// <summary>
        /// To be implemented by deriving classes in order to actually populate the link uptime.
        /// </summary>
        protected abstract bool RetrieveLinkUptime();

        /// <summary>
        /// To be implemented by deriving classes in order to actually populate the RX signal strength.
        /// </summary>
        protected abstract bool RetrieveRxSignalStrength();

        /// <summary>
        /// To be implemented by deriving classes in order to actually populate the TX signal strength.
        /// </summary>
        protected abstract bool RetrieveTxSignalStrength();

        /// <summary>
        /// To be implemented by deriving classes in order to actually populate the CCQ value.
        /// </summary>
        protected abstract bool RetrieveCcq();

        /// <summary>
        /// Layz-loading of TX signal strength.
        /// </summary>
        private void PopulateTxSignalStrength()
        {
            if (this.txSignalStrengthPopulated)
            {
                return;
            }

            this.txSignalStrengthPopulated = this.RetrieveTxSignalStrength();
        }

        /// <summary>
        /// Layz-loading of CCQ.
        /// </summary>
        private void PopulateCcq()
        {
            if (this.ccqPopulated)
            {
                return;
            }

            this.ccqPopulated = this.RetrieveCcq();
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

             this.rxSignalStrengthPopulated = this.RetrieveRxSignalStrength();
        }

        /// <summary>
        /// Layz-loading of link uptime.
        /// </summary>
        private void PopulateLinkUptime()
        {
            if (this.linkUptimePopulated)
            {
                return;
            }

            this.linkUptimePopulated = this.RetrieveLinkUptime();
        }
    }
}
