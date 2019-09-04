using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SnmpAbstraction
{
    /// <summary>
    /// Class handling the detection of devices types with enough precsision to decided about the
    /// class type that is needed to further talk to this device.
    /// </summary>
    internal class DeviceDetector
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        /// Trigger detection of the device based.<br/>
        /// Idea is, that this might already be sufficient to finally decide about a specific device and thus decision can be done without doing any network communiation.
        /// </summary>
        /// <returns>A <see cref="IDetectableDevice" /> representing the detected device or null if the device is unknown and cannot be detected.</returns>
        public IDeviceHandler Detect()
        {
            Stopwatch detectionDuration = Stopwatch.StartNew();
            
            foreach (IDetectableDevice currentDevice in this.detectableDevices)
            {
                if (currentDevice.IsApplicable(this.lowerLayer))
                {
                    log.Info($"Device detection of '{this.lowerLayer.Address}' took {detectionDuration.ElapsedMilliseconds} ms");
                    
                    detectionDuration.Stop();
                    return currentDevice.CreateHandler(this.lowerLayer);
                }
            }

            detectionDuration.Stop();
            return null;
        }
    }
}
