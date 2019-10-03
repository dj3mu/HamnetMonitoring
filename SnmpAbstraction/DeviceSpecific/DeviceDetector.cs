using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using SnmpSharpNet;

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
            
            var snmpVersionBackup = this.lowerLayer.ProtocolVersionInUse;

            // for device detection fall back to the lowest possible version
            this.lowerLayer.AdjustSnmpVersion(SnmpVersion.Ver1);

            List<Exception> collectedExceptions = new List<Exception>();
            List<string> collectedErrors = new List<string>();

            var type = typeof(IDetectableDevice);
            var detectableDevices = Assembly.GetExecutingAssembly().GetTypes()
                .Where(p => type.IsAssignableFrom(p) && !p.IsAbstract && !p.IsInterface);

            foreach (Type currentType in detectableDevices)
            {
                IDetectableDevice currentDevice = (IDetectableDevice)Activator.CreateInstance(currentType);

                try
                {
                    if (currentDevice.IsApplicable(this.lowerLayer))
                    {
                        detectionDuration.Stop();

                        log.Info($"Device detection of '{this.lowerLayer.Address}' took {detectionDuration.ElapsedMilliseconds} ms");
                    }
                    else
                    {
                        collectedErrors.Add($"{currentDevice}: Returned 'not applicable'");
                        continue;
                    }
                }
                catch(SnmpException ex)
                {
                    collectedExceptions.Add(ex);

                    var errorInfo2 = $"SnmpException talking to device '{this.lowerLayer.Address}' during applicability check: {ex.Message}";
                    collectedErrors.Add($"{currentDevice}: {errorInfo2}");

                    if (ex.Message.Equals("Request has reached maximum retries.") || ex.Message.ToLowerInvariant().Contains("timeout"))
                    {
                        var snmpErrorInfo = $"Timeout talking to device '{this.lowerLayer.Address}' during applicability check{Environment.NewLine}Collected Errors:{Environment.NewLine}{string.Join("\n", collectedErrors)}{Environment.NewLine}Collected Exceptions:{Environment.NewLine}{string.Join("\n", collectedExceptions.Select(e => e.Message))}";
                        log.Error(snmpErrorInfo, ex);

                        // Re-throwing a different exception is not good practice.
                        // But here we have a good reason: We need to add the IP address which timed out as that information is not contained in the SnmpException itself.
                        throw new HamnetSnmpException(snmpErrorInfo, ex);
                    }

                    log.Info($"Trying next device: {errorInfo2}");

                    continue;
                }
                catch(HamnetSnmpException ex)
                {
                    var errorInfo2 = $"HamnetSnmpException talking to device '{this.lowerLayer.Address}' during applicability check: {ex.Message}";
                    collectedErrors.Add($"{currentDevice}: {errorInfo2}");

                    log.Info($"Trying next device: Exception talking to device '{this.lowerLayer.Address}' during applicability check", ex);

                    continue;
                }
                catch(Exception ex)
                {
                    var errorInfo2 = $"Exception talking to device '{this.lowerLayer.Address}' during applicability check: {ex.Message}";
                    collectedErrors.Add($"{currentDevice}: {errorInfo2}");

                    log.Info($"Trying next device: Exception talking to device '{this.lowerLayer.Address}' during applicability check", ex);

                    continue;
                }

                try
                {
                    var handler = currentDevice.CreateHandler(this.lowerLayer);
                    var handlerBase = handler as DeviceHandlerBase;

                    var internalLowerLayer = this.lowerLayer as SnmpLowerLayer;
                    var internalSystemData = internalLowerLayer.InternalSystemData;
                    internalSystemData.ModifyableModel = handler.Model;
                    internalSystemData.ModifyableVersion = handler.OsVersion;
                    internalSystemData.ModifyableMaximumSnmpVersion = handlerBase.OidLookup.MaximumSupportedSnmpVersion;

                    if (handlerBase.OidLookup.MaximumSupportedSnmpVersion < snmpVersionBackup)
                    {
                        log.Info($"Device '{this.lowerLayer.Address}': Adjusting SNMP protocol version from {snmpVersionBackup} to {handlerBase.OidLookup.MaximumSupportedSnmpVersion} due to maximum version in device database");
                        internalLowerLayer.AdjustSnmpVersion(handlerBase.OidLookup.MaximumSupportedSnmpVersion);
                    }
                    else
                    {
                        this.lowerLayer.AdjustSnmpVersion(snmpVersionBackup);
                    }
                    
                    return handler;
                }
                catch(SnmpException ex)
                {
                    if (ex.Message.Equals("Request has reached maximum retries.") || ex.Message.ToLowerInvariant().Contains("timeout"))
                    {
                        var snmpErrorInfo = $"Timeout talking to device '{this.lowerLayer.Address}' ({this.lowerLayer?.SystemData?.DeviceModel}) during handler creation{Environment.NewLine}{string.Join("\n", collectedErrors)}{Environment.NewLine}Collected Exceptions:{Environment.NewLine}{string.Join("\n", collectedExceptions.Select(e => e.Message))}";
                        log.Error(snmpErrorInfo, ex);

                        // Re-throwing a different exception is not good practice.
                        // But here we have a good reason: We need to add the IP address which timed out as that information is not contained in the SnmpException itself.
                        throw new HamnetSnmpException(snmpErrorInfo, ex);
                    }

                    log.Error($"Trying next device: Exception talking to device '{this.lowerLayer.Address}' during handler creation: {ex.Message}");
                }
                catch(HamnetSnmpException ex)
                {
                    log.Error($"Trying next device: Exception talking to device '{this.lowerLayer.Address}' during handler creation", ex);
                }
            }

            detectionDuration.Stop();

            var errorInfo = $"Device '{this.lowerLayer.Address}' cannot be identified as a supported/known device after {detectionDuration.ElapsedMilliseconds} ms and trying {detectableDevices.Count()} devices.{Environment.NewLine}{string.Join("\n", collectedErrors)}{Environment.NewLine}Collected Exceptions:{Environment.NewLine}{string.Join("\n", collectedExceptions.Select(e => e.Message))}";
            log.Error(errorInfo);
            throw new HamnetSnmpException(errorInfo);
        }
    }
}
