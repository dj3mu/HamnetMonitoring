using System;
using System.Collections.Generic;
using System.Linq;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Imlementation of an <see cref="IHamnetQuerier" /> which first tries to get the data from
    /// a cache database and only if not found, delegates to an underlying querier.
    /// </summary>
    internal class CachingHamnetQuerier : IHamnetQuerier
    {
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
            throw new System.NotImplementedException();
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
                        if (this.cacheEntry != null)
                        {
                            this.cacheDatabaseContext.CacheData.Update(this.cacheEntry);
                            this.cacheDatabaseContext.SaveChanges();
                        }

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

            this.cacheDatabaseContext.CacheData.Update(this.cacheEntry);
            this.cacheDatabaseContext.SaveChanges();
        }

        /// <summary>
        /// Fetches the interface details from lower querier and saves it to the database.
        /// </summary>
        private void LowerQuerierFetchInterfaceDetails()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fetches the wireless peer info from lower querier and saves it to the database.
        /// </summary>
        private void LowerQuerierFetchWirelessPeerInfo()
        {
            throw new NotImplementedException();
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
                    false));
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
                this.cacheEntry = new CacheData { Address = this.Address, CachableOids = this.cachableOidLookup.Values };
                this.cacheDatabaseContext.CacheData.Add(this.cacheEntry);
                this.cacheDatabaseContext.SaveChanges();
            }
            else
            {
                // only need to do this in else branch as in if we've just initialized the database from the field
                this.GetOidLookupFromDatabase();
            }
        }

        /// <summary>
        /// New sets or updates the cachableOidLookup.
        /// </summary>
        private void GetOidLookupFromDatabase()
        {
            lock(this.SyncRoot)
            {
                this.cachableOidLookup.Clear();
                foreach (ICachableOid coid in this.cacheEntry.CachableOids)
                {
                    this.cachableOidLookup.Add(coid.Meaning, coid);
                }
            }
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
