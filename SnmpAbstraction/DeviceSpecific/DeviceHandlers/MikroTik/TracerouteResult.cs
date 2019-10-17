using System;
using System.Collections.Generic;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Container for a traceroute result.
    /// </summary>
    internal class TracerouteResult : ITracerouteResult
    {
        private IReadOnlyCollection<ITracerouteHop> hopsBacking;

        /// <summary>
        /// Construct 
        /// </summary>
        /// <param name="fromAddress">The address initiating the traceroute.</param>
        /// <param name="toAddress">The address of the target of the traceroute.</param>
        /// <param name="deviceModel">The device model of the device that triggered the traceroute.</param>
        /// <param name="queryDuration">The duration of the traceroute operation.</param>
        /// <param name="hops">The list of hops of the route.</param>
        public TracerouteResult(IpAddress fromAddress, IpAddress toAddress, string deviceModel, TimeSpan queryDuration, IReadOnlyCollection<ITracerouteHop> hops)
        {
            this.FromAddress = fromAddress ?? throw new ArgumentNullException(nameof(fromAddress), "the fromAddress is null when constructing TracerouteResult");
            this.ToAddress = toAddress ?? throw new ArgumentNullException(nameof(toAddress));
            this.DeviceModel = deviceModel ?? string.Empty;
            this.QueryDuration = queryDuration;
            this.hopsBacking = hops ?? throw new ArgumentNullException(nameof(hops), "the hops of this traceroute is null when constructing TracerouteResult");
        }

        /// <inheritdoc />
        public IpAddress FromAddress { get; }

        /// <inheritdoc />
        public IpAddress ToAddress { get; }

        /// <inheritdoc />
        public IEnumerable<ITracerouteHop> Hops => this.hopsBacking;

        /// <inheritdoc />
        public IpAddress DeviceAddress => this.FromAddress;

        /// <inheritdoc />
        public string DeviceModel { get; }

        /// <inheritdoc />
        public TimeSpan QueryDuration { get; }

        /// <inheritdoc />
        public int HopCount => this.hopsBacking.Count;
    }
}
