using System;

namespace SnmpAbstraction
{
    /// <summary>
    /// Container to combine the query result handles related to a single link (i.e. the connection between exactly two devices).
    /// </summary>
    internal class LinkRelatedResultCollection
    {
        /// <summary>
        /// Initialize with all parameters.
        /// </summary>
        /// <param name="interfaceDetail1">The interface details of side #1.</param>
        /// <param name="interfaceDetail2">The interface details of side #2</param>
        /// <param name="wirelessPeerInfo1">The wireless peer info of side #1.</param>
        /// <param name="wirelessPeerInfo2">The wireless peer info of side #2.</param>
        public LinkRelatedResultCollection(IInterfaceDetail interfaceDetail1, IInterfaceDetail interfaceDetail2, IWirelessPeerInfo wirelessPeerInfo1, IWirelessPeerInfo wirelessPeerInfo2)
        {
            this.WirelessPeerInfo1 = wirelessPeerInfo1;
            this.WirelessPeerInfo2 = wirelessPeerInfo2;
            this.InterfaceDetail1 = interfaceDetail1;
            this.InterfaceDetail2 = interfaceDetail2;
        }

        /// <summary>
        /// Gets the interface details of side #1.
        /// </summary>
        public IInterfaceDetail InterfaceDetail1 { get; }

        /// <summary>
        /// Gets the interface details of side #2.
        /// </summary>
        public IInterfaceDetail InterfaceDetail2 { get; }

        /// <summary>
        /// Gets the wireless peer info of side #1.
        /// </summary>
        public IWirelessPeerInfo WirelessPeerInfo1 { get; }

        /// <summary>
        /// Gets the wireless peer info of side #2.
        /// </summary>
        public IWirelessPeerInfo WirelessPeerInfo2 { get; }

        /// <summary>
        /// The sum all all stored result's query durations.
        /// </summary>
        public TimeSpan TotalQueryDuration 
        {
            get
            {
                // we have to re-calculate every time as the results might be lazy-evaluated
                TimeSpan queryDurationSum = TimeSpan.Zero;
                if (this.InterfaceDetail1 != null)
                {
                    queryDurationSum += this.InterfaceDetail1.QueryDuration;
                }

                if (this.InterfaceDetail2 != null)
                {
                    queryDurationSum += this.InterfaceDetail2.QueryDuration;
                }

                if (this.WirelessPeerInfo1 != null)
                {
                    queryDurationSum += this.WirelessPeerInfo1.QueryDuration;
                }

                if (this.WirelessPeerInfo2 != null)
                {
                    queryDurationSum += this.WirelessPeerInfo2.QueryDuration;
                }

                return queryDurationSum;
            }
        }
    }
}