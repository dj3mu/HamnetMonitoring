namespace tik4net.Objects.Routing.Bgp
{
    /// <summary>
    /// Mikrotik BGP advertisements
    /// /routing/bgp/advertisements: 
    /// </summary>
    [TikEntity("routing/bgp/advertisements", IsReadOnly = true)]
    public class BgpAdvertisement
    {
        /// <summary>
        /// .id: 
        /// </summary>
        [TikProperty(".id", IsReadOnly = true, IsMandatory = true)]
        public string Id { get; private set; }

        /// <summary>
        /// peer: 
        /// </summary>
        [TikProperty("peer")]
        public string Peer { get; set; }

        /// <summary>
        /// prefix: 
        /// </summary>
        [TikProperty("prefix")]
        public string Prefix { get; set; }

        /// <summary>
        /// nexthop: 
        /// </summary>
        [TikProperty("nexthop")]
        public string Nexthop { get; set; }

        /// <summary>
        /// as-path: 
        /// </summary>
        [TikProperty("as-path")]
        public string AsPath { get; set; }

        /// <summary>
        /// origin: 
        /// </summary>
        [TikProperty("origin")]
        public string Origin { get; set; }

        /// <summary>
        /// communities: 
        /// </summary>
        [TikProperty("communities")]
        public string Communities { get; set; }
    }
}
