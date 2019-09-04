namespace SnmpAbstraction
{
    /// <summary>
    /// Interface that serves as a handler for a specific device.
    /// </summary>
    public interface IDeviceHandler
    {
        /// <summary>
        /// Gets the device's generic system data.
        /// </summary>
        IDeviceSystemData SystemData { get; }

        /// <summary>
        /// Gets the device's interface details like interface type, MAC, IP, ...
        /// </summary>
        /// <value>A lazy-evaluated interface to the data.</value>
        IInterfaceDetails NetworkInterfaceDetails { get; }
    }
}