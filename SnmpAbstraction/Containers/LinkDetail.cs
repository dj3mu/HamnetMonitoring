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
        private Tuple<IInterfaceDetail, IWirelessPeerInfo, IInterfaceDetail> linkInfoTuple;

        public LinkDetail(IpAddress address, Tuple<IInterfaceDetail, IWirelessPeerInfo, IInterfaceDetail> linkInfoTuple)
            : base(address, TimeSpan.Zero)
        {
            this.linkInfoTuple = linkInfoTuple;

            if (this.linkInfoTuple != null)
            {
                this.QueryDuration = this.linkInfoTuple.Item1.QueryDuration + this.linkInfoTuple.Item2.QueryDuration;
                this.MacString1 = this.linkInfoTuple.Item1.MacAddressString;
                this.MacString2 = this.linkInfoTuple.Item3.MacAddressString;
            }
        }

        /// <inheritdoc />
        public string MacString1 { get; } = null;

        /// <inheritdoc />
        public string MacString2 { get; } = null;

        /// <inheritdoc />
        public double RxLevel1at2 { get; } = int.MinValue;

        /// <inheritdoc />
        public double RxLevel2at1 { get; } = int.MinValue;

        /// <inheritdoc />
        public override TimeSpan QueryDuration { get; } = TimeSpan.Zero;

        /// <inheritdoc />
        public override string ToTextString()
        {
            if (this.linkInfoTuple == null)
            {
                return $"No link found";
            }

            StringBuilder returnBuilder = new StringBuilder(128);

            returnBuilder.Append("Link between side #1 (").Append(this.linkInfoTuple.Item1.DeviceAddress).Append(") and side #2 (").Append(this.linkInfoTuple.Item3.DeviceAddress).AppendLine("):");
            returnBuilder.Append("Side #1 MAC: ").AppendLine(this.MacString1);
            returnBuilder.Append("Side #2 MAC: ").AppendLine(this.MacString2);
            returnBuilder.Append("Rx level of side #1 at side #2: ").AppendLine(this.RxLevel1at2.ToString());
            returnBuilder.Append("Rx level of side #2 at side #1: ").Append(this.RxLevel2at1);

            return returnBuilder.ToString();
        }
    }
}