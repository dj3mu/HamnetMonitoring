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
            : base(address, TimeSpan.Zero)
        {
            this.linkRelatedResultCollection = linkRelatedResultCollection;
            if ((this.linkRelatedResultCollection?.WirelessPeerInfo1?.IsAccessPoint != null)
                && (this.linkRelatedResultCollection.WirelessPeerInfo1.IsAccessPoint.HasValue))
            {
                this.SideOfAccessPoint = this.linkRelatedResultCollection.WirelessPeerInfo1.IsAccessPoint.Value ? 1 : 2;
            }
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
        public TimeSpan LinkUptime => this.linkRelatedResultCollection?.WirelessPeerInfo2?.LinkUptime ?? TimeSpan.Zero;

        /// <inheritdoc />
        public int? SideOfAccessPoint { get; } = null;

        /// <inheritdoc />
        public IPAddress Address1 => (IPAddress)this.linkRelatedResultCollection.InterfaceDetail1.DeviceAddress;

        /// <inheritdoc />
        public IPAddress Address2 => (IPAddress)this.linkRelatedResultCollection.InterfaceDetail2.DeviceAddress;

        /// <inheritdoc />
        public override string ToTextString()
        {
            if (this.linkRelatedResultCollection == null)
            {
                return $"No link found";
            }

            StringBuilder returnBuilder = new StringBuilder(128);

            returnBuilder.Append("Link between side #1 (").Append(this.Address1).Append(") and side #2 (").Append(this.Address2).AppendLine("):");
            returnBuilder.Append("Side #1 MAC: ").AppendLine(this.MacString1);
            returnBuilder.Append("Side #2 MAC: ").AppendLine(this.MacString2);
            returnBuilder.Append("Side of AP : ").AppendLine(this.SideOfAccessPoint?.ToString("0") ?? "not available");
            returnBuilder.Append("Rx level of side #1 at side #2: ").AppendLine(this.RxLevel1at2.ToString("0.0 dBm"));
            returnBuilder.Append("Rx level of side #2 at side #1: ").AppendLine(this.RxLevel2at1.ToString("0.0 dBm"));
            returnBuilder.Append("Link Uptime: ").Append(this.LinkUptime.ToString());

            return returnBuilder.ToString();
        }
    }
}
