using System;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the central querier object that queries for delivers specific data.
    /// </summary>
    public interface IHamnetQuerier : IDisposable
    {
        /// <summary>
        /// Gets the IP address that this querier talks to.
        /// </summary>
        IpAddress Address { get; }

        /// <summary>
        /// Gets the API(s) that this querier is using.
        /// </summary>
        QueryApis Api { get; }

        /// <summary>
        /// Gets the type of the device handler that is currently in use.
        /// </summary>
        Type HandlerType { get; }

        /// <summary>
        /// Gets the device system data (i.e. the .1.3.6.1.2.1.1 subtree which is mainly device-agnostic).
        /// </summary>
        /// <remarks>This data will implicitly be queried when a instance of an SNMP Querier initialized.<br/>
        /// This is because it includes the data that is needed to determine how to talk to the device.</remarks>
        /// <value>A lazy-evaluated interface to the data.</value>
        IDeviceSystemData SystemData { get; }

        /// <summary>
        /// Gets the device's interface details like interface type, MAC, IP, ...
        /// </summary>
        /// <value>A lazy-evaluated interface to the data.</value>
        IInterfaceDetails NetworkInterfaceDetails { get; }

        /// <summary>
        /// Gets the device's wireless peer details like MAC, ...
        /// </summary>
        /// <value>A lazy-evaluated interface to the data.</value>
        IWirelessPeerInfos WirelessPeerInfos { get; }

        /// <summary>
        /// Fetches the information about the BGP peers that are connected to the device that is associated
        /// with this instance and the devices owning the listed remote host names or IP addresses.
        /// </summary>
        /// <param name="remotePeer">The BGP peer IP address to which the BGP details shall be fetched. If null or empty, all peers will be fetched.</param>
        /// <returns>The BGP peers that are currently connected to this device.</returns>
        IBgpPeers FetchBgpPeers(string remotePeer);

        /// <summary>
        /// Fetches the detailed information for the link between the device that is associated
        /// with this instance and the devices owning the listed remote host names or IP addresses.
        /// </summary>
        /// <param name="remoteHostNamesOrIps">The list of remote device's host name or IP address to which the link details shall be fetched.</param>
        /// <returns>The link details for the link between this device and the listed remote devices.</returns>
        ILinkDetails FetchLinkDetails(params string[] remoteHostNamesOrIps);

        /// <summary>
        /// Fetches the detailed information for the link between the device that is associated
        /// with this instance and the devices owning the listed remote host names or IP addresses.
        /// </summary>
        /// <param name="remoteQueriers">The list of queriers to the remote devices to which the link details shall be fetched.</param>
        /// <returns>The link details for the link between this device and the listed remote devices.</returns>
        ILinkDetails FetchLinkDetails(params IHamnetQuerier[] remoteQueriers);

        /// <summary>
        /// Performs a traceroute 
        /// </summary>
        /// <param name="remoteHostNameOrIp">The remote device's host name or IP address to which the traceroute shall be done.</param>
        /// <param name="count">The number of packets to send for tracing the route.</param>
        /// <param name="timeout">The timeout of a single ping.</param>
        /// <param name="maxHops">The maximum number of hops.</param>
        /// <returns>The result of the traceroute.</returns>
        ITracerouteResult Traceroute(string remoteHostNameOrIp, uint count, TimeSpan timeout, int maxHops);
    }
}
