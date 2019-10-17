namespace tik4net.Objects.Routing.Bgp
{
    /// <summary>
    /// /routing/bgp/instance: 
    /// </summary>
    [TikEntity("/routing/bgp/instance")]
    public class BgpInstance
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
        /// as: 
        /// </summary>
        [TikProperty("as")]
        public long As { get; set; }

        /// <summary>
        /// router-id: 
        /// </summary>
        [TikProperty("router-id")]
        public string RouterId { get; set; }

        /// <summary>
        /// redistribute-connected: 
        /// </summary>
        [TikProperty("redistribute-connected")]
        public bool RedistributeConnected { get; set; }

        /// <summary>
        /// redistribute-static: 
        /// </summary>
        [TikProperty("redistribute-static")]
        public bool RedistributeStatic { get; set; }

        /// <summary>
        /// redistribute-rip: 
        /// </summary>
        [TikProperty("redistribute-rip")]
        public bool RedistributeRip { get; set; }

        /// <summary>
        /// redistribute-ospf: 
        /// </summary>
        [TikProperty("redistribute-ospf")]
        public bool RedistributeOspf { get; set; }

        /// <summary>
        /// redistribute-other-bgp: 
        /// </summary>
        [TikProperty("redistribute-other-bgp")]
        public bool RedistributeOtherBgp { get; set; }

        /// <summary>
        /// client-to-client-reflection: 
        /// </summary>
        [TikProperty("client-to-client-reflection")]
        public bool ClientToClientReflection { get; set; }

        /// <summary>
        /// ignore-as-path-len: 
        /// </summary>
        [TikProperty("ignore-as-path-len")]
        public bool IgnoreAsPathLen { get; set; }

        /// <summary>
        /// default: 
        /// </summary>
        [TikProperty("default")]
        public bool Default { get; set; }

        /// <summary>
        /// disabled: 
        /// </summary>
        [TikProperty("disabled")]
        public bool Disabled { get; set; }
    }
}
