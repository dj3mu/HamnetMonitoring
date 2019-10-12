namespace tik4net.Objects.Routing.Bgp
{
    /// <summary>
    /// /routing/bgp/peer: 
    /// </summary>
    [TikEntity("/routing/bgp/peer")]
    public class BgpPeer
    {
        /// <summary>
        /// .id: 
        /// </summary>
        [TikProperty(".id", IsReadOnly = true, IsMandatory = true)]
        public string Id { get; private set; }

        /// <summary>
        /// name: 
        /// </summary>
        [TikProperty("name", IsMandatory = true)]
        public string Name { get; set; }

        /// <summary>
        /// instance: 
        /// </summary>
        [TikProperty("instance")]
        public string Instance { get; set; }

        /// <summary>
        /// remote-address: 
        /// </summary>
        [TikProperty("remote-address")]
        public string RemoteAddress { get; set; }

        /// <summary>
        /// remote-as: 
        /// </summary>
        [TikProperty("remote-as")]
        public long RemoteAs { get; set; }

        /// <summary>
        /// nexthop-choice: 
        /// </summary>
        [TikProperty("nexthop-choice")]
        public string NexthopChoice { get; set; }

        /// <summary>
        /// multihop: 
        /// </summary>
        [TikProperty("multihop")]
        public bool Multihop { get; set; }

        /// <summary>
        /// route-reflect: 
        /// </summary>
        [TikProperty("route-reflect")]
        public bool RouteReflect { get; set; }

        /// <summary>
        /// hold-time: 
        /// </summary>
        [TikProperty("hold-time")]
        public string HoldTime { get; set; }

        /// <summary>
        /// ttl: 
        /// </summary>
        [TikProperty("ttl")]
        public string Ttl { get; set; }

        /// <summary>
        /// address-families: 
        /// </summary>
        [TikProperty("address-families")]
        public string AddressFamilies { get; set; }

        /// <summary>
        /// default-originate: 
        /// </summary>
        [TikProperty("default-originate")]
        public string DefaultOriginate { get; set; }

        /// <summary>
        /// remove-private-as: 
        /// </summary>
        [TikProperty("remove-private-as")]
        public bool RemovePrivateAs { get; set; }

        /// <summary>
        /// as-override: 
        /// </summary>
        [TikProperty("as-override")]
        public bool AsOverride { get; set; }

        /// <summary>
        /// passive: 
        /// </summary>
        [TikProperty("passive")]
        public bool Passive { get; set; }

        /// <summary>
        /// use-bfd: 
        /// </summary>
        [TikProperty("use-bfd")]
        public bool UseBfd { get; set; }

        /// <summary>
        /// remote-id: 
        /// </summary>
        [TikProperty("remote-id")]
        public string RemoteId { get; set; }

        /// <summary>
        /// local-address: 
        /// </summary>
        [TikProperty("local-address")]
        public string LocalAddress { get; set; }

        /// <summary>
        /// uptime: 
        /// </summary>
        [TikProperty("uptime")]
        public string Uptime { get; set; }

        /// <summary>
        /// prefix-count: 
        /// </summary>
        [TikProperty("prefix-count")]
        public long PrefixCount { get; set; }

        /// <summary>
        /// updates-sent: 
        /// </summary>
        [TikProperty("updates-sent")]
        public long UpdatesSent { get; set; }

        /// <summary>
        /// updates-received: 
        /// </summary>
        [TikProperty("updates-received")]
        public long UpdatesReceived { get; set; }

        /// <summary>
        /// withdrawn-sent: 
        /// </summary>
        [TikProperty("withdrawn-sent")]
        public long WithdrawnSent { get; set; }

        /// <summary>
        /// withdrawn-received: 
        /// </summary>
        [TikProperty("withdrawn-received")]
        public long WithdrawnReceived { get; set; }

        /// <summary>
        /// remote-hold-time: 
        /// </summary>
        [TikProperty("remote-hold-time")]
        public string RemoteHoldTime { get; set; }

        /// <summary>
        /// used-hold-time: 
        /// </summary>
        [TikProperty("used-hold-time")]
        public string UsedHoldTime { get; set; }

        /// <summary>
        /// used-keepalive-time: 
        /// </summary>
        [TikProperty("used-keepalive-time")]
        public string UsedKeepaliveTime { get; set; }

        /// <summary>
        /// refresh-capability: 
        /// </summary>
        [TikProperty("refresh-capability")]
        public bool RefreshCapability { get; set; }

        /// <summary>
        /// as4-capability: 
        /// </summary>
        [TikProperty("as4-capability")]
        public bool As4Capability { get; set; }

        /// <summary>
        /// state: 
        /// </summary>
        [TikProperty("state")]
        public string State { get; set; }

        /// <summary>
        /// established: 
        /// </summary>
        [TikProperty("established")]
        public bool Established { get; set; }

        /// <summary>
        /// disabled: 
        /// </summary>
        [TikProperty("disabled")]
        public bool Disabled { get; set; }
    }
}
