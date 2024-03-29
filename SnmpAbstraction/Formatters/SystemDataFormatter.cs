using System.Linq;
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

            if (someObject is IDeviceSystemData asDevSysData)
            {
                return this.Format(asDevSysData);
            }

            if (someObject is IInterfaceDetails asIfDetails)
            {
                return this.Format(asIfDetails);
            }

            if (someObject is IInterfaceDetail asIfDetail)
            {
                return this.Format(asIfDetail);
            }

            if (someObject is IWirelessPeerInfos asWiPeerInfos)
            {
                return this.Format(asWiPeerInfos);
            }

            if (someObject is IWirelessPeerInfo asWiPeerInfo)
            {
                return this.Format(asWiPeerInfo);
            }

            if (someObject is ILinkDetails asLinkDetails)
            {
                return this.Format(asLinkDetails);
            }

            if (someObject is IBgpPeers asBgpPeers)
            {
                return this.Format(asBgpPeers);
            }

            if (someObject is IBgpPeer asBgpPeer)
            {
                return this.Format(asBgpPeer);
            }

            if (someObject is ITracerouteResult asTracerouteResult)
            {
                return this.Format(asTracerouteResult);
            }

            if (someObject is ITracerouteHop asTracerouteHop)
            {
                return this.Format(asTracerouteHop);
            }

            // fallback: call the object's ToString
            return someObject.ToString();
        }

        /// <summary>
        /// Formats as block-formatted string of an <see cref="ITracerouteResult" />.
        /// </summary>
        /// <param name="traceRouteResult">The data to format.</param>
        /// <returns>The formatted string.</returns>
        public string Format(ITracerouteResult traceRouteResult)
        {
            if (traceRouteResult == null)
            {
                return "<null>";
            }

            StringBuilder returnBuilder = new StringBuilder(traceRouteResult.HopCount * 128);

            returnBuilder.Append("Routing Hops:");

            if (traceRouteResult.HopCount == 0)
            {
                returnBuilder.Append(" No hops found.");
            }
            else
            {
                uint hopCounter = 0;
                foreach (var item in traceRouteResult.Hops)
                {
                    returnBuilder.Append("Hop #").Append(++hopCounter).Append(": ").AppendLine(SnmpAbstraction.IndentLines(this.Format(item)));
                }
            }

            return returnBuilder.ToString();
        }

        /// <summary>
        /// Formats as block-formatted string of an <see cref="ITracerouteHop" />.
        /// </summary>
        /// <param name="tracerouteHop">The data to format.</param>
        /// <returns>The formatted string.</returns>
#pragma warning disable CA1822 // API
        public string Format(ITracerouteHop tracerouteHop)
#pragma warning restore
        {
            if (tracerouteHop == null)
            {
                return "<null>";
            }

            StringBuilder returnBuilder = new StringBuilder(256);

            returnBuilder
                .Append("Address:").Append(tracerouteHop.Address)
                .Append(", RTT ").Append(tracerouteHop.BestRtt)
                .Append(" / ").Append(tracerouteHop.AverageRtt)
                .Append(" / ").Append(tracerouteHop.WorstRtt)
                .Append(", Last RTT ").Append(tracerouteHop.LastRtt)
                .Append(", Loss ").Append(tracerouteHop.LossPercent)
                .Append(", Sent ").Append(tracerouteHop.SentCount);

            return returnBuilder.ToString();
        }

        /// <summary>
        /// Formats as block-formatted string of an <see cref="IBgpPeer" />.
        /// </summary>
        /// <param name="bgpPeer">The data to format.</param>
        /// <returns>The formatted string.</returns>
#pragma warning disable CA1822 // API
        public string Format(IBgpPeer bgpPeer)
