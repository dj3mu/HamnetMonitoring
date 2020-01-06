using System;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Base class for all device handlers
    /// </summary>
    internal abstract class DeviceHandlerBase : IDeviceHandler
    {
        /// <summary>
        /// To detect multiple calls to <see cref="Dispose()" />.
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Prevent default construction
        /// </summary>
        private DeviceHandlerBase()
        {
        }

        /// <summary>
        /// Constructs for the given lower layer.
        /// </summary>
        /// <param name="lowerLayer">The lower layer for talking to this device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="osVersion">The SW version of the device.</param>
        /// <param name="model">The device's model name. Shall be the same name as used for the device name during OID database lookups.</param>
        /// <param name="options">The options to use.</param>
        public DeviceHandlerBase(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup, SemVersion.SemanticVersion osVersion, string model, IQuerierOptions options)
        {
            if (lowerLayer == null)
            {
                throw new ArgumentNullException(nameof(lowerLayer), "lower layer is null when constructing a device handler");
            }

            if (oidLookup == null)
            {
                throw new ArgumentNullException(nameof(oidLookup), "OID lookup table is null when constructing a device handler");
            }

            if (osVersion == null)
            {
                throw new ArgumentNullException(nameof(osVersion), "OS version info is null when constructing a device handler");
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new ArgumentNullException(nameof(model), "Model name is null, empty or white-space-only when constructing a device handler");
            }

            this.LowerLayer = lowerLayer;
            this.OidLookup = oidLookup;
            this.OsVersion = osVersion;
            this.Model = model;
            this.Options = options ?? throw new ArgumentNullException(nameof(options), "The options are null when constructing a device handler");
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~DeviceHandlerBase()
        // {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        /// <summary>
        /// Gets the lower layer to talk to this device.
        /// </summary>
        public ISnmpLowerLayer LowerLayer { get; }

        /// <summary>
        /// Gets the OID lookup table for this device.
        /// </summary>
        public IDeviceSpecificOidLookup OidLookup { get; }

        /// <inheritdoc />
        public IDeviceSystemData SystemData => this.LowerLayer.SystemData;

        /// <inheritdoc />
        public abstract IInterfaceDetails NetworkInterfaceDetails { get; }

        /// <inheritdoc />
        public abstract IWirelessPeerInfos WirelessPeerInfos { get; }

        /// <inheritdoc />
        public IpAddress Address => this.LowerLayer.Address;

        /// <inheritdoc />
        public SemanticVersion OsVersion { get; }

        /// <inheritdoc />
        public string Model { get; }

        /// <inheritdoc />
        public IQuerierOptions Options { get; }

        /// <inheritdoc />
        public abstract QueryApis SupportedApi { get; }

        /// <inheritdoc />
        public abstract IBgpPeers FetchBgpPeers(string remotePeerIp);

        /// <inheritdoc />
        public abstract ITracerouteResult Traceroute(IpAddress remoteIp, uint count, TimeSpan timeout, int maxHops);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Model} v {this.OsVersion} @ {this.Address}";
        }

        public void Dispose()
        {
            this.Dispose(true);

            // TODO: Imcomment following line only if finalizer is overloaded above.
            // GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.LowerLayer != null)
                    {
                        this.LowerLayer.Dispose();
                    }
                }

                this.disposedValue = true;
            }
        }
    }
}
