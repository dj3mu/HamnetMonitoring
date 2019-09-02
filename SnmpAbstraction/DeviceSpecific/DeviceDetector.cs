using System;
using System.Collections.Generic;

namespace SnmpAbstraction
{
    /// <summary>
    /// Class handling the detection of devices types with enough precsision to decided about the
    /// class type that is needed to further talk to this device.
    /// </summary>
    internal class DeviceDetector
    {
        /// <summary>
        /// The communication layer.
        /// </summary>
        private readonly ISnmpLowerLayer lowerLayer;

        /// <summary>
        /// Represents the list of detectable device objects that will be queried for applicability one after the other.
        /// </summary>
        private readonly List<IDetectableDevice> detectableDevices = new List<IDetectableDevice>
        {
            new MikrotikDetectableDevice()
        };

        /// <summary>
        /// Creates of detector using the given lower communication layer.
        /// </summary>
        /// <param name="lowerLayer">The lower layer to talk to the device of which the type shall be detected.</param>
        public DeviceDetector(ISnmpLowerLayer lowerLayer)
        {
            this.lowerLayer = lowerLayer ?? throw new ArgumentNullException(nameof(lowerLayer), "lower communiation layer for device detection is null");
        }

        /// <summary>
        /// Trigger detection of the device based.
        /// </summary>
        /// <param name="cachedSystemData">The cached system data.<br/>
        /// Idea is, that this might already be sufficient to finally decide about a specific device and thus decision can be done without doing any network communiation.</param>
        /// <returns>A <see cref="IDetectableDevice" /> representing the detected device or null if the device is unknown and cannot be detected.</returns>
        public IDeviceHandler Detect()
        {
            foreach (IDetectableDevice currentDevice in this.detectableDevices)
            {
                if (currentDevice.IsApplicable(this.lowerLayer))
                {
                    return currentDevice.CreateHandler(this.lowerLayer);
                }
            }

            return null;
        }
    }
}
