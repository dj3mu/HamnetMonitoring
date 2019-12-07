using System;
using System.Net;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Container for <see cref="ILinkDetail" />.
    /// </summary>
    internal class LinkDetail : HamnetSnmpQuerierResultBase, ILinkDetail
    {
        /// <summary>
        /// The tuple of the two link partners
        /// </summary>
        private LinkRelatedResultCollection linkRelatedResultCollection;

        /// <summary>
        /// Construct from all paraters.
        /// </summary>
        /// <param name="address">The address of the side #1 of this link detail.</param>
        /// <param name="linkRelatedResultCollection">The collection of interface and wireless details for sides #1 and #2.</param>
        public LinkDetail(IpAddress address, LinkRelatedResultCollection linkRelatedResultCollection)
            : base(address, $"Side #1: {linkRelatedResultCollection.InterfaceDetail1.DeviceModel}, Side #2: {linkRelatedResultCollection.InterfaceDetail2.DeviceModel}", TimeSpan.Zero)
        {
            this.linkRelatedResultCollection = linkRelatedResultCollection;
            if ((this.linkRelatedResultCollection?.WirelessPeerInfo1?.IsAccessPoint != null)
                && (this.linkRelatedResultCollection.WirelessPeerInfo1.IsAccessPoint.HasValue))
            {
                this.SideOfAccessPoint = this.linkRelatedResultCollection.WirelessPeerInfo1.IsAccessPoint.Value ? 1 : 2;
            }

            // heuristics to detect a usable link uptime
            // contained in both WirelessPeerInfo but e.g. Ubnt AirOS 5 doesn't provide it
            // in which case we can see if we can get it from the "other" side
            if ((this.linkRelatedResultCollection?.WirelessPeerInfo2?.LinkUptime ?? TimeSpan.Zero) == TimeSpan.Zero)
            {
                if ((this.linkRelatedResultCollection?.WirelessPeerInfo1?.LinkUptime ?? TimeSpan.Zero) == TimeSpan.Zero)
                {
                    this.LinkUptime = TimeSpan.Zero;
                }
                else
                {
                    this.LinkUptime = this.linkRelatedResultCollection.WirelessPeerInfo1.LinkUptime;
                }
            }
            else
            {
                this.LinkUptime = this.linkRelatedResultCollection.WirelessPeerInfo2.LinkUptime;
            }

            // heuristic to take CCQ from either side
            this.Ccq = this.linkRelatedResultCollection.WirelessPeerInfo1.Ccq ?? this.linkRelatedResultCollection.WirelessPeerInfo2.Ccq;
        }

        /// <inheritdoc />
        public string MacString1 => this.linkRelatedResultCollection?.InterfaceDetail1?.MacAddressString;

        /// <inheritdoc />
        public string MacString2 => this.linkRelatedResultCollection?.InterfaceDetail2?.MacAddressString;

        /// <inheritdoc />
        public double RxLevel1at2 => this.linkRelatedResultCollection?.WirelessPeerInfo2?.RxSignalStrength ?? double.NaN;

        /// <inheritdoc />
        public double RxLevel2at1 => this.linkRelatedResultCollection?.WirelessPeerInfo1?.RxSignalStrength ?? double.NaN;

        /// <inheritdoc />
        public override TimeSpan QueryDuration => this.linkRelatedResultCollection?.TotalQueryDuration ?? TimeSpan.Zero;

        /// <inheritdoc />
        public TimeSpan LinkUptime { get; }

        /// <inheritdoc />
        public int? SideOfAccessPoint { get; } = null;

        /// <inheritdoc />
        public IPAddress Address1 => (IPAddress)this.linkRelatedResultCollection.InterfaceDetail1.DeviceAddress;

        /// <inheritdoc />
        public IPAddress Address2 => (IPAddress)this.linkRelatedResultCollection.InterfaceDetail2.DeviceAddress;

        /// <inheritdoc />
        public string ModelAndVersion1 => this.linkRelatedResultCollection.InterfaceDetail1.DeviceModel;

        /// <inheritdoc />
        public string ModelAndVersion2 => this.linkRelatedResultCollection.InterfaceDetail2.DeviceModel;

        /// <inheritdoc />
        public double? Ccq { get; } = null;

        /// <inheritdoc />
        public void ForceEvaluateAll()
        {
            this.linkRelatedResultCollection?.InterfaceDetail1?.ForceEvaluateAll();
            this.linkRelatedResultCollection?.InterfaceDetail2?.ForceEvaluateAll();
            this.linkRelatedResultCollection?.WirelessPeerInfo1?.ForceEvaluateAll();
            this.linkRelatedResultCollection?.WirelessPeerInfo2?.ForceEvaluateAll();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (this.linkRelatedResultCollection == null)
            {
                return $"No link found";
            }

            StringBuilder returnBuilder = new StringBuilder(128);

            returnBuilder.Append("Link between side #1 (").Append(this.Address1).Append(", ").Append(this.ModelAndVersion1).Append(") and side #2 (").Append(this.Address2).Append(", ").Append(this.ModelAndVersion2).AppendLine("):");
            returnBuilder.Append("Side #1 MAC: ").AppendLine(this.MacString1);
            returnBuilder.Append("Side #2 MAC: ").AppendLine(this.MacString2);
            returnBuilder.Append("Side of AP : ").AppendLine(this.SideOfAccessPoint?.ToString("0") ?? "not available");
            returnBuilder.Append("Rx level of side #1 at side #2: ").AppendLine(this.RxLevel1at2.ToString("0.0 dBm"));
            returnBuilder.Append("Rx level of side #2 at side #1: ").AppendLine(this.RxLevel2at1.ToString("0.0 dBm"));
            returnBuilder.Append("Link Uptime: ").Append(this.LinkUptime.ToString());
            returnBuilder.Append("Link CCQ: ").Append(this.Ccq.ToString());

            return returnBuilder.ToString();
        }
    }
}
