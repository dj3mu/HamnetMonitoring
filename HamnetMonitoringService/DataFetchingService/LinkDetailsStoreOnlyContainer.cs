using System;
using System.Collections.Generic;
using System.Linq;
using SnmpAbstraction;
using SnmpSharpNet;

namespace RestService.DataFetchingService
{
    /// <summary>
    /// Container that copies and stores the link details received via constructor.
    /// </summary>
    internal class LinkDetailsStoreOnlyContainer : ILinkDetails
    {
        /// <summary>
        /// Construct by copying another <see cref="ILinkDetails" />
        /// </summary>
        /// <param name="linkDetails">The link details to copy and store.</param>
        public LinkDetailsStoreOnlyContainer(ILinkDetails linkDetails)
        {
            this.DeviceAddress = linkDetails.DeviceAddress;
            this.DeviceModel = linkDetails.DeviceModel;

            this.Details = linkDetails.Details.Select(d => new LinkDetailStoreOnlyContainer(d)).ToList();
            
            // assign this last so it contains the sum of the possibly lazy evaluations triggered by above assignments
            this.QueryDuration = linkDetails.QueryDuration;
        }

        /// <inheritdoc />
        public IReadOnlyList<ILinkDetail> Details { get; }

        /// <inheritdoc />
        public IpAddress DeviceAddress { get; }

        /// <inheritdoc />
        public string DeviceModel { get; }

        /// <inheritdoc />
        public TimeSpan QueryDuration { get; }

        /// <inheritdoc />
        public void ForceEvaluateAll()
        {
            // NOP here - we're a simple, stupid container
        }
    }
}