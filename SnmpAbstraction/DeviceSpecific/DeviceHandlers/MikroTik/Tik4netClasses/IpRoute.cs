namespace tik4net.Objects.Ip
{
    /// <summary>
    /// /ip/route: 
    /// </summary>
    [TikEntity("/ip/route")]
    public class IpRoute
    {
        /// <summary>
        /// .id: 
        /// </summary>
        [TikProperty(".id", IsReadOnly = true, IsMandatory = true)]
        public string Id { get; private set; }

        /// <summary>
        /// dst-address: 
        /// </summary>
        [TikProperty("dst-address")]
        public string DstAddress { get; set; }

        /// <summary>
        /// gateway: 
        /// </summary>
        [TikProperty("gateway")]
        public string Gateway { get; set; }

        /// <summary>
        /// gateway-status: 
        /// </summary>
        [TikProperty("gateway-status")]
        public string GatewayStatus { get; set; }

        /// <summary>
        /// distance: 
        /// </summary>
        [TikProperty("distance")]
        public long Distance { get; set; }

        /// <summary>
        /// scope: 
        /// </summary>
        [TikProperty("scope")]
        public long Scope { get; set; }

        /// <summary>
        /// target-scope: 
        /// </summary>
        [TikProperty("target-scope")]
        public long TargetScope { get; set; }

        /// <summary>
        /// active: 
        /// </summary>
        [TikProperty("active")]
        public bool Active { get; set; }

        /// <summary>
        /// static: 
        /// </summary>
        [TikProperty("static")]
        public bool Static { get; set; }

        /// <summary>
        /// disabled: 
        /// </summary>
        [TikProperty("disabled")]
        public bool Disabled { get; set; }

        /// <summary>
        /// bgp-as-path: 
        /// </summary>
        [TikProperty("bgp-as-path")]
        public string BgpAsPath { get; set; }

        /// <summary>
        /// bgp-origin: 
        /// </summary>
        [TikProperty("bgp-origin")]
        public string BgpOrigin { get; set; }

        /// <summary>
        /// bgp-communities: 
        /// </summary>
        [TikProperty("bgp-communities")]
        public string BgpCommunities { get; set; }

        /// <summary>
        /// received-from: 
        /// </summary>
        [TikProperty("received-from")]
        public string ReceivedFrom { get; set; }

        /// <summary>
        /// dynamic: 
        /// </summary>
        [TikProperty("dynamic", IsReadOnly = true)]
        public bool Dynamic { get; private set; }

        /// <summary>
        /// bgp: 
        /// </summary>
        [TikProperty("bgp")]
        public bool Bgp { get; set; }

        /// <summary>
        /// pref-src: 
        /// </summary>
        [TikProperty("pref-src")]
        public string PrefSrc { get; set; }

        /// <summary>
        /// connect: 
        /// </summary>
        [TikProperty("connect")]
        public bool Connect { get; set; }
    }
}
