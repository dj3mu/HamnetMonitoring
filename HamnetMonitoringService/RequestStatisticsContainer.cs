namespace HamnetDbRest
{
    /// <summary>
    /// Container for request statistics
    /// </summary>
    internal class RequestStatisticsContainer
    {
        /// <summary>
        /// Default-construct
        /// </summary>
        public RequestStatisticsContainer()
        {
        }

        /// <summary>
        /// Gets or sets the count of legacy RSSI API requests
        /// </summary>
        public ulong LegacyVzwRssiRequests { get; set; } = 0;

        /// <summary>
        /// Gets or sets the count of legacy RSSI API failing queries requests
        /// </summary>
        public int LegacyVzwRssiFailingRequests { get; internal set; } = 0;

        /// <summary>
        /// Gets or sets the count of APIv1 failing RSSI queries requests
        /// </summary>
        public int ApiV1RssiFailingRequests { get; internal set; } = 0;

        /// <summary>
        /// Gets or sets the count of APIv1 RSSI requests
        /// </summary>
        public int ApiV1RssiRequests { get; internal set; } = 0;

        /// <summary>
        /// Gets or sets the count of APIv1 host-specific BGP requests
        /// </summary>
        public int ApiV1BgpHostSpecificRequests { get; internal set; } = 0;

        /// <summary>
        /// Gets or sets the count of APIv1 BGP all monitored routers requests
        /// </summary>
        public int ApiV1BgpMonitoredRoutersRequests { get; internal set; } = 0;

        /// <summary>
        /// Gets or sets the count of APIv1 BGP all routers failed monitorings requests
        /// </summary>
        public int ApiV1BgpFailingRequests { get; internal set; } = 0;

        /// <summary>
        /// Gets or sets the count of APIv1 cache-info requests
        /// </summary>
        public int ApiV1CacheInfoRequests { get; internal set; } = 0;

        /// <summary>
        /// Gets or sets the count of APIv1 traceroute requests
        /// </summary>
        public int ApiV1TraceRouteRequests { get; internal set; } = 0;

        /// <summary>
        /// Gets or sets the count of APIv1 linktest/ping requests
        /// </summary>
        public int ApiV1LinkTestPingRequests { get; internal set; } = 0;

        /// <summary>
        /// Gets or sets the count of APIv1 linktest/info requests
        /// </summary>
        public int ApiV1LinkTestInfoRequests { get; internal set; } = 0;

        /// <summary>
        /// Gets or sets the count of APIv1 linktest/link requests
        /// </summary>
        public int ApiV1LinkTestLinkRequests { get; internal set; } = 0;

        /// <summary>
        /// Gets or sets the count of APIv1 linktest/network requests
        /// </summary>
        public int ApiV1LinkTestNetworkRequests { get; internal set; } = 0;

        /// <summary>
        /// Gets or sets the count of api/status requests
        /// </summary>
        public int ApiStatusRequests { get; internal set; } = 0;
    }
}