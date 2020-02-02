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
        public TracerrouteHop(HamnetToolTraceroute tik4netTracerouteHop)
        {
            if (tik4netTracerouteHop == null)
            {
                throw new ArgumentNullException(nameof(tik4netTracerouteHop), "The tik4net traceroute hop is null when constructing a TracerouteHop");
            }

            if (string.IsNullOrWhiteSpace(tik4netTracerouteHop.Address))
            {
                this.Address = null;
            }
            else
            {
                if (IPAddress.TryParse(tik4netTracerouteHop.Address, out IPAddress parsed))
                {
                    this.Address = parsed;
                }
                else
                {
                    this.Address = null;
                }
            }

            if (string.IsNullOrWhiteSpace(tik4netTracerouteHop.Loss))
            {
                this.LossPercent = double.NaN;
            }
            else
            {
                if (double.TryParse(tik4netTracerouteHop.Loss, out double parsed))
                {
                    this.LossPercent = parsed;
                }
                else
                {
                    this.LossPercent = double.NaN;
                }
            }

            this.SentCount = tik4netTracerouteHop.Sent;

            // some micro-expert-system filling the (almost always) empty status field with something useful derived from other values
            if (!string.IsNullOrWhiteSpace(tik4netTracerouteHop.Status))
            {
                this.Info = tik4netTracerouteHop.Status;
            }
            else
            {
                if (this.Address == null)
                {
                    this.Info = "lost";
                }
                else if (string.IsNullOrWhiteSpace(tik4netTracerouteHop.Last))
                {
                    this.Info = "timeout";
                }
                else
                {
                    this.Info = this.LossPercent > 0 ? "unstable" : "ok";
                }
            }

            if (this.Address == null)
            {
                this.Status = HopStatus.Lost;
            }
            else if (tik4netTracerouteHop.Status?.ToUpperInvariant()?.Contains("UNREACHABLE") ?? false)
            {
                this.Status = HopStatus.Unreachable;
            }
            else if (string.IsNullOrWhiteSpace(tik4netTracerouteHop.Last))
            {
                this.Status = HopStatus.Timeout;
            }
            else
            {
                this.Status = this.LossPercent > 0 ? HopStatus.Unstable : HopStatus.Ok;
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

            if (string.IsNullOrWhiteSpace(tik4netTracerouteHop.Avg))
            {
                this.AverageRtt = double.NaN;
            }
            else
            {
                if (double.TryParse(tik4netTracerouteHop.Avg, out double value))
                {
                    this.AverageRtt = value;
                }
                else
                {
                    this.AverageRtt = double.NaN;
                }
            }

            if (string.IsNullOrWhiteSpace(tik4netTracerouteHop.Best))
            {
                this.BestRtt = double.NaN;
            }
            else
            {
                if (double.TryParse(tik4netTracerouteHop.Best, out double value))
                {
                    this.BestRtt = value;
                }
                else
                {
                    this.BestRtt = double.NaN;
                }
            }

            if (string.IsNullOrWhiteSpace(tik4netTracerouteHop.Worst))
            {
                this.WorstRtt = double.NaN;
            }
            else
            {
                if (double.TryParse(tik4netTracerouteHop.Worst, out double value))
                {
                    this.WorstRtt = value;
                }
                else
                {
                    this.WorstRtt = double.NaN;
                }
            }
        }

        /// <inheritdoc />
        public IPAddress Address { get; }

        /// <inheritdoc />
        public double LossPercent { get; }

        /// <inheritdoc />
        public int SentCount { get; }

        /// <inheritdoc />
        public double LastRtt { get; }

        /// <inheritdoc />
        public double AverageRtt { get; }

        /// <inheritdoc />
        public double BestRtt { get; }

        /// <inheritdoc />
        public double WorstRtt { get; }

        /// <inheritdoc />
        public string Info { get; }

        /// <inheritdoc />
        public HopStatus Status { get; }
    }
}