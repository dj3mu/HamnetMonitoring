namespace tik4net.Objects.Routing.Bgp
{
    /// <summary>
    /// /routing/bgp/network: 
    /// </summary>
    [TikEntity("/routing/bgp/network")]
    public class BgpNetwork
    {
        /// <summary>
        /// .id: 
        /// </summary>
        [TikProperty(".id", IsReadOnly = true, IsMandatory = true)]
        public string Id { get; private set; }

        /// <summary>
        /// network: 
        /// </summary>
        [TikProperty("network")]
        public string Network { get; set; }

        /// <summary>
        /// synchronize: 
        /// </summary>
        [TikProperty("synchronize")]
        public bool Synchronize { get; set; }

        /// <summary>
        /// disabled: 
        /// </summary>
        [TikProperty("disabled")]
        public bool Disabled { get; set; }
    }
}
