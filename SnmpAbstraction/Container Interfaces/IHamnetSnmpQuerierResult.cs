using System;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Marker interface for all result containers returned by a <see cref="IHamnetQuerier" />.
    /// </summary>
    public interface IHamnetSnmpQuerierResult
    {
        /// <summary>
        /// Gets the address of the device that produced this result.
        /// </summary>
        IpAddress DeviceAddress { get; }

        /// <summary>
        /// Gets the model of the device that produced this result.
        /// </summary>
        string DeviceModel { get; }

        /// <summary>
        /// Gets the total duration of the value queries that have been executed by this object.
        /// </summary>
        /// <remarks>The value may increase continuously as queries might be done lazily when needed.</remarks>
        TimeSpan QueryDuration { get; }
    }
}