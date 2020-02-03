#define DEBUG

using System;
using System.Collections.Generic;
using System.Net;

namespace RestService.DataFetchingService
{
    /// <summary>
    /// Enumeration of failure sources (entry point for data store).
    /// </summary>
    public enum QueryType
    {
        /// <summary>
        /// Feasibility for RSSI query
        /// </summary>
        RssiQuery,

        /// <summary>
        /// Feasibility for BGP query
        /// </summary>
        BgpQuery
    }

    /// <summary>
    /// Extension of an <see cref="IAquiredDataHandler" /> that records failures and allows filtering
    /// based on the failures and the time and how often they have last been seen.
    /// </summary>
    public interface IFailureRetryFilteringDataHandler : IAquiredDataHandler
    {
        /// <summary>
        /// Checks whether a retry shall be made for the given combination of query type, address and network.
        /// </summary>
        /// <param name="source">The source that shall be retried.</param>
        /// <param name="network">The affected network that is being retried.</param>
        /// <param name="address">The affected host address that is being retried.</param>
        /// <returns>
        /// <c>true</c> if a retry is feasible according to the store's settings.
        /// <c>null</c> if no information about the source/network combination is available. It's up to the caller do consider this as retry or no retry.
        /// <c>false</c> if a retry is not yet due according to the store's settings.
        /// </returns>
        bool? IsRetryFeasible(QueryType source, IPAddress address, IPNetwork network);

        /// <summary>
        /// Checks whether a retry shall be made for the given combination of query type, address and network.
        /// </summary>
        /// <param name="source">The source that shall be retried.</param>
        /// <param name="network">The affected network that is being retried.</param>
        /// <param name="addresses">The affected host addresses that are being retried.</param>
        /// <returns>
        /// <c>true</c> if a retry is feasible according to the store's settings.
        /// <c>null</c> if no information about the source/network combination is available. It's up to the caller do consider this as retry or no retry.
        /// <c>false</c> if a retry is not yet due according to the store's settings.
        /// </returns>
        bool? IsRetryFeasible(QueryType source, IEnumerable<IPAddress> addresses, IPNetwork network);

        /// <summary>
        /// Checks whether a retry shall be made for the given combination of query type and address.
        /// </summary>
        /// <param name="source">The source that shall be retried.</param>
        /// <param name="address">The affected host address that is being retried.</param>
        /// <returns>
        /// <c>true</c> if a retry is feasible according to the store's settings.
        /// <c>null</c> if no information about the source/network combination is available. It's up to the caller do consider this as retry or no retry.
        /// <c>false</c> if a retry is not yet due according to the store's settings.
        /// </returns>
        bool? IsRetryFeasible(QueryType source, IPAddress address);

        /// <summary>
        /// Checks whether a retry shall be made for the given combination of query type and network.
        /// </summary>
        /// <param name="source">The source that shall be retried.</param>
        /// <param name="network">The affected network that is being retried.</param>
        /// <returns>
        /// <c>true</c> if a retry is feasible according to the store's settings.
        /// <c>null</c> if no information about the source/network combination is available. It's up to the caller do consider this as retry or no retry.
        /// <c>false</c> if a retry is not yet due according to the store's settings.
        /// </returns>
        bool? IsRetryFeasible(QueryType source, IPNetwork network);

        /// <summary>
        /// Queries the details of the given combination of query type and address.
        /// </summary>
        /// <param name="source">The source that shall be retried.</param>
        /// <param name="address">The affected host address that is being retried.</param>
        /// <returns>
        /// <c>true</c> if a retry is feasible according to the store's settings.
        /// <c>null</c> if no information about the source/address combination is available.
        /// <c>false</c> if a retry is not yet due according to the store's settings.
        /// </returns>
        ISingleFailureInfo QueryPenaltyDetails(QueryType source, IPAddress address);


        /// <summary>
        /// Queries the details of the given combination of query type and network.
        /// </summary>
        /// <param name="source">The source that shall be retried.</param>
        /// <param name="network">The affected network that is being retried.</param>
        /// <returns>
        /// <c>true</c> if a retry is feasible according to the store's settings.
        /// <c>null</c> if no information about the source/network combination is available.
        /// <c>false</c> if a retry is not yet due according to the store's settings.
        /// </returns>
        ISingleFailureInfo QueryPenaltyDetails(QueryType source, IPNetwork network);
    }
    
    /// <summary>
    /// Class for the info about a single failure.
    /// </summary>
    public interface ISingleFailureInfo
    {
        /// <summary>
        /// Gets the number of occurances of this specific single failure.
        /// </summary>
        uint OccuranceCount { get; }

        /// <summary>
        /// Gets the time of the last occurance of this specific single failure.
        /// </summary>
        DateTime LastOccurance { get; }

        /// <summary>
        /// Gets the time of the first occurance of this specific single failure (i.e. the time when this failure set has been created).
        /// </summary>
        DateTime FirsOccurance { get; }

        /// <summary>
        /// Gets the current penalty time.
        /// </summary>
        TimeSpan CurrentPenalty { get; }

        /// <summary>
        /// Gets a value indicating whether a retry is feasible at the current moment.
        /// </summary>
        bool IsRetryFeasible { get; }
    }
}