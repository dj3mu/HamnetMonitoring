using System;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface that serves as a handler for a specific device.
    /// </summary>
    public interface IDeviceHandler : IDisposable
    {
        /// <summary>
        /// Gets the API that is supported by this handler.
        /// </summary>
        QueryApis SupportedApi { get; }
        
        /// <summary>
        /// Gets the IP address that this device handler handles.
        /// </summary>
        IpAddress Address { get; }

        /// <summary>
        /// Gets that options that are used by this device handler.
        /// </summary>
        IQuerierOptions Options { get; }

        /// <summary>
        /// Gets the device's operation system (i.e. Software) version.
        /// </summary>
        SemanticVersion OsVersion { get; }

        /// <summary>
        /// Gets the device's model name.<br/>
        /// Shall return the same name as used for the device name during OID database lookups.
        /// </summary>
        string Model { get; }

        /// <summary>
        /// Gets the device's generic system data.
        /// </summary>
        IDeviceSystemData SystemData { get; }

        /// <summary>
        /// Gets the device's interface details like interface type, MAC, IP, ...
        /// </summary>
        /// <value>A lazy-evaluated interface to the data.</value>
        IInterfaceDetails NetworkInterfaceDetails { get; }

        /// <summary>
        /// Gets the device's interface details like interface type, MAC, IP, ...
        /// </summary>
        /// <value>A lazy-evaluated interface to the data.</value>
        IWirelessPeerInfos WirelessPeerInfos { get; }

        /// <summary>
        /// Fetches the information about the BGP peers that are connected to the device that is associated
        /// with this instance and the devices owning the listed remote host names or IP addresses.
        /// </summary>
        /// <param name="remotePeerIp">The BGP peer IP address to which the BGP details shall be fetched. If null or empty, all peers will be fetched.</param>
        /// <returns>The BGP peers that are currently connected to this device.</returns>
        IBgpPeers FetchBgpPeers(string remotePeerIp);

        /// <summary>
        /// Performs a traceroute 
        /// </summary>
        /// <param name="remoteIp">The remote device's IP address to which the traceroute shall be done.</param>
        /// <param name="count">The number of packets to send for tracing the route.</param>
        /// <returns>The result of the traceroute.</returns>
        ITracerouteResult Traceroute(IpAddress remoteIp, uint count);
    }
}