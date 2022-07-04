using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class VolatileFetchingInterfaceDetails : IInterfaceDetails
    {
        private readonly IInterfaceDetails underlyingInterfaceDetails;

        private readonly ISnmpLowerLayer lowerLayer;

        private readonly TimeSpan queryDurationBacking = TimeSpan.Zero;

        /// <summary>
        /// Construct for a lower layer and a cache data set (from which we return the non-volatile data).
        /// </summary>
        public VolatileFetchingInterfaceDetails(IInterfaceDetails interfaceDetails, ISnmpLowerLayer lowerLayer)
        {
            this.lowerLayer = lowerLayer ?? throw new System.ArgumentNullException(nameof(lowerLayer), "lower layer is null");
            this.underlyingInterfaceDetails = interfaceDetails ?? throw new System.ArgumentNullException(nameof(interfaceDetails), "underlying interface details is null");

            this.Details = interfaceDetails.Details.Select(underlyingInterfaceDetail => new VolatileFetchingInterfaceDetail(underlyingInterfaceDetail, this.lowerLayer)).ToList();
        }

        /// <inheritdoc />
        public IReadOnlyList<IInterfaceDetail> Details { get; }

        /// <inheritdoc />
        public IpAddress DeviceAddress => this.underlyingInterfaceDetails.DeviceAddress;

        /// <inheritdoc />
        public string DeviceModel => this.underlyingInterfaceDetails.DeviceModel;

        /// <inheritdoc />
        public TimeSpan QueryDuration => this.queryDurationBacking;

        /// <inheritdoc />
        public void ForceEvaluateAll()
        {
            // TODO: Trigger fetching of volatile data
        }

        /// <inheritdoc />
        public IEnumerator<IInterfaceDetail> GetEnumerator()
        {
            return this.Details.GetEnumerator();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder returnBuilder = new StringBuilder((this.Details?.Count ?? 1) * 128);

            returnBuilder.Append("Interface Details:");

            if (this.Details == null)
            {
                returnBuilder.Append(" Not yet retrieved");
            }
            else
            {
                foreach (var item in this.Details)
                {
                    returnBuilder.AppendLine().AppendLine(SnmpAbstraction.IndentLines(item.ToString()));
                }
            }

            return returnBuilder.ToString();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Details.GetEnumerator();
        }
    }
}