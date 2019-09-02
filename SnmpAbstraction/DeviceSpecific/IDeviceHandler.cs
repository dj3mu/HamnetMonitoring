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
    }
}