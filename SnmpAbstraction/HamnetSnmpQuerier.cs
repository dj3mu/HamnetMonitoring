namespace SnmpAbstraction
{
    /// <summary>
    /// The main SNMP Querier implementation.
    /// </summary>
    internal class HamnetSnmpQuerier : IHamnetSnmpQuerier
    {
        /// <summary>
        /// The device handler to use for obtaining SNMP data.
        /// </summary>
        private IDeviceHandler handler;

        /// <summary>
        /// Creates a new instance using the specified device handler.
        /// </summary>
        /// <param name="handler">The device handler to use for obtaining SNMP data.</param>
        public HamnetSnmpQuerier(IDeviceHandler handler)
        {
            this.handler = handler;
        }

        /// <inheritdoc />
        public IDeviceSystemData SystemData => this.handler.SystemData;

        /// <inheritdoc />
        public IInterfaceDetails NetworkInterfaceDetails => this.handler.NetworkInterfaceDetails;
    }
}