#pragma warning restore
        {
            if (bgpPeer == null)
            {
                return "<null>";
            }

            StringBuilder returnBuilder = new StringBuilder(256);

            returnBuilder.Append("ID:").AppendLine(bgpPeer.Id);
            returnBuilder.Append("Name:").AppendLine(bgpPeer.Name);
            returnBuilder.Append("Instance").AppendLine(bgpPeer.Instance);
            returnBuilder.Append("Local Address: ").AppendLine(bgpPeer.LocalAddress?.ToString());
            returnBuilder.Append("Remote Address: ").AppendLine(bgpPeer.RemoteAddress?.ToString());
            returnBuilder.Append("Remote AS: ").AppendLine(bgpPeer.RemoteAs.ToString());
            returnBuilder.Append("NextHop choice: ").AppendLine(bgpPeer.NexthopChoice);
            returnBuilder.Append("Multihop: ").AppendLine(bgpPeer.Multihop.ToString());
            returnBuilder.Append("RouteReflex: ").AppendLine(bgpPeer.RouteReflect.ToString());
            returnBuilder.Append("HoldTime: ").AppendLine(bgpPeer.HoldTime.ToString());
            returnBuilder.Append("Ttl: ").AppendLine(bgpPeer.Ttl.ToString());
            returnBuilder.Append("Address Families: ").AppendLine(string.Join(", ", bgpPeer.AddressFamilies.Select(f => f.ToString())));
            returnBuilder.Append("Default Originate: ").AppendLine(bgpPeer.DefaultOriginate);
            returnBuilder.Append("Remove Priv. AS: ").AppendLine(bgpPeer.RemovePrivateAs.ToString());
            returnBuilder.Append("AS Override: ").AppendLine(bgpPeer.AsOverride.ToString());
            returnBuilder.Append("Passive: ").AppendLine(bgpPeer.Passive.ToString());
            returnBuilder.Append("Use BFD: ").AppendLine(bgpPeer.UseBfd.ToString());
            returnBuilder.Append("Remote ID: ").AppendLine(bgpPeer.RemoteId);
            returnBuilder.Append("Uptime: ").AppendLine(bgpPeer.Uptime.ToString());
            returnBuilder.Append("Prefix count: ").AppendLine(bgpPeer.PrefixCount.ToString());
            returnBuilder.Append("Updates sent: ").AppendLine(bgpPeer.UpdatesSent.ToString());
            returnBuilder.Append("Updates received: ").AppendLine(bgpPeer.UpdatesReceived.ToString());
            returnBuilder.Append("Withdrawn sent: ").AppendLine(bgpPeer.WithdrawnSent.ToString());
            returnBuilder.Append("Withdrawn received: ").AppendLine(bgpPeer.WithdrawnReceived.ToString());
            returnBuilder.Append("Remote hold time: ").AppendLine(bgpPeer.RemoteHoldTime.ToString());
            returnBuilder.Append("Used hold time: ").AppendLine(bgpPeer.UsedHoldTime.ToString());
            returnBuilder.Append("Used Keepalive time: ").AppendLine(bgpPeer.UsedKeepaliveTime.ToString());
            returnBuilder.Append("Refresh Capability: ").AppendLine(bgpPeer.RefreshCapability.ToString());
            returnBuilder.Append("AS4 Capability: ").AppendLine(bgpPeer.As4Capability.ToString());
            returnBuilder.Append("State: ").AppendLine(bgpPeer.State);
            returnBuilder.Append("Established: ").AppendLine(bgpPeer.Established.ToString());
            returnBuilder.Append("Disabled: ").AppendLine(bgpPeer.Disabled.ToString());

            return returnBuilder.ToString();
        }

        /// <summary>
        /// Formats as block-formatted string of an <see cref="IBgpPeers" />.
        /// </summary>
        /// <param name="bgpPeers">The data to format.</param>
        /// <returns>The formatted string.</returns>
        public string Format(IBgpPeers bgpPeers)
        {
            if (bgpPeers == null)
            {
                return "<null>";
            }

            StringBuilder returnBuilder = new StringBuilder((bgpPeers.Details?.Count ?? 1) * 128);

            returnBuilder.Append("BGP Peers:");

            if (bgpPeers.Details.Count == 0)
            {
                returnBuilder.Append(" No BGP peers found.");
            }
            else
            {
                foreach (var item in bgpPeers.Details)
                {
                    returnBuilder.AppendLine().AppendLine(SnmpAbstraction.IndentLines(this.Format(item)));
                }
            }

            return returnBuilder.ToString();
        }

        /// <summary>
        /// Formats as block-formatted string of an <see cref="ILinkDetail" />.
        /// </summary>
        /// <param name="linkDetail">The data to format.</param>
        /// <returns>The formatted string.</returns>
#pragma warning disable CA1822 // API
        public string Format(ILinkDetail linkDetail)
#pragma warning restore
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
#pragma warning disable CA1822 // API
        public string Format(IWirelessPeerInfo wirelessPeerInfo)
#pragma warning restore
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
            returnBuilder.Append("  - TX signal [dBm]: ").AppendLine(wirelessPeerInfo.TxSignalStrength.ToString("0.0 dBm"));
            returnBuilder.Append("  - CCQ            : ").Append(wirelessPeerInfo.Ccq?.ToString("0") ?? "not available");

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
#pragma warning disable CA1822 // API
        public string Format(IInterfaceDetail interfaceDetail)
#pragma warning restore
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
#pragma warning disable CA1822 // API
        public string Format(IDeviceSystemData deviceSystemData)
#pragma warning restore
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