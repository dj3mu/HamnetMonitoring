using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using SemVersion;
using SnmpSharpNet;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.System;
using tik4net.Objects.User;

namespace SnmpAbstraction
{
    /// <summary>
    /// Device Handler class for Mikrotik devices using SNMP communication.
    /// </summary>
    internal class MikrotikApiDeviceHandler : IDeviceHandler
    {
        private static readonly Regex OsVersionExtractionRegex = new Regex(@"([0-9.]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// The minimum version for support of MTik API v2.
        /// </summary>
        private static readonly SemanticVersion ApiV2MinimumVersion = new SemanticVersion(6, 45, 0, string.Empty, string.Empty);

        /// <summary>
        /// To avoid redundant calls to Dispose.
        /// </summary>
        private bool disposedValue = false;

        private SemanticVersion osVersionBacking = null;

        private IDeviceSystemData systemDataBacking = null;
        
        private string modelBacking = null;

        /// <summary>
        /// Constructs for the given lower layer.
        /// </summary>
        /// <param name="address">The address of this device.</param>
        /// <param name="connectionType">The type of the connection in use (required in case we have to re-open).</param>
        /// <param name="tikConnection">The lower layer connection for talking to this device.</param>
        /// <param name="options">The options to use.</param>
        public MikrotikApiDeviceHandler(IpAddress address, TikConnectionType connectionType, ITikConnection tikConnection, IQuerierOptions options)
        {
            if ((options.AllowedApis & this.SupportedApi) == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options), $"This device handler doesn't support any of the APIs allowed by the IQuerierOptions (allowed: {options.AllowedApis}, supported {this.SupportedApi}).");
            }

            this.Address = address ?? throw new ArgumentNullException(nameof(address), "address is null when creating a MikrotikApiDeviceHandler");
            this.TikConnection = tikConnection ?? throw new ArgumentNullException(nameof(tikConnection), "tikConnection is null when creating a MikrotikApiDeviceHandler");
            this.DetectedConnectionType = connectionType;
            this.Options = options ?? throw new ArgumentNullException(nameof(options), "options are null when creating a MikrotikApiDeviceHandler");
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~DeviceHandlerBase()
        // {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        /// <inheritdoc />
        public QueryApis SupportedApi { get; } = QueryApis.VendorSpecific;

        /// <inheritdoc />
        public IpAddress Address { get; }

        /// <inheritdoc />
        public IQuerierOptions Options { get; }

        /// <inheritdoc />
        public SemanticVersion OsVersion
        {
            get
            {
                this.FetchSystemData(); // also initializes the osVersionBacking as its content comes with the same API calls
                return this.osVersionBacking;
            }
        }

        /// <inheritdoc />
        public string Model
        {
            get
            {
                this.FetchSystemData(); // also initializes the modelBacking as its content comes with the same API calls
                return this.modelBacking;
            }
        }

        /// <inheritdoc />
        public IDeviceSystemData SystemData
        {
            get
            {
                this.FetchSystemData();
                return this.systemDataBacking;
            }
        }

        /// <inheritdoc />
        public IInterfaceDetails NetworkInterfaceDetails => throw new NotSupportedException("Getting network interface details is not (yet) supported for a Mikrotik API-based device handler. Please use the SNMP-based device handler");

        /// <inheritdoc />
        public IWirelessPeerInfos WirelessPeerInfos => throw new NotSupportedException("Getting wireless peers is not (yet) supported for a Mikrotik API-based device handler. Please use the SNMP-based device handler");
        
        /// <summary>
        /// Gets the connection type that has been used when checking whether this device support MTik API.
        /// </summary>
        protected TikConnectionType DetectedConnectionType { get; }

        public void Dispose()
        {
            this.Dispose(true);

            // TODO: Imcomment following line only if finalizer is overloaded above.
            // GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public IBgpPeers FetchBgpPeers(string remotePeerIp)
        {
            return new LazyLoadingMikrotikBgpPeerInfos(this.Address, this.TikConnection, remotePeerIp);
        }

        /// <inheritdoc />
        public ITracerouteResult Traceroute(IpAddress remoteIp, uint count)
        {
            return new MikrotikApiTracerouteOperation(this.Address, this.TikConnection, remoteIp, count).Execute(); 
        }

        /// <summary>
        /// Gets the tik4Net connection handle to use for talking to this device.
        /// </summary>
        protected ITikConnection TikConnection { get; }

        protected void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.TikConnection != null)
                    {
                        this.TikConnection.Dispose();
                    }
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Fetches the system data info.
        /// </summary>
        private void FetchSystemData()
        {
            if (this.systemDataBacking != null)
            {
                return;
            }

            Stopwatch stopper = Stopwatch.StartNew();

            this.EnsureOpenConnection();

            var sysResource = this.TikConnection.LoadSingle<SystemResource>();
            var sysRouterboard = this.TikConnection.LoadSingle<SystemRouterboard>();
            var sysIdent = this.TikConnection.LoadSingle<SystemIdentity>();

            stopper.Stop();

            var versionMatch = OsVersionExtractionRegex.Match(sysResource.Version);
            if (versionMatch.Success)
            {
                this.osVersionBacking = versionMatch.Value.ToSemanticVersion();
            }
            else
            {
                throw new InvalidOperationException($"Cannot convert version string '{sysResource.Version}' to a valid SemanticVersion: It's not matching the version Regex '{OsVersionExtractionRegex.ToString()}'");
            }

            this.modelBacking = sysRouterboard.Model.Replace("RouterBOARD", "RB").Replace(" ", string.Empty);

            var users = this.TikConnection.LoadList<User>();
            var groups = this.TikConnection.LoadList<UserGroup>();
            
            var detectedFeatures = DeviceSupportedFeatures.None;
            var myUser = users.SingleOrDefault(u => u.Name == this.Options.LoginUser);
            if (myUser != null)
            {
                var myGroup = groups.SingleOrDefault(g => g.Name == myUser.Group);
                if (myGroup != null)
                {
                    string[] policies = myGroup.Policy.Split(',');
                    if (policies.Contains("api"))
                    {
                        // only with API allowed we need to check further (well - since we connected to it, this should always be true)
                        if (policies.Contains("read"))
                        {
                            // yes, other features would also be supported with "read" policiy
                            // but since we've not implemented retrieval of RSSI & Co, we also don't report it for now
                            // TODO: Add more features once implemented
                            detectedFeatures |= DeviceSupportedFeatures.BgpPeers;
                        }

                        if (policies.Contains("test"))
                        {
                            detectedFeatures |= DeviceSupportedFeatures.Traceroute;
                        }
                    }
                }
            }

            this.systemDataBacking = new SerializableSystemData
            {
                Contact = string.Empty,
                Description = $"RouterOS {this.modelBacking}",
                DeviceAddress = this.Address,
                DeviceModel = $"{this.modelBacking} v {this.osVersionBacking}",
                EnterpriseObjectId = null,
                Location = string.Empty,
                MaximumSnmpVersion = SnmpVersion.Ver2, // we know that MTik supports SNMPv2c
                Model = this.modelBacking,
                Name = sysIdent.Name,
                Uptime = sysResource.Uptime,
                Version = this.osVersionBacking,
                QueryDuration = stopper.Elapsed,
                SupportedFeatures = detectedFeatures
            };
        }

        /// <summary>
        /// Ensures that the MTik API connection is open.
        /// </summary>
        private void EnsureOpenConnection()
        {
            if (!this.TikConnection.IsOpened)
            {
                this.TikConnection.Open(this.Address.ToString(), this.Options.LoginUser ?? string.Empty, this.Options.LoginPassword ?? string.Empty);
            }
        }
    }
}
