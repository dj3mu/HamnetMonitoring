using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Imlementation of an <see cref="IHamnetQuerier" /> which first tries to get the data from
    /// a cache database and only if not found, delegates to an underlying querier.
    /// </summary>
    internal class CachingHamnetQuerier : IHamnetQuerier
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The SNMP lower layer used to query volatile data.
        /// </summary>
        private readonly ISnmpLowerLayer lowerLayer = null;

        /// <summary>
        /// The options of the querier.
        /// </summary>
        private readonly IQuerierOptions options;

        /// <summary>
        /// The underlying querier. Only instantiated on demand.
        /// </summary>
        private IHamnetQuerier lowerQuerier = null;

        /// <summary>
        /// To detect redundant calls to Dispose.
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// The database context to access the cache database.
        /// </summary>
        private CacheDatabaseContext cacheDatabaseContext = null;

        /// <summary>
        /// The cache entry for our device.
        /// </summary>
        private CacheData cacheEntry;

        /// <summary>
        /// The lookup for cachable values.
        /// </summary>
        private Dictionary<CachableValueMeanings, ICachableOid> cachableOidLookup = new Dictionary<CachableValueMeanings, ICachableOid>();

        /// <summary>
        /// The volatile data fetching wireless peer info object.
        /// </summary>
        private VolatileFetchingWirelessPeerInfos volatileFetchingWirelessPeerInfo = null;

        /// <summary>
        /// The volatile data fetching interface details object.
        /// </summary>
        private VolatileFetchingInterfaceDetails volatileFetchingInterfaceDetails = null;

        /// <summary>
        /// Initializes using the given lower layer.
        /// </summary>
        /// <param name="lowerLayer">The lower layer to use.</param>
        /// <param name="options">The options of the querier.</param>
        public CachingHamnetQuerier(ISnmpLowerLayer lowerLayer, IQuerierOptions options)
        {
            this.lowerLayer = lowerLayer ?? throw new ArgumentNullException(nameof(lowerLayer), "The lower layer SNMP engine is null");
            this.options = options ?? throw new ArgumentNullException(nameof(options), "The querier options are null");
        }

        // TODO: Only implement if unmanaged resources are to be freed.
        // ~CachingHamnetQuerier()
        // {
        //   this. Dispose(false);
        // }

        /// <summary>
        /// Thread sync / locking object.
        /// </summary>
        internal CachingHamnetQuerier(object syncRoot)
        {
            this.SyncRoot = syncRoot;

        }
        public object SyncRoot { get; } = new object();

        /// <inheritdoc />
        public IpAddress Address => this.lowerLayer.Address;

        /// <inheritdoc />
        public IDeviceSystemData SystemData
        {
            get
            {
                this.InitializeCacheEntry();

                this.LowerQuerierFetchSystemData();

                return this.cacheEntry.SystemData;
            }
        }

        /// <inheritdoc />
        public IInterfaceDetails NetworkInterfaceDetails
        {
            get
            {
                this.InitializeCacheEntry();

                this.LowerQuerierFetchInterfaceDetails();

                return this.cacheEntry.InterfaceDetails;
            }
        }

        /// <inheritdoc />
        public IWirelessPeerInfos WirelessPeerInfos
        {
            get
            {
                this.InitializeCacheEntry();

                this.LowerQuerierFetchWirelessPeerInfo();

                return this.volatileFetchingWirelessPeerInfo;
            }
        }

        /// <inheritdoc />
        public ILinkDetails FetchLinkDetails(params string[] remoteHostNamesOrIps)
        {
            if (remoteHostNamesOrIps.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(remoteHostNamesOrIps), "No remote IP address specified at all: You need to specify at least two host names or addresses for fetching link details");
            }

            List<IHamnetQuerier> remoteQueriers = remoteHostNamesOrIps.Select(remoteHostNamesOrIp =>
            {
                IPAddress outAddress;
                if (!remoteHostNamesOrIp.TryGetResolvedConnecionIPAddress(out outAddress))
                {
                    log.Error($"Cannot resolve host name or IP string '{remoteHostNamesOrIp}' to a valid IPAddres. Skipping that remote for link detail fetching");
                }

                return SnmpQuerierFactory.Instance.Create(outAddress, this.options);
            }).ToList();

            if (remoteQueriers.Count == 0)
            {
                throw new InvalidOperationException($"No remote IP address available at all after resolving {remoteHostNamesOrIps.Length} host name or address string to IP addresses");
            }

            List<ILinkDetail> fetchedDetails = new List<ILinkDetail>(remoteQueriers.Count);
            foreach (var remoteQuerier in remoteQueriers)
            {
                var linkDetectionAlgorithm = new LinkDetectionAlgorithm(this, remoteQuerier);
                fetchedDetails.AddRange(linkDetectionAlgorithm.DoQuery().Details);
            }

            return new LinkDetails(fetchedDetails, this.Address, this.SystemData.DeviceModel);
        }

        /// <inheritdoc />
        public IBgpPeers FetchBgpPeers(string remotePeerIp)
        {
            this.InitializeLowerQuerier();

            // currently not caching BGP info at all -- it's pretty volatile
            // not sure whether we'll implement caching at all one day.
            return this.lowerQuerier.FetchBgpPeers(remotePeerIp);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);

            // TODO: Uncomment only if finalizer is implemented above.
            // GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes off this instance.
        /// </summary>
        /// <param name="disposing"><c>true</c> if called from Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.lowerQuerier != null)
                    {
                        this.lowerQuerier.Dispose();
                        this.lowerQuerier = null;
                    }

                    if (this.cacheDatabaseContext != null)
                    {
                        //if (this.cacheEntry != null)
                        //{
                        //    this.cacheDatabaseContext.CacheData.Update(this.cacheEntry);
                        //    this.cacheDatabaseContext.SaveChanges();
                        //}

                        this.cacheDatabaseContext.Dispose();
                        this.cacheDatabaseContext = null;
                    }
                }

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Fetches the system data from lower querier and saves it to the database.
        /// </summary>
        private void LowerQuerierFetchSystemData()
        {
            if (this.cacheEntry.SystemData != null)
            {
                return;
            }

            this.InitializeLowerQuerier();

            this.cacheEntry.SystemData = new SerializableSystemData(this.lowerQuerier.SystemData);
            this.cacheEntry.LastModification = DateTime.UtcNow;

            this.cacheDatabaseContext.CacheData.Update(this.cacheEntry);
            this.cacheDatabaseContext.SaveChanges();
        }

        /// <summary>
        /// Fetches the interface details from lower querier and saves it to the database.
        /// </summary>
        private void LowerQuerierFetchInterfaceDetails()
        {
            if (this.volatileFetchingInterfaceDetails != null)
            {
                return;
            }

            if (this.cacheEntry.InterfaceDetails == null)
            {
                this.InitializeLowerQuerier();

                var lowerLayerInterfaceDetails = this.lowerQuerier.NetworkInterfaceDetails;

                // we force immediate evaluation in order to ensure that really all cachable OIDs have been set.
                lowerLayerInterfaceDetails.ForceEvaluateAll();

                this.cacheEntry.InterfaceDetails = new SerializableInterfaceDetails(lowerLayerInterfaceDetails);
                this.cacheEntry.LastModification = DateTime.UtcNow;

                this.cacheDatabaseContext.CacheData.Update(this.cacheEntry);
                this.cacheDatabaseContext.SaveChanges();
            }

            this.volatileFetchingInterfaceDetails = new VolatileFetchingInterfaceDetails(this.cacheEntry.InterfaceDetails, this.lowerLayer);
        }

        /// <summary>
        /// Fetches the wireless peer info from lower querier and saves it to the database.
        /// </summary>
        private void LowerQuerierFetchWirelessPeerInfo()
        {
            if (this.volatileFetchingWirelessPeerInfo != null)
            {
                return;
            }

            if (this.cacheEntry.WirelessPeerInfos == null)
            {
                this.InitializeLowerQuerier();

                var lowerLayerWirelessPeerInfos = this.lowerQuerier.WirelessPeerInfos;

                // we force immediate evaluation in order to ensure that really all cachable OIDs have been set.
                lowerLayerWirelessPeerInfos.ForceEvaluateAll();

                this.cacheEntry.WirelessPeerInfos = new SerializableWirelessPeerInfos(lowerLayerWirelessPeerInfos);
                this.cacheEntry.LastModification = DateTime.UtcNow;

                this.cacheDatabaseContext.CacheData.Update(this.cacheEntry);
                this.cacheDatabaseContext.SaveChanges();
            }

            this.volatileFetchingWirelessPeerInfo = new VolatileFetchingWirelessPeerInfos(this.cacheEntry.WirelessPeerInfos, this.lowerLayer);
        }

        /// <summary>
        /// Initializes the lower querier.
        /// </summary>
        private void InitializeLowerQuerier()
        {
            if (this.lowerQuerier != null)
            {
                return;
            }

            this.lowerQuerier = SnmpQuerierFactory.Instance.Create(
                this.lowerLayer,
                new QuerierOptions(
                    this.options.Port,
                    this.options.ProtocolVersion,
                    this.options.Community,
                    this.options.Timeout,
                    this.options.Retries,
                    this.options.Ver2cMaximumValuesPerRequest,
                    this.options.Ver2cMaximumRequests,
                    false,
                    this.options.LoginUser,
                    this.options.LoginPassword));
        }

        /// <summary>
        /// Initializes the cache.
        /// </summary>
        private void InitializeCacheEntry()
        {
            this.InitializeDatabaseContext();

            if (this.cacheEntry != null)
            {
                return;
            }

            this.cacheEntry = this.cacheDatabaseContext.CacheData.FirstOrDefault(e => e.Address == this.Address);

            if (this.cacheEntry == null)
            {
                this.cacheEntry = new CacheData { Address = this.Address, LastModification = DateTime.UtcNow };
                this.cacheDatabaseContext.CacheData.Add(this.cacheEntry);
                this.cacheDatabaseContext.SaveChanges();
            }

            if (this.cacheEntry.SystemData == null)
            {
                this.LowerQuerierFetchSystemData();
            }

            var internalLowerLayer = this.lowerLayer as SnmpLowerLayer;
#if DEBUG
            log.Debug($"Device '{this.lowerLayer.Address}': Setting SNMP protocol version to {this.cacheEntry.SystemData.MaximumSnmpVersion} due cache database SystemData.MaximumSnmpVersion setting");
#endif
            internalLowerLayer.AdjustSnmpVersion(this.cacheEntry.SystemData.MaximumSnmpVersion);
        }

        /// <summary>
        /// Initializes a database context to access the cache database.
        /// </summary>
        private void InitializeDatabaseContext()
        {
            if (this.cacheDatabaseContext != null)
            {
                return;
            }

            this.cacheDatabaseContext = CacheDatabaseProvider.Instance.CacheDatabase;
        }
    }
}
