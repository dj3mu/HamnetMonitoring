using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface for a class that allows detection of devices.
    /// </summary>
    internal interface IDetectableDevice
    {
        /// <summary>
        /// Gets the priority of this detectable device. This property is the ordering criteria when checking device applicability.
        /// Device with highest priority value wins and no lower-priority device will be checked any more.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Gets the API that is supported by this handler.
        /// </summary>
        QueryApis SupportedApi { get; }
        
        /// <summary>
        /// Checks if the device is applicable (i.e. detected) to the device that
        /// the give lower layer talks to.
        /// </summary>
        /// <param name="address">The IP address for further, proprietary, communication to the device.</param>
        /// <param name="options">The options to use.</param>
        /// <returns><c>true</c> if the device connected via <see parmref="snmpLowerLayer" /> is one that this detectable device represents.</returns>
        bool IsApplicableVendorSpecific(IpAddress address, IQuerierOptions options);

        /// <summary>
        /// Checks if the device is applicable (i.e. detected) to the device that
        /// the give lower layer talks to.
        /// </summary>
        /// <param name="snmpLowerLayer">The lower layer for further SNMP commincation to the device.</param>
        /// <param name="options">The options to use.</param>
        /// <returns><c>true</c> if the device connected via <see parmref="snmpLowerLayer" /> is one that this detectable device represents.</returns>
        bool IsApplicableSnmp(ISnmpLowerLayer snmpLowerLayer, IQuerierOptions options);

        /// <summary>
        /// Creates the device handler for the device represented by this detectable device instance.<br/>
        /// That handler is what will susequently be used to query the device.
        /// </summary>
        /// <param name="lowerLayer">The lower layer for further commincation to the device.</param>
        /// <param name="options">The options to use.</param>
        /// <returns>The device handler for the device represented by this detectable device instance.</returns>
        IDeviceHandler CreateHandler(ISnmpLowerLayer lowerLayer, IQuerierOptions options);

        /// <summary>
        /// Creates the device handler for the device represented by this detectable device instance.<br/>
        /// That handler is what will susequently be used to query the device.
        /// </summary>
        /// <param name="address">The IP address to use for talking to the device.</param>
        /// <param name="options">The options to use.</param>
        /// <returns>The device handler for the device represented by this detectable device instance.</returns>
        IDeviceHandler CreateHandler(IpAddress address, IQuerierOptions options);
    }
}