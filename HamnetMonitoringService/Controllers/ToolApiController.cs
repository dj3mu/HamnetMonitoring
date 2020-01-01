﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestService.Database;
using SnmpAbstraction;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Controller class for the &quot;vw_rest_rssi&quot; REST API
    /// </summary>
    [Route("api/v1/tools")]
    [ApiController]
    public class ToolController : ControllerBase
    {
        /// <summary>
        /// The maximum number of packets that we allow for traceroute.
        /// </summary>
        private const int MaxTracerouteSendCount = 100;

        /// <summary>
        /// The minimum timeout for a single hop during traceroute.
        /// </summary>
        private const double MinTracerouteTimeoutSeconds = 0.1;

        /// <summary>
        /// The maximum timeout for a single hop during traceroute.
        /// </summary>
        private const double MaxTracerouteTimeoutSeconds = 60.0;

        /// <summary>
        /// The minimum maximum hops during traceroute.
        /// </summary>
        private const int MinTracerouteMaxHops = 10;

        /// <summary>
        /// The maximum maximum hops during traceroute.
        /// </summary>
        private const int MaxTracerouteMaxHops = 255;

        private static readonly char[] Separators = new[] { ' ', '\t', ',' };

        private readonly ILogger logger;

        private readonly IConfiguration configuration;

        private readonly QueryResultDatabaseContext dbContext;
        
        /// <summary>
        /// Instantiates the controller taking a logger.
        /// </summary>
        /// <param name="logger">The logger to use for logging.</param>
        /// <param name="configuration">The configuration settings.</param>
        /// <param name="dbContext">The databse context to use for retrieving the values to return.</param>
        public ToolController(ILogger<RestController> logger, IConfiguration configuration, QueryResultDatabaseContext dbContext)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger), "The logger to use is null");
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration), "The configuration to use is null");
            this.dbContext = dbContext ?? throw new System.ArgumentNullException(nameof(dbContext), "The database context to take the data from is null");
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("hostsSupportingFeature/{featureList}")]
        public async Task<ActionResult<IStatusReply>> HostsSupportingFeature(string featureList)
        {
            Program.RequestStatistics.ApiV1HostsSupportingFeature++;

            if (string.IsNullOrWhiteSpace(featureList))
            {
                return new ErrorReply(new ArgumentException("Found null, empty or white-space only feature list."));
            }

            DeviceSupportedFeatures requestedFeatures = DeviceSupportedFeatures.None;
            try
            {
                requestedFeatures = featureList.Split(Separators, StringSplitOptions.RemoveEmptyEntries).ToDeviceSupportedFeatures();
            }
            catch(ArgumentOutOfRangeException aoorex)
            {
                return new ErrorReply(aoorex);
            }

            if (requestedFeatures == DeviceSupportedFeatures.None)
            {
                return new ErrorReply(new ArgumentException("Invalid feature list."));
            }

            return await Task.Run(() => this.FetchCacheEntries(requestedFeatures));
        }

        /// <summary>
        /// Fetches the cache data and converts to an array.
        /// </summary>
        /// <returns>The result list.</returns>
        private ActionResult<IStatusReply> FetchCacheEntries(DeviceSupportedFeatures features)
        {
            var cacheMaintenance = new CacheMaintenance(true);
            return new HostsSupportingFeatureResult(cacheMaintenance.FetchEntryList().Where(e => ((e.SystemData?.SupportedFeatures ?? DeviceSupportedFeatures.None) & features) == features).Select(cacheEntry => new HostInfoReply(cacheEntry.Address, cacheEntry.SystemData, cacheEntry.ApiUsed, cacheEntry.LastModification)));
        }

        /// <summary>
        /// Implementation of GET request.
        /// </summary>
        /// <returns>The results of the get request.</returns>
        [HttpGet("traceroute/{fromHost}/{toHost}/{count?}/{timeoutSeconds?}/{maxHops?}")]
        public async Task<ActionResult<IStatusReply>> TracerouteHost(string fromHost, string toHost, [FromQuery]FromUrlQueryQuerierOptions options, int count = 1, double timeoutSeconds = 1.0, int maxHops = MaxTracerouteMaxHops / 4)
        {
            Program.RequestStatistics.ApiV1TraceRouteRequests++;

            IQuerierOptions optionsInUse = this.CreateOptions(options);

            if ((count < 1) || (count > MaxTracerouteSendCount))
            {
                return new ErrorReply(new ArgumentOutOfRangeException(nameof(count), $"count must be in range [1; {MaxTracerouteSendCount}] but was found as {count}"));
            }

            if ((timeoutSeconds < MinTracerouteTimeoutSeconds) || (timeoutSeconds > MaxTracerouteTimeoutSeconds))
            {
                return new ErrorReply(new ArgumentOutOfRangeException(nameof(timeoutSeconds), $"timeoutSeconds must be in range [{MinTracerouteTimeoutSeconds}; {MaxTracerouteTimeoutSeconds}] but was found as {timeoutSeconds}"));
            }

            if ((maxHops < MinTracerouteMaxHops) || (maxHops > MaxTracerouteMaxHops))
            {
                return new ErrorReply(new ArgumentOutOfRangeException(nameof(maxHops), $"maxHops must be in range [{MinTracerouteMaxHops}; {MaxTracerouteMaxHops}] but was found as {maxHops}"));
            }

            return await new TracerouteAction(WebUtility.UrlDecode(fromHost), WebUtility.UrlDecode(toHost), count, TimeSpan.FromSeconds(timeoutSeconds), maxHops, optionsInUse as FromUrlQueryQuerierOptions).Execute();
        }

        /// <summary>
        /// Creates the querier options prioritizing query URL over the confiugration settings.
        /// </summary>
        /// <param name="options">The options from query URL.</param>
        /// <returns>The assembled options to use.</returns>
        private IQuerierOptions CreateOptions(FromUrlQueryQuerierOptions options)
        {
            FromUrlQueryQuerierOptions optionsInUse = options;
            
            var monitoringAccountsSection = this.configuration.GetSection(Program.MonitoringAccountsSectionKey).GetSection(Program.BgpAccountSectionKey);

            var loginUserName = monitoringAccountsSection.GetValue<string>("User");
            if (string.IsNullOrWhiteSpace(optionsInUse.LoginUser) && !string.IsNullOrWhiteSpace(loginUserName))
            {
                optionsInUse.LoginUser = loginUserName;
            }

            var loginPassword = monitoringAccountsSection.GetValue<string>("Password");
            if (string.IsNullOrWhiteSpace(optionsInUse.LoginPassword) && !string.IsNullOrWhiteSpace(loginPassword))
            {
                optionsInUse.LoginPassword = loginPassword;
            }

            return optionsInUse;
        }

        /// <summary>
        /// Container for a Cache Data Result
        /// </summary>
        private class HostsSupportingFeatureResult : IStatusReply, IReadOnlyList<IHostInfoReply>
        {
            private List<IHostInfoReply> underlyingData;

            /// <summary>
            /// Construct
            /// </summary>
            /// <param name="enumerable">An enumeration of the data we hold.</param>
            public HostsSupportingFeatureResult(IEnumerable<IHostInfoReply> enumerable)
            {
                this.underlyingData = enumerable.ToList();
            }

            /// <inheritdoc />
            public IHostInfoReply this[int index] => this.underlyingData[index];

            /// <inheritdoc />
            public IEnumerable<string> ErrorDetails { get; } = Enumerable.Empty<string>();

            /// <inheritdoc />
            public int Count => this.underlyingData.Count;

            /// <inheritdoc />
            public IEnumerator<IHostInfoReply> GetEnumerator()
            {
                return this.underlyingData.GetEnumerator();
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.underlyingData.GetEnumerator();
            }
        }
    }
}
