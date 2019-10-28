using System;
using System.Collections.Generic;
using System.Linq;
using SnmpAbstraction;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Container for a list link details replies.
    /// </summary>
    internal class LinkDetailsReply : ILinkDetailsReply
    {
        /// <summary>
        /// Construct from error info and link details.
        /// </summary>
        /// <param name="linkDetails">The link details.</param>
        public LinkDetailsReply(ILinkDetails linkDetails)
        {
            if (linkDetails == null)
            {
                throw new ArgumentNullException(nameof(linkDetails), "The link details to return is null");
            }

            linkDetails.ForceEvaluateAll();

            this.Details = linkDetails.Details.Select(d => new LinkDetailReply(d)).ToList();
        }

        /// <summary>
        /// Construct from error info and link details.
        /// </summary>
        /// <param name="linkDetails">The link details.</param>
        public LinkDetailsReply(IEnumerable<ILinkDetailReply> linkDetails)
        {
            if (linkDetails == null)
            {
                throw new ArgumentNullException(nameof(linkDetails), "The link details to return is null");
            }

            this.Details = linkDetails as IReadOnlyList<ILinkDetailReply> ?? linkDetails.ToList();
        }

        /// <inheritdoc />
        public IReadOnlyList<ILinkDetailReply> Details { get; }

        /// <inheritdoc />
        public IEnumerable<string> ErrorDetails { get; } = Enumerable.Empty<string>();

        /// <summary>
        /// Container for a single link test detail reply.
        /// </summary>
        internal class LinkDetailReply : ILinkDetailReply
        {
            public LinkDetailReply(ILinkDetail linkDetail)
            {
                if (linkDetail == null)
                {
                    throw new ArgumentNullException(nameof(linkDetail), "The link detail to construct from is null");
                }

                this.MacString1 = linkDetail.MacString1;
                this.MacString2 = linkDetail.MacString2;
                this.Address1 = linkDetail.Address1.ToString();
                this.Address2 = linkDetail.Address2.ToString();
                this.ModelAndVersion1 = linkDetail.ModelAndVersion1;
                this.ModelAndVersion2 = linkDetail.ModelAndVersion2;

                try
                {
                    this.RxLevel1at2 = linkDetail.RxLevel1at2;
                }
                catch(HamnetSnmpException)
                {
                    this.RxLevel1at2 = double.NaN;
                }

                try
                {
                    this.RxLevel2at1 = linkDetail.RxLevel2at1;
                }
                catch(HamnetSnmpException)
                {
                    this.RxLevel2at1 = double.NaN;
                }

                try
                {
                    this.LinkUptime = linkDetail.LinkUptime;
                }
                catch(HamnetSnmpException)
                {
                    this.LinkUptime = TimeSpan.Zero;
                }

                try
                {
                    this.SideOfAccessPoint = linkDetail.SideOfAccessPoint;
                }
                catch(HamnetSnmpException)
                {
                    this.SideOfAccessPoint = null;
                }
            }

            /// <inheritdoc />
            public string MacString1 { get; }

            /// <inheritdoc />
            public string MacString2 { get; }

            /// <inheritdoc />
            public string Address1 { get; }

            /// <inheritdoc />
            public string Address2 { get; }

            /// <inheritdoc />
            public string ModelAndVersion1 { get; }

            /// <inheritdoc />
            public string ModelAndVersion2 { get; }

            /// <inheritdoc />
            public double RxLevel1at2 { get; }
            /// <inheritdoc />

            /// <inheritdoc />
            public double RxLevel2at1 { get; }

            /// <inheritdoc />
            public TimeSpan LinkUptime { get; }

            /// <inheritdoc />
            public int? SideOfAccessPoint { get; }
        }
    }
}
