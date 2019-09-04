namespace SnmpAbstraction
{
    /// <summary>
    /// Interface for a class that allows detection of devices.
    /// </summary>
    internal interface IDetectableDevice
    {
        /// <summary>
        /// Checks if the device is applicable (i.e. detected) to the device that
        /// the give lower layer talks to.
        /// </summary>
        /// <param name="snmpLowerLayer">The lower layer for further commincation to the device.</param>
        /// <returns><c>true</c> if the device connected via <see parmref="snmpLowerLayer" /> is one that this detectable device represents.</returns>
        bool IsApplicable(ISnmpLowerLayer snmpLowerLayer);

        /// <summary>
        /// Creates the device handler for the device represented by this detectable device instance.<br/>
        /// That handler is what will susequently be used to query the device.
        /// </summary>
        /// <param name="lowerLayer">The lower layer for further commincation to the device.</param>
        /// <returns>The device handler for the device represented by this detectable device instance.</returns>
        IDeviceHandler CreateHandler(ISnmpLowerLayer lowerLayer);
    }
}