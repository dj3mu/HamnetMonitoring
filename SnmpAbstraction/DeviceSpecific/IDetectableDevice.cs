using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface for a class that allows detection of devices.
    /// </summary>
    internal interface IDetectableDevice
    {
        /// <summary>
        /// Gets the API that is supported by this handler.
        /// </summary>
        QueryApis SupportedApi { get; }
        
        /// <summary>
        /// Checks if the device is applicable (i.e. detected) to the device that
        /// the give lower layer talks to.
        /// </summary>
        /// <param name="address">The IP address for further, proprietary, commincation to the device.</param>
        /// <returns><c>true</c> if the device connected via <see parmref="snmpLowerLayer" /> is one that this detectable device represents.</returns>
        bool IsApplicableVendorSpecific(IpAddress address);

        /// <summary>
        /// Checks if the device is applicable (i.e. detected) to the device that
        /// the give lower layer talks to.
        /// </summary>
        /// <param name="snmpLowerLayer">The lower layer for further SNMP commincation to the device.</param>
        /// <returns><c>true</c> if the device connected via <see parmref="snmpLowerLayer" /> is one that this detectable device represents.</returns>
        bool IsApplicableSnmp(ISnmpLowerLayer snmpLowerLayer);

        /// <summary>
        /// Creates the device handler for the device represented by this detectable device instance.<br/>
        /// That handler is what will susequently be used to query the device.
        /// </summary>
        /// <param name="lowerLayer">The lower layer for further commincation to the device.</param>
        /// <param name="options">The options to use.</param>
        /// <returns>The device handler for the device represented by this detectable device instance.</returns>
        IDeviceHandler CreateHandler(ISnmpLowerLayer lowerLayer, IQuerierOptions options);
    }
}