using System;
using System.Net;
using SnmpAbstraction;
using SnmpSharpNet;

namespace RestService.DataFetchingService
{
    /// <summary>
    /// Container that copies and stores the link detail received via constructor.
    /// </summary>
    internal class LinkDetailStoreOnlyContainer : ILinkDetail
    {
        /// <summary>
        /// Construct by copying another <see cref="ILinkDetail" />
        /// </summary>
        /// <param name="linkDetail">The link detail to copy and store.</param>
        public LinkDetailStoreOnlyContainer(ILinkDetail linkDetail)
        {
            // NOTE: These assignment might look really dumb.
            //       But remember: The rhs might be a lazy container for which exactly this access triggers the evaluation!
            this.MacString1 = linkDetail.MacString1;
            this.MacString2 = linkDetail.MacString2;
            this.Address1 = linkDetail.Address1;
            this.Address2 = linkDetail.Address2;
            this.ModelAndVersion1 = linkDetail.ModelAndVersion1;
            this.ModelAndVersion2 = linkDetail.ModelAndVersion2;
            this.RxLevel1at2 = linkDetail.RxLevel1at2;
            this.RxLevel2at1 = linkDetail.RxLevel2at1;
            this.LinkUptime = linkDetail.LinkUptime;
            this.SideOfAccessPoint = linkDetail.SideOfAccessPoint;
            this.DeviceAddress = linkDetail.DeviceAddress;
            this.DeviceModel = linkDetail.DeviceModel;
            
            // assign this last so it contains the sum of the possibly lazy evaluations triggered by above assignments
            this.QueryDuration = linkDetail.QueryDuration;
        }

        public string MacString1 { get; }

        public string MacString2 { get; }

        public IPAddress Address1 { get; }

        public IPAddress Address2 { get; }

        public string ModelAndVersion1 { get; }

        public string ModelAndVersion2 { get; }

        public double RxLevel1at2 { get; }

        public double RxLevel2at1 { get; }

        public TimeSpan LinkUptime { get; }

        public int? SideOfAccessPoint { get; }

        public IpAddress DeviceAddress { get; }

        public string DeviceModel { get; }

        public TimeSpan QueryDuration { get; }

        public void ForceEvaluateAll()
        {
            // NOP here - we're a simple, stupid container
        }
    }
}