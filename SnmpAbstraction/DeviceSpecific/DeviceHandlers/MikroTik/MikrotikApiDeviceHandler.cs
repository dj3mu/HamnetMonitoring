using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using SemVersion;
using SnmpSharpNet;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.Interface;
using tik4net.Objects.Interface.Wireless;
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

        private IInterfaceDetails networkInterfaceDetailsBacking = null;

        private IWirelessPeerInfos wirelessPeerInfosBacking = null;

        private readonly SystemIdentity sysIdent = null;

        private readonly SystemResource sysResource = null;

        private readonly SystemRouterboard sysRouterboard = null;

        /// <summary>
        /// Constructs for the given lower layer.
        /// </summary>
        /// <param name="address">The address of this device.</param>
        /// <param name="connectionType">The type of the connection in use (required in case we have to re-open).</param>
        /// <param name="tikConnection">The lower layer connection for talking to this device.</param>
        /// <param name="options">The options to use.</param>
        /// <param name="sysIdent">The system ident (to create SystemData from).</param>
        /// /// <param name="sysResource">The system resource info (to create SystemData from).</param>
        /// <param name="sysRouterboard">The system routerboard info (to create SystemData from).</param>
        public MikrotikApiDeviceHandler(IpAddress address, TikConnectionType connectionType, ITikConnection tikConnection, IQuerierOptions options, SystemIdentity sysIdent, SystemResource sysResource, SystemRouterboard sysRouterboard)
        {
            if ((options.AllowedApis & this.SupportedApi) == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options), $"This device handler doesn't support any of the APIs allowed by the IQuerierOptions (allowed: {options.AllowedApis}, supported {this.SupportedApi}).");
            }

            this.sysRouterboard = sysRouterboard ?? throw new ArgumentNullException(nameof(sysRouterboard), "sysRouterboard is null when creating a MikrotikApiDeviceHandler");
            this.sysResource = sysResource ?? throw new ArgumentNullException(nameof(sysResource), "sysResource is null when creating a MikrotikApiDeviceHandler");
            this.sysIdent = sysIdent ?? throw new ArgumentNullException(nameof(sysIdent), "sysIdent is null when creating a MikrotikApiDeviceHandler");

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
        public IInterfaceDetails NetworkInterfaceDetails
        {
            get
            {
                this.FetchNetworkInterfaceData();
                return this.networkInterfaceDetailsBacking;
            }
        }

        /// <inheritdoc />
        public IWirelessPeerInfos WirelessPeerInfos
        {
            get
            {
                this.FetchWirelessPeerInfo();
                return this.wirelessPeerInfosBacking;
            }
        }

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
        /// Fetches wireless peer data.
        /// </summary>
        private void FetchWirelessPeerInfo()
        {
            if (this.wirelessPeerInfosBacking != null)
            {
                return;
            }

            Stopwatch stopper = Stopwatch.StartNew();

            this.EnsureOpenConnection();

            var wirelessPeers = this.TikConnection.LoadList<WirelessRegistrationTable>();

            stopper.Stop();

            this.wirelessPeerInfosBacking = new SerializableWirelessPeerInfos
            {
                DeviceAddress = this.Address,
                DeviceModel = this.SystemData.DeviceModel,
                Details = wirelessPeers.Select(d => this.MakeWirelessPeerInfo(d)).ToList(),
                QueryDuration = stopper.Elapsed
            };
        }

        private IWirelessPeerInfo MakeWirelessPeerInfo(WirelessRegistrationTable registrationTableEntry)
        {
            var returnDetail = new SerializableWirelessPeerInfo
            {
                DeviceAddress = this.Address,
                DeviceModel = this.SystemData.DeviceModel,
                InterfaceId = this.NetworkInterfaceDetails.Details.Single(i => i.InterfaceName == registrationTableEntry.Interface).InterfaceId,
                RemoteMacString = registrationTableEntry.MacAddress,
                IsAccessPoint = !registrationTableEntry.Ap,
                LinkUptime = registrationTableEntry.Uptime,
                Oids = new Dictionary<CachableValueMeanings, ICachableOid>(),
                RxSignalStrength = new string[] { registrationTableEntry.SignalStrengthCh0, registrationTableEntry.SignalStrengthCh1, registrationTableEntry.SignalStrengthCh2 }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => Convert.ToDouble(s))
                    .DecibelLogSum(),
                TxSignalStrength = new string[] { registrationTableEntry.TxSignalStrengthCh0, registrationTableEntry.TxSignalStrengthCh1, registrationTableEntry.TxSignalStrengthCh2 }
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => Convert.ToDouble(s))
                    .DecibelLogSum()
            };

            return returnDetail;
        }

        /// <summary>
        /// Fetches network interface data.
        /// </summary>
        private void FetchNetworkInterfaceData()
        {
            if (this.networkInterfaceDetailsBacking != null)
            {
                return;
            }

            Stopwatch stopper = Stopwatch.StartNew();

            this.EnsureOpenConnection();

            var interfaceData = this.TikConnection.LoadList<Interface>();

            stopper.Stop();

            this.networkInterfaceDetailsBacking = new SerializableInterfaceDetails
            {
                DeviceAddress = this.Address,
                DeviceModel = this.SystemData.DeviceModel,
                Details = interfaceData.Select(d => this.MakeInterfaceDetail(d)).ToList(),
                QueryDuration = stopper.Elapsed
            };
        }

        private IInterfaceDetail MakeInterfaceDetail(Interface interfaceDetail)
        {
            var returnDetail = new SerializableInterfaceDetail
            {
                DeviceAddress = this.Address,
                DeviceModel = this.SystemData.DeviceModel,
                InterfaceId = Convert.ToInt32(interfaceDetail.Id.Trim('*')),
                InterfaceName = interfaceDetail.Name,
                MacAddressString = interfaceDetail.MacAddress,
                InterfaceType = interfaceDetail.Type.ToIanaInterfaceType()
            };

            return returnDetail;
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

            var versionMatch = OsVersionExtractionRegex.Match(sysResource.Version);
            if (versionMatch.Success)
            {
                this.osVersionBacking = versionMatch.Value.ToSemanticVersion();
            }
            else
            {
                throw new InvalidOperationException($"Cannot convert version string '{sysResource.Version}' to a valid SemanticVersion: It's not matching the version Regex '{OsVersionExtractionRegex.ToString()}'");
            }

            this.modelBacking = this.sysRouterboard.Model.Replace("RouterBOARD", "RB").Replace(" ", string.Empty);

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
                            detectedFeatures |= DeviceSupportedFeatures.BgpPeers | DeviceSupportedFeatures.Rssi;
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
                EnterpriseObjectId = new Oid(),
                Location = string.Empty,
                MaximumSnmpVersion = SnmpVersion.Ver2, // we know that MTik supports SNMPv2c
                Model = this.modelBacking,
                Name = this.sysIdent.Name,
                Uptime = this.sysResource.Uptime,
                Version = this.osVersionBacking,
                QueryDuration = TimeSpan.Zero,
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
