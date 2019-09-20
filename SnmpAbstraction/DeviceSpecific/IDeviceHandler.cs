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
        /// Gets the IP address that this device handler handles.
        /// </summary>
        IpAddress Address { get; }

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
    }
}