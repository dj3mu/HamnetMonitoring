using System;
using System.Net;
using tik4net.Objects.Tool;

namespace SnmpAbstraction
{
    /// <summary>
    /// Container for a single traceroute hop
    /// </summary>
    internal class TracerrouteHop : ITracerouteHop
    {
        public TracerrouteHop(ToolTraceroute tik4netTracerouteHop)
        {
            if (tik4netTracerouteHop == null)
            {
                throw new ArgumentNullException(nameof(tik4netTracerouteHop), "The tik4net traceroute hop is null when constructing a TracerouteHop");
            }

            this.Address = string.IsNullOrWhiteSpace(tik4netTracerouteHop.Address) ? null : IPAddress.Parse(tik4netTracerouteHop.Address);
            this.LossPercent = Convert.ToDouble(tik4netTracerouteHop.Loss);
            this.SentCount = tik4netTracerouteHop.Sent;
            this.Status = tik4netTracerouteHop.Status ?? string.Empty;
            this.LastRtt = string.IsNullOrWhiteSpace(tik4netTracerouteHop.Last) ? double.NaN : double.Parse(tik4netTracerouteHop.Last);
            this.AverageRtt = string.IsNullOrWhiteSpace(tik4netTracerouteHop.Avg) ? double.NaN : double.Parse(tik4netTracerouteHop.Avg);
            this.BestRtt = string.IsNullOrWhiteSpace(tik4netTracerouteHop.Best) ? double.NaN : double.Parse(tik4netTracerouteHop.Best);
            this.WorstRtt = string.IsNullOrWhiteSpace(tik4netTracerouteHop.Worst) ? double.NaN : double.Parse(tik4netTracerouteHop.Worst);
        }

        /// <inheritdoc />
        public IPAddress Address { get; }

        /// <inheritdoc />
        public double LossPercent { get; }

        /// <inheritdoc />
        public int SentCount { get; }

        /// <inheritdoc />
        public string Status { get; }

        /// <inheritdoc />
        public double LastRtt { get; }

        /// <inheritdoc />
        public double AverageRtt { get; }

        /// <inheritdoc />
        public double BestRtt { get; }

        /// <inheritdoc />
        public double WorstRtt { get; }
    }
}