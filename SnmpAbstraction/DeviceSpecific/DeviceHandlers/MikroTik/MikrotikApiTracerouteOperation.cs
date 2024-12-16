using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly IpAddress remoteIp;
        private readonly uint count;
        private readonly IpAddress address;
        private readonly ITikConnection tikConnection;
        private readonly TimeSpan timeout;
        private readonly int maxHops;

        /// <summary>
        /// Construct from all parameters.
        /// </summary>
        /// <param name="address">The address of the device executing the traceroute operation.</param>
        /// <param name="tikConnection">The Mikrotik API connection.</param>
        /// <param name="remoteIp">The target of the traceroute operation.</param>
        /// <param name="count">The number of packets to send for tracing the route.</param>
        /// <param name="timeout">The timeout of a single ping.</param>
        /// <param name="maxHops">The maximum number of hops.</param>
        public MikrotikApiTracerouteOperation(IpAddress address, ITikConnection tikConnection, IpAddress remoteIp, uint count, TimeSpan timeout, int maxHops)
        {
            this.maxHops = maxHops;
            this.timeout = timeout;
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
            var results = this.tikConnection.LoadList<HamnetToolTraceroute>(
                            this.tikConnection.CreateParameter("address", this.remoteIp.ToString(), TikCommandParameterFormat.NameValue),
                            this.tikConnection.CreateParameter("count", countToUse.ToString(), TikCommandParameterFormat.NameValue),
                            this.tikConnection.CreateParameter("timeout", this.timeout.TotalSeconds.ToString(), TikCommandParameterFormat.NameValue),
                            this.tikConnection.CreateParameter("max-hops", this.maxHops.ToString(), TikCommandParameterFormat.NameValue));

            stopper.Stop();

            List<TracerrouteHop> hopsToSend = new List<TracerrouteHop>();

            int currentHopListIndex = 0;
            TracerrouteHop firstHop = null;
            foreach(var result in results)
            {
                var currentHop = new TracerrouteHop(result);

                if (currentHop.Address == null)
                {
                    if (currentHop.LossPercent <= 0.001)
                    {
                        // After sending an empty address and loss of 0 the complete hop sequence
                        // restarts from scratch.
                        currentHopListIndex = 0;
                    }

                    // but we always ignore "null" addresses even if we don't reset the sequence
                    continue;
                }

                if (firstHop == null)
                {
                    firstHop = currentHop;
                }
                else if (currentHop.Address.Equals(firstHop.Address))
                {
                    // first IP found (again?)
                    if (currentHop.SentCount >= firstHop.SentCount)
                    {
                        // hop count for first IP same or higher --> new sequence starting
                        firstHop = currentHop;
                        currentHopListIndex = 0;
                    }
                }

                hopsToSend.PutAt(currentHopListIndex++, currentHop);
            }

            var returnResult = new TracerouteResult(this.address, this.remoteIp, string.Empty, stopper.Elapsed, hopsToSend);

            return returnResult;
        }
    }
}