using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Container for a list of retrieved link details.
    /// </summary>
    internal class LinkDetails : HamnetSnmpQuerierResultBase, ILinkDetails
    {
        /// <summary>
        /// Construct the container populating all fields.
        /// </summary>
        /// <param name="fetchedDetails">The feteched link details.</param>
        /// <param name="addressOfSide1">The address of side 1 device.</param>
        /// <param name="deviceModel">The model and version of the device.</param>
        public LinkDetails(IEnumerable<ILinkDetail> fetchedDetails, IpAddress addressOfSide1, string deviceModel)
            : base(addressOfSide1, deviceModel, TimeSpan.Zero)
        {
            this.Details = fetchedDetails as IReadOnlyList<ILinkDetail> ?? fetchedDetails?.ToList() ?? new List<ILinkDetail>();
        }

        /// <inheritdoc />
        public IReadOnlyList<ILinkDetail> Details { get; }

        public override TimeSpan QueryDuration
        {
            get
            {
                if (this.Details == null)
                {
                    return TimeSpan.Zero;
                }

                return this.Details.Aggregate(TimeSpan.Zero, (a, c) => a += c.QueryDuration);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder returnBuilder = new StringBuilder((this.Details?.Count ?? 1) * 128);

            returnBuilder.Append("Link Details:");

            if (this.Details.Count == 0)
            {
                returnBuilder.Append(" No link found.");
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
    }
}
