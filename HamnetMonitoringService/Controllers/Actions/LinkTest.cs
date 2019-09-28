using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SnmpAbstraction;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Class doing a link test between two hosts specified by IP or host name.
    /// </summary>
    internal class LinkTest
    {
        private string host1;
        private string host2;

        /// <summary>
        /// Construct for a specific host.
        /// </summary>
        /// <param name="host1">The first host or IP of the link.</param>
        /// <param name="host2">The second host or IP of the link.</param>
        public LinkTest(string host1, string host2)
        {
            if (string.IsNullOrWhiteSpace(host1))
            {
                throw new ArgumentNullException(nameof(host1), "Host #1 is null, empty or white-space-only");
            }

            if (string.IsNullOrWhiteSpace(host2))
            {
                throw new ArgumentNullException(nameof(host2), "Host #2 is null, empty or white-space-only");
            }

            this.host1 = host1;
            this.host2 = host2;
        }

        /// <summary>
        /// Asynchronuously executes the ping task.
        /// </summary>
        /// <returns></returns>
        internal async Task<ActionResult<IStatusReply>> Execute()
        {
            return await Task.Run(this.DoLinkTest);
        }

        private ActionResult<IStatusReply> DoLinkTest()
        {
            try
            {
                var querier = SnmpQuerierFactory.Instance.Create(this.host1, QuerierOptions.Default);

                var linkDetails = querier.FetchLinkDetails(this.host2);

                return new LinkDetailsReply(linkDetails);
            }
            catch(Exception ex)
            {
                return new ErrorReply(ex);
            }
        }

        private class LinkDetailsReply : ILinkDetailsReply
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

                this.Details = linkDetails.Details.Select(d => new LinkDetailReply(d)).ToList();
            }

            /// <inheritdoc />
            public IReadOnlyList<ILinkDetailReply> Details { get; }

            /// <inheritdoc />
            public IEnumerable<string> ErrorDetails { get; } = Enumerable.Empty<string>();

            private class LinkDetailReply : ILinkDetailReply
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
}
