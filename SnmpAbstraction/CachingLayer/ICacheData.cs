using System;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to a cache data set.
    /// </summary>
    public interface ICacheData
    {
        /// <summary>
        /// Gets the device address that this entry is for.
        /// </summary>
        IpAddress Address { get; }

        /// <summary>
        /// Gets device system data.
        /// </summary>
        IDeviceSystemData SystemData { get; }

        /// <summary>
        /// Gets the bytes of the serialized wireless peer info data.
        /// </summary>
        IWirelessPeerInfos WirelessPeerInfos { get; }

        /// <summary>
        /// Gets the bytes of the serialized interface data.
        /// </summary>
        IInterfaceDetails InterfaceDetails { get; }

        /// <summary>
        /// Gets the date and time of last modification of this table row.
        /// </summary>
        DateTime LastModification { get; }

        /// <summary>
        /// Gets the API that has been used to query the data recorded in the cache. Recommendation is to try talking to the device again using this API.
        /// </summary>
        QueryApis ApiUsed { get; }

        /// <summary>
        /// Gets the full qualified class name of the device handle that has been used to query the data recorded in the cache. Recommendation is to try talking to the device again using this API.
        /// </summary>
        string DeviceHandlerClass { get; }
    }
}
