using System;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class LinkDetail : HamnetSnmpQuerierResultBase, ILinkDetail
    {
        /// <summary>
        /// The tuple of the two link partners
        /// </summary>
        private LinkRelatedResultCollection linkRelatedResultCollection;

        public LinkDetail(IpAddress address, LinkRelatedResultCollection linkRelatedResultCollection)
            : base(address, TimeSpan.Zero)
        {
            this.linkRelatedResultCollection = linkRelatedResultCollection;
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

        public TimeSpan LinkUptime => this.linkRelatedResultCollection?.WirelessPeerInfo2?.LinkUptime ?? TimeSpan.Zero;

        /// <inheritdoc />
        public override string ToTextString()
        {
            if (this.linkRelatedResultCollection == null)
            {
                return $"No link found";
            }

            StringBuilder returnBuilder = new StringBuilder(128);

            returnBuilder.Append("Link between side #1 (").Append(this.linkRelatedResultCollection.InterfaceDetail1.DeviceAddress).Append(") and side #2 (").Append(this.linkRelatedResultCollection.InterfaceDetail2.DeviceAddress).AppendLine("):");
            returnBuilder.Append("Side #1 MAC: ").AppendLine(this.MacString1);
            returnBuilder.Append("Side #2 MAC: ").AppendLine(this.MacString2);
            returnBuilder.Append("Rx level of side #1 at side #2: ").AppendLine(this.RxLevel1at2.ToString());
            returnBuilder.Append("Rx level of side #2 at side #1: ").AppendLine(this.RxLevel2at1.ToString());
            returnBuilder.Append("Link Uptime: ").Append(this.LinkUptime.ToString());

            return returnBuilder.ToString();
        }
    }
}
