using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Configuration;
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
        /// Sets the database configuration from the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration to use.</param>
        public static void SetDatabaseConfiguration(IConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration), "the configuration to set is null");
            }

            CacheDatabaseProvider.Instance.Configuration = configuration.GetSection(CacheDatabaseProvider.CacheDatabaseSectionName);
        }

        /// <summary>
        /// Removes (i.e. prevents caching for the next query) the cache entry for the given addresses.
        /// </summary>
        /// <param name="addresses">The list of addresses to delete cache entries for.</param>
        public void DeleteForAddress(IEnumerable<IPAddress> addresses)
        {
            log.Info($"{(this.dryRunMode ? "DRY RUN: " : string.Empty)}STARTING: Deleting explicitly requested cache entries");
            if (addresses == null)
            {
                throw new ArgumentNullException(nameof(addresses), "Address list to remove is null");
            }

            if (!addresses.Any())
            {
                // save some time if the adress list is empty
                log.Info($"{(this.dryRunMode ? "DRY RUN: " : string.Empty)}COMPLETED: Nothing to delete for explicitly requested cache entries");
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

                        var entryToDelete = dbContext.CacheData
                            .Where(d => d.Address == itemIp);

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

            log.Info($"{(this.dryRunMode ? "DRY RUN: " : string.Empty)}COMPLETED: Deleting explicitly requested cache entries");
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
            log.Info($"{(this.dryRunMode ? "DRY RUN: " : string.Empty)}STARTING: Deleting cache entries that last changed more than {cacheValidSpan} ago");

            using (var dbContext = CacheDatabaseProvider.Instance.CacheDatabase)
            {
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    var nowItIs = DateTime.UtcNow;

                    // [KL 2020-02-02]: The following is NOT deleting outdated entries since 2020-01-31.
                    //                  Looks like a bug in DateTime handling of entity framework.
                    //                  As this method is only seldomly executed, we can switch to C#-internal evaluation
                    //                  without significant performance issue and investigate later.
                    var entriesToDelete = dbContext.CacheData
                        .ToList() // this makes it C# linq instead of entity-converted mysql
                        .Where(d => (nowItIs - d.LastModification) > cacheValidSpan);

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

            log.Info($"{(this.dryRunMode ? "DRY RUN: " : string.Empty)}COMPLETED: Deleting cache entries that last changed more than {cacheValidSpan} ago");
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