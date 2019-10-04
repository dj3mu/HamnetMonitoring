using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Class performaning maitenance tasks on the cache data.
    /// </summary>
    public class CacheMaintenance
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private bool dryRunMode;

        /// <summary>
        /// Default-Construct
        /// </summary>
        public CacheMaintenance(bool dryRunMode)
        {
            this.dryRunMode = dryRunMode;
        }

        /// <summary>
        /// Removes (i.e. prevents caching for the next query) the cache entry for the given addresses.
        /// </summary>
        /// <param name="addresses">The list of addresses to delete cache entries for.</param>
        public void DeleteForAddress(IEnumerable<IPAddress> addresses)
        {
            if (addresses == null)
            {
                throw new ArgumentNullException(nameof(addresses), "Address list to remove is null");
            }

            if (!addresses.Any())
            {
                // save some time if the adress list is empty
                return;
            }

            using (var dbContext = CacheDatabaseProvider.Instance.CacheDatabase)
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    List<CacheData> dataToDelete = new List<CacheData>();
                    foreach (var item in addresses)
                    {
                        var itemIp = new IpAddress(item);
                        var entryToDelete = dbContext.CacheData.Where(d => d.Address == itemIp);

                        if (entryToDelete.Any())
                        {
                            log.Info($"Listing cache entry for address '{item}' as explicitly requested REMOVAL");
                        }

                        dataToDelete.AddRange(entryToDelete);
                    }

                    log.Info($"{(this.dryRunMode ? "DRY RUN: Would now remove" : "Removing")} {dataToDelete.Count} cache entries as explicitly requested");

                    if (!this.dryRunMode)
                    {
                        dbContext.CacheData.RemoveRange(dataToDelete);
                    }

                    dbContext.SaveChanges();
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Fetches and returns the full cache data.<br/>
        /// EXPENSIVE. Use with care.
        /// </summary>
        /// <returns>The complete cache data list.</returns>
        public IEnumerable<ICacheData> FetchEntryList()
        {
            List<ICacheData> returnList = null;
            using (var dbContext = CacheDatabaseProvider.Instance.CacheDatabase)
            {
                returnList = dbContext.CacheData.Cast<ICacheData>().ToList();
            }

            return returnList;
        }

        /// <summary>
        /// Removes (i.e. prevents caching for the next query) the cache entries which have last been modified
        /// before the given amount of time.
        /// </summary>
        /// <param name="cacheValidSpan">The amount of time that an entry stays valid.</param>
        public void RemoveFromCacheIfModificationOlderThan(TimeSpan cacheValidSpan)
        {
            using (var dbContext = CacheDatabaseProvider.Instance.CacheDatabase)
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    var nowItIs = DateTime.UtcNow;

                    var entriesToDelete = dbContext.CacheData.Where(d => (nowItIs - d.LastModification) > cacheValidSpan);

                    foreach (var item in entriesToDelete)
                    {
                        log.Info($"{(this.dryRunMode ? "DRY RUN: Would now remove" : "Removing")} outdated cache entry for address {item.Address}: Last modified {item.LastModification} was more than {cacheValidSpan} ago");
                    }

                    log.Info($"{(this.dryRunMode ? "DRY RUN: Would now remove" : "Removing")} {entriesToDelete.Count()} cache entries which have last been modified more than {cacheValidSpan} ago");

                    if (!this.dryRunMode)
                    {
                        dbContext.RemoveRange(entriesToDelete);
                    }

                    dbContext.SaveChanges();
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Gets some statistics info about that database.
        /// </summary>
        /// <returns>Key-value-pairs with the statistics information.</returns>
        public IReadOnlyDictionary<string, string> CacheStatistics()
        {
            using (var dbContext = CacheDatabaseProvider.Instance.CacheDatabase)
            {
                return new Dictionary<string, string>
                {
                    { "UniqueCacheEntries", dbContext.CacheData.Count().ToString() }
                };
            }
        }
    }
}