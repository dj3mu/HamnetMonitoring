namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Interface to the data of single host entry
    /// </summary>
    internal interface IHostData
    {
        /// <summary>
        /// Gets the IP address of the host.
        /// </summary>
        string Address { get; }

        /// <summary>
        /// Gets the name of the device as retrieved from the device itself (e.g. via SNMP or API).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of the device as retrieved from the device itself (e.g. via SNMP or API).
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the location of the device as retrieved from the device itself (e.g. via SNMP or API).
        /// </summary>
        string Location { get; }

        /// <summary>
        /// Gets the maintainer contact name of the device as retrieved from the device itself (e.g. via SNMP or API).
        /// </summary>
        string Contact { get; }

        /// <summary>
        /// Gets the device model string as retrieved from the device itself (e.g. via SNMP or API).
        /// </summary>
        string DeviceModel { get; }

        /// <summary>
        /// Gets the firmware version of the device as retrieved from the device itself (e.g. via SNMP or API).
        /// </summary>
        string DeviceVersion { get; }

        /// <summary>
        /// Gets a comma-separated list of features that this device supports.
        /// </summary>
        string SupportedFeatures { get; }

        /// <summary>
        /// Gets the time stamp at which this device entry has last been updated.
        /// </summary>
        string LastUpdated { get; }

        /// <summary>
        /// Gets the API used (by default) with this device.
        /// </summary>
        string DefaultApi { get; }
    }
}
