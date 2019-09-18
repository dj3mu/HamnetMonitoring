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
        /// Fetches the detailed information for the link between the device that is associated
        /// with this instance and the devices owning the listed remote host names or IP addresses.
        /// </summary>
        /// <param name="remoteHostNamesOrIps">The list of remote device's host name or IP address to which the link details shall be fetched.</param>
        /// <returns>The link details for the link between this device and the listed remote devices.</returns>
        ILinkDetails FetchLinkDetails(params string[] remoteHostNamesOrIps);
    }
}
