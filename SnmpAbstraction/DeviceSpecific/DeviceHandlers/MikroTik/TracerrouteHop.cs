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

            // some micro-expert-system filling the (almost always) empty status field with something useful derived from other values
            if (!string.IsNullOrWhiteSpace(tik4netTracerouteHop.Status))
            {
                this.Status = tik4netTracerouteHop.Status;
            }
            else
            {
                if (this.Address == null)
                {
                    this.Status = "lost";
                }
                else if (string.IsNullOrWhiteSpace(tik4netTracerouteHop.Last))
                {
                    this.Status = "timeout";
                }
                else
                {
                    this.Status = this.LossPercent > 0 ? "unstable" : "ok";
                }
            }

            if (string.IsNullOrWhiteSpace(tik4netTracerouteHop.Last))
            {
                this.LastRtt = double.NaN;
            }
            else
            {
                if (double.TryParse(tik4netTracerouteHop.Last, out double parsedRtt))
                {
                    this.LastRtt = parsedRtt;
                }
                else if (tik4netTracerouteHop.Last.ToUpperInvariant().Contains("TIMEOUT"))
                {
                    this.LastRtt = double.PositiveInfinity;
                }
                else
                {
                    this.LastRtt = double.NaN;
                }
            }

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