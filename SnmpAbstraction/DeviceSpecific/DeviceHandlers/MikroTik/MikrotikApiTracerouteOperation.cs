using System;
using System.Diagnostics;
using System.Linq;
using SnmpSharpNet;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.Tool;

namespace SnmpAbstraction
{
    /// <summary>
    /// Handler class for a traceroute operation that uses the Mikrotik API.
    /// </summary>
    internal class MikrotikApiTracerouteOperation
    {
        private IpAddress remoteIp;
        private readonly uint count;
        private IpAddress address;
        private ITikConnection tikConnection;

        /// <summary>
        /// Construct from all parameters.
        /// </summary>
        /// <param name="address">The address of the device executing the traceroute operation.</param>
        /// <param name="tikConnection">The Mikrotik API connection.</param>
        /// <param name="remoteIp">The target of the traceroute operation.</param>
        /// <param name="count">The number of packets to send for tracing the route.</param>
        public MikrotikApiTracerouteOperation(IpAddress address, ITikConnection tikConnection, IpAddress remoteIp, uint count)
        {
            this.address = address ?? throw new System.ArgumentNullException(nameof(address), "address of executing device is null when creating a MikrotikApiTracerouteOperation");
            this.tikConnection = tikConnection ?? throw new System.ArgumentNullException(nameof(tikConnection), "tikConnection is null when creating a MikrotikApiTracerouteOperation");
            this.remoteIp = remoteIp ?? throw new System.ArgumentNullException(nameof(remoteIp), "address of the traceroute destination is null when creating a MikrotikApiTracerouteOperation");
            this.count = count;
        }

        /// <summary>
        /// Executes the operation and returns the result.
        /// </summary>
        /// <returns>The result of the traceroute operation.</returns>
        internal ITracerouteResult Execute()
        {
            Stopwatch stopper = Stopwatch.StartNew();
            
            var countToUse = (this.count <= 0) ? 1 : this.count;
            var result = this.tikConnection.LoadList<ToolTraceroute>(
                            this.tikConnection.CreateParameter("address", this.remoteIp.ToString(), TikCommandParameterFormat.NameValue),
                            this.tikConnection.CreateParameter("count", countToUse.ToString(), TikCommandParameterFormat.NameValue));

            stopper.Stop();

            if (countToUse > 1)
            {
                result = result.Where(h => h.Sent ==  countToUse);
            }

            var hops = result.Select(h => new TracerrouteHop(h));
            var returnResult = new TracerouteResult(this.address, this.remoteIp, string.Empty, stopper.Elapsed, hops.ToList());

            return returnResult;
        }
    }
}