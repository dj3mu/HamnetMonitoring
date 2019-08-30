namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the central querier object that queries for delivers specific data.
    /// </summary>
    public interface IHamnetSnmpQuerier
    {
        /// <summary>
        /// Gets the device system data (i.e. the .1.3.6.1.2.1.1 subtree which is mainly device-agnostic).
        /// </summary>
        /// <remarks>This data will implicitly be queried when a instance of an SNMP Querier initialized.<br/>
        /// This is because it includes the data that is needed to determine how to talk to the device.</remarks>
        IDeviceSystemData SystemData { get; }
    }
}
