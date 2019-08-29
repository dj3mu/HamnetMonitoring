namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the central querier object that queries for delivers specific data.
    /// </summary>
    public interface IHamnetSnmpQuerier
    {
        /// <summary>
        /// Queries for the basic device data (i.e. the .1.3.6.1.2.1.1 subtree)
        /// </summary>
        /// <returns>A data container with the device's system data.</returns>
        IDeviceSystemData QuerySystemData();
    }
}
