using System;
using System.IO;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
///     /// Base class for all <see cref="IHamnetSnmpQuerierResult" />.
    /// </summary>
    internal abstract class HamnetSnmpQuerierResultBase : IHamnetSnmpQuerierResult
    {
        /// <summary>
        /// Construct for the given device address and query duration.
        /// </summary>
        /// <param name="deviceAddress">The device address that this result is for.</param>
        /// <param name="queryDuration">The duration of the query.</param>
        protected HamnetSnmpQuerierResultBase(IpAddress deviceAddress, TimeSpan queryDuration)
        {
            this.DeviceAddress = deviceAddress;
            this.QueryDuration = queryDuration;
        }

        /// <summary>
        /// Construct for the given device address.
        /// </summary>
        /// <param name="deviceAddress">The device address that this result is for.</param>
        /// <param name="queryDuration">The duration of the query.</param>
        protected HamnetSnmpQuerierResultBase(IpAddress deviceAddress)
            : this(deviceAddress, TimeSpan.Zero)
        {
        }

        /// <inheritdoc />
        public IpAddress DeviceAddress { get; }

        /// <inheritdoc />
        public virtual TimeSpan QueryDuration { get; }

        /// <inheritdoc />
        public void ToTextWriter(TextWriter writer)
        {
            writer.WriteLine($"Device {this.DeviceAddress}:");
            writer.WriteLine(this.ToTextString());
            writer.WriteLine($"  --> Query took {this.QueryDuration.TotalMilliseconds} ms");
        }
        
        /// <summary>
        /// Prints the result body to the given <see cref="TextWriter" /> using
        /// human-readable formatting.
        /// </summary>
        /// <returns>A string with the human readable data.</returns>
        public abstract string ToTextString();
    }
}
