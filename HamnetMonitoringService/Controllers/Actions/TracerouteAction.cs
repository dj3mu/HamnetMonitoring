using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SnmpAbstraction;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Class doing a ping to a host
    /// </summary>
    internal class TracerouteAction
    {
        private string host;

        private string remotePeerAddress;
        private readonly int count;
        private readonly FromUrlQueryQuerierOptions querierOptions;
        private readonly TimeSpan timeout;
        private readonly int maxHops;

        /// <summary>
        /// Construct for a specific host.
        /// </summary>
        /// <param name="host">The host or IP address to ping.</param>
        /// <param name="remotePeerAddress">The address of the remote peer to get the data for. If null or empty, all peer's data.</param>
        /// <param name="count">The number of packets to send.</param>
        /// <param name="timeout">The timeout of a single ping.</param>
        /// <param name="maxHops">The maximum number of hops.</param>
        /// <param name="querierOptions">The querier options to use.</param>
        public TracerouteAction(string host, string remotePeerAddress, int count, TimeSpan timeout, int maxHops, FromUrlQueryQuerierOptions querierOptions)
        {
            this.maxHops = maxHops;
            this.timeout = timeout;
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentNullException(nameof(host), "Host is null, empty or white-space-only");
            }

            this.host = host;
            this.remotePeerAddress = remotePeerAddress;
            this.count = count;
            this.querierOptions = querierOptions ?? new FromUrlQueryQuerierOptions();

            this.querierOptions.AllowedApis = QueryApis.VendorSpecific; // currently we only support vendor-specific API - no matter what user requests
        }

        /// <summary>
        /// Asynchronuously executes the ping task.
        /// </summary>
        /// <returns></returns>
        internal async Task<ActionResult<IStatusReply>> Execute()
        {
            return await Task.Run(this.DoTraceroute);
        }

        private ActionResult<IStatusReply> DoTraceroute()
        {
            try
            {
                using (var querier = SnmpQuerierFactory.Instance.Create(this.host, this.querierOptions))
                {
                    ITracerouteResult tracerouteResult = querier.Traceroute(this.remotePeerAddress, Convert.ToUInt32(this.count), this.timeout, this.maxHops);

                    return new TracerouteWebResult(tracerouteResult);
                }
            }
            catch (Exception ex)
            {
                return new ErrorReply(ex);
            }
        }

        /// <summary>
        /// Container for a list of BGP peer results.
        /// </summary>
        private class TracerouteWebResult : ITracerouteWebResult
        {
            List<ITracerouteWebHop> hopResultsBacking = null;

            /// <summary>
            /// Construct from the querier interface container.
            /// </summary>
            /// <param name="tracerouteResult">The peers to construct from.</param>
            public TracerouteWebResult(ITracerouteResult tracerouteResult)
            {
                this.hopResultsBacking = (tracerouteResult == null) ? new List<ITracerouteWebHop>() : tracerouteResult.Hops.Select(p => new TracerouteWebHop(p) as ITracerouteWebHop).ToList();
                this.FromAddress = tracerouteResult.FromAddress?.ToString();
                this.ToAddress = tracerouteResult.ToAddress?.ToString();
            }

            /// <inheritdoc />
            public int HopCount => this.hopResultsBacking.Count;

            /// <inheritdoc />
            public IEnumerable<string> ErrorDetails { get; } = Enumerable.Empty<string>();

            /// <inheritdoc />
            public string FromAddress { get; }

            /// <inheritdoc />
            public string ToAddress { get; }

            /// <inheritdoc />
            public IReadOnlyList<ITracerouteWebHop> Hops => this.hopResultsBacking;
        }

        /// <summary>
        /// Container for a single BGP peer result.
        /// </summary>
        private class TracerouteWebHop : ITracerouteWebHop
        {
            /// <summary>
            /// Construct from a querier traceroute hop.
            /// </summary>
            /// <param name="querierHop">The querier hop to construct from.</param>
            public TracerouteWebHop(ITracerouteHop querierHop)
            {
                this.Address = querierHop.Address?.ToString();
                this.LossPercent = querierHop.LossPercent;
                this.SentCount = querierHop.SentCount;
                this.Status = querierHop.Status;
                this.LastRttMs = querierHop.LastRtt;
                this.AverageRttMs = querierHop.AverageRtt;
                this.BestRttMs = querierHop.BestRtt;
                this.WorstRttMs = querierHop.WorstRtt;
            }

            /// <inheritdoc />
            public string Address { get; }

            /// <inheritdoc />
            public double LossPercent { get; }

            /// <inheritdoc />
            public int SentCount { get; }

            /// <inheritdoc />
            public string Status { get; }

            /// <inheritdoc />
            public double LastRttMs { get; }

            /// <inheritdoc />
            public double AverageRttMs { get; }

            /// <inheritdoc />
            public double BestRttMs { get; }

            /// <inheritdoc />
            public double WorstRttMs { get; }
        }
    }
}
