using System.Text;

namespace SnmpAbstraction
{
    /// <summary>
    /// Standard formatter for blocktext formatting.
    /// </summary>
    public class BlockTextFormatter
    {
        /// <summary>
        /// String i case a value is not available.
        /// </summary>
        private const string NotAvailableString = "not available";

        /// <summary>
        /// Formats a generic object if it's of one of the supported types.
        /// </summary>
        /// <param name="someObject">The object to format.</param>
        /// <returns>The formatted text.</returns>
        public string Format(object someObject)
        {
            if (someObject == null)
            {
                return "<null>";
            }

            IDeviceSystemData asDevSysData = someObject as IDeviceSystemData;
            if (asDevSysData != null)
            {
                return this.Format(asDevSysData);
            }

            IInterfaceDetails asIfDetails = someObject as IInterfaceDetails;
            if (asIfDetails != null)
            {
                return this.Format(asIfDetails);
            }

            IInterfaceDetail asIfDetail = someObject as IInterfaceDetail;
            if (asIfDetail != null)
            {
                return this.Format(asIfDetail);
            }

            IWirelessPeerInfos asWiPeerInfos = someObject as IWirelessPeerInfos;
            if (asWiPeerInfos != null)
            {
                return this.Format(asWiPeerInfos);
            }

            IWirelessPeerInfo asWiPeerInfo = someObject as IWirelessPeerInfo;
            if (asWiPeerInfo != null)
            {
                return this.Format(asWiPeerInfo);
            }

            ILinkDetails asLinkDetails = someObject as ILinkDetails;
            if (asLinkDetails != null)
            {
                return this.Format(asLinkDetails);
            }

            // fallback: call the object's ToString
            return someObject.ToString();
        }

        /// <summary>
        /// Formats as block-formatted string of an <see cref="ILinkDetail" />.
        /// </summary>
        /// <param name="linkDetail">The data to format.</param>
        /// <returns>The formatted string.</returns>
        public string Format(ILinkDetail linkDetail)
        {
            if (linkDetail == null)
            {
                return "<null>";
            }

            StringBuilder returnBuilder = new StringBuilder(256);

            returnBuilder.Append("Link between side #1 (").Append(linkDetail.Address1).Append(", ").Append(linkDetail.ModelAndVersion1).Append(") and side #2 (").Append(linkDetail.Address2).Append(", ").Append(linkDetail.ModelAndVersion2).AppendLine("):");
            returnBuilder.Append("Side #1 MAC: ").AppendLine(linkDetail.MacString1);
            returnBuilder.Append("Side #2 MAC: ").AppendLine(linkDetail.MacString2);
            returnBuilder.Append("Side of AP : ").AppendLine(linkDetail.SideOfAccessPoint?.ToString("0") ?? "not available");
            returnBuilder.Append("Rx level of side #1 at side #2: ").AppendLine(linkDetail.RxLevel1at2.ToString("0.0 dBm"));
            returnBuilder.Append("Rx level of side #2 at side #1: ").AppendLine(linkDetail.RxLevel2at1.ToString("0.0 dBm"));
            returnBuilder.Append("Link Uptime: ").Append(linkDetail.LinkUptime.ToString());

            return returnBuilder.ToString();
        }

        /// <summary>
        /// Formats as block-formatted string of an <see cref="ILinkDetails" />.
        /// </summary>
        /// <param name="linkDetails">The data to format.</param>
        /// <returns>The formatted string.</returns>
        public string Format(ILinkDetails linkDetails)
        {
            if (linkDetails == null)
            {
                return "<null>";
            }

            StringBuilder returnBuilder = new StringBuilder((linkDetails.Details?.Count ?? 1) * 128);

            returnBuilder.Append("Link Details:");

            if (linkDetails.Details.Count == 0)
            {
                returnBuilder.Append(" No link found.");
            }
            else
            {
                foreach (var item in linkDetails.Details)
                {
                    returnBuilder.AppendLine().AppendLine(SnmpAbstraction.IndentLines(this.Format(item)));
                }
            }

            return returnBuilder.ToString();
        }

        /// <summary>
        /// Formats as block-formatted string of an <see cref="IWirelessPeerInfo" />.
        /// </summary>
        /// <param name="wirelessPeerInfo">The data to format.</param>
        /// <returns>The formatted string.</returns>
        public string Format(IWirelessPeerInfo wirelessPeerInfo)
        {
            if (wirelessPeerInfo == null)
            {
                return "<null>";
            }

            StringBuilder returnBuilder = new StringBuilder(256);

            returnBuilder.Append("Peer ").Append(wirelessPeerInfo.RemoteMacString).AppendLine(":");
            returnBuilder.Append("  - Mode           : ").AppendLine(wirelessPeerInfo.IsAccessPoint.HasValue ? (wirelessPeerInfo.IsAccessPoint.Value ? "AP" : "Client") : NotAvailableString);
            returnBuilder.Append("  - On interface ID: ").AppendLine(wirelessPeerInfo.InterfaceId.HasValue ? wirelessPeerInfo.InterfaceId.Value.ToString() : NotAvailableString);
            returnBuilder.Append("  - Link Uptime    : ").AppendLine(wirelessPeerInfo.LinkUptime.ToString());
            returnBuilder.Append("  - RX signal [dBm]: ").AppendLine(wirelessPeerInfo.RxSignalStrength.ToString("0.0 dBm"));
            returnBuilder.Append("  - TX signal [dBm]: ").Append(wirelessPeerInfo.TxSignalStrength.ToString("0.0 dBm"));

            return returnBuilder.ToString();
        }

        /// <summary>
        /// Formats as block-formatted string of an <see cref="IWirelessPeerInfos" />.
        /// </summary>
        /// <param name="wirelessPeerInfos">The data to format.</param>
        /// <returns>The formatted string.</returns>
        public string Format(IWirelessPeerInfos wirelessPeerInfos)
        {
            if (wirelessPeerInfos == null)
            {
                return "<null>";
            }

            StringBuilder returnBuilder = new StringBuilder(256);

            returnBuilder.Append("Wireless Peer Infos: ");

            if (wirelessPeerInfos.Details == null)
            {
                returnBuilder.Append(NotAvailableString);
            }
            else
            {
                foreach (var item in wirelessPeerInfos.Details)
                {
                    returnBuilder.AppendLine().AppendLine(SnmpAbstraction.IndentLines(this.Format(item)));
                }
            }

            return returnBuilder.ToString();
        }

        /// <summary>
        /// Formats as block-formatted string of an <see cref="IInterfaceDetails" />.
        /// </summary>
        /// <param name="interfaceDetails">The data to format.</param>
        /// <returns>The formatted string.</returns>
        public string Format(IInterfaceDetails interfaceDetails)
        {
            if (interfaceDetails == null)
            {
                return "<null>";
            }

            StringBuilder returnBuilder = new StringBuilder(256);

            returnBuilder.Append("Interface Details: ");

            if (interfaceDetails.Details == null)
            {
                returnBuilder.Append(NotAvailableString);
            }
            else
            {
                foreach (var item in interfaceDetails.Details)
                {
                    returnBuilder.AppendLine().AppendLine(SnmpAbstraction.IndentLines(this.Format(item)));
                }
            }

            return returnBuilder.ToString();
        }

        /// <summary>
        /// Formats as block-formatted string of an <see cref="IInterfaceDetail" />.
        /// </summary>
        /// <param name="interfaceDetail">The data to format.</param>
        /// <returns>The formatted string.</returns>
        public string Format(IInterfaceDetail interfaceDetail)
        {
            if (interfaceDetail == null)
            {
                return "<null>";
            }

            StringBuilder returnBuilder = new StringBuilder(256);

            returnBuilder.Append("Interface #").Append(interfaceDetail.InterfaceId).Append(" (").Append(string.IsNullOrWhiteSpace(interfaceDetail.InterfaceName) ? NotAvailableString : interfaceDetail.InterfaceName).AppendLine("):");
            returnBuilder.Append("  - Type: ").AppendLine(interfaceDetail.InterfaceType.ToString());
            returnBuilder.Append("  - MAC : ").Append(string.IsNullOrWhiteSpace(interfaceDetail.InterfaceName) ? NotAvailableString : interfaceDetail.MacAddressString);

            return returnBuilder.ToString();
        }

        /// <summary>
        /// Formats as block-formatted string of an <see cref="IDeviceSystemData" />.
        /// </summary>
        /// <param name="deviceSystemData">The data to format.</param>
        /// <returns>The formatted string.</returns>
        public string Format(IDeviceSystemData deviceSystemData)
        {
            if (deviceSystemData == null)
            {
                return "<null>";
            }

            StringBuilder returnBuilder = new StringBuilder(256);

            returnBuilder.Append("  - System Model      : ").AppendLine(string.IsNullOrWhiteSpace(deviceSystemData.Model) ? NotAvailableString : deviceSystemData.Model);
            returnBuilder.Append("  - System SW Version : ").AppendLine((deviceSystemData.Version == null) ? NotAvailableString : deviceSystemData.Version.ToString());
            returnBuilder.Append("  - System Name       : ").AppendLine(string.IsNullOrWhiteSpace(deviceSystemData.Name) ? NotAvailableString : deviceSystemData.Name);
            returnBuilder.Append("  - System location   : ").AppendLine(string.IsNullOrWhiteSpace(deviceSystemData.Location) ? NotAvailableString : deviceSystemData.Location);
            returnBuilder.Append("  - System description: ").AppendLine(string.IsNullOrWhiteSpace(deviceSystemData.Description) ? NotAvailableString : deviceSystemData.Description);
            returnBuilder.Append("  - System admin      : ").AppendLine(string.IsNullOrWhiteSpace(deviceSystemData.Contact) ? NotAvailableString : deviceSystemData.Contact);
            returnBuilder.Append("  - System uptime     : ").AppendLine(deviceSystemData.Uptime.HasValue ? deviceSystemData.Uptime?.ToString() : NotAvailableString);
            returnBuilder.Append("  - System root OID   : ").AppendLine(deviceSystemData.EnterpriseObjectId == null ? NotAvailableString : deviceSystemData.EnterpriseObjectId?.ToString());
            returnBuilder.Append("  - Max. SNMP version : ").AppendLine(deviceSystemData.MaximumSnmpVersion.ToString());

            return returnBuilder.ToString();
        }
    }
}