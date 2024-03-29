﻿using System;
using System.Net;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Factory class for creating SNMP queriers to specific devices.<br/>
    /// Access via singleton property <see cref="Instance" />.
    /// </summary>
    public class SnmpQuerierFactory
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Prevent instantiation from outside.
        /// </summary>
        private SnmpQuerierFactory()
        {
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static SnmpQuerierFactory Instance { get; } = new SnmpQuerierFactory();

        /// <summary>
        /// Creates a new querier to the given host name or address string using default options and allowing caching.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address of the device to query.</param>
        /// <returns>An <see cref="IHamnetQuerier" /> that talks to the given address.</returns>
        public IHamnetQuerier Create(string hostNameOrAddress)
        {
            return this.Create(hostNameOrAddress, null);
        }

        /// <summary>
        /// Creates a new querier to the given address using the given options and cache settings.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address of the device to query.</param>
        /// <param name="options">The options for the query.</param>
        /// <returns>An <see cref="IHamnetQuerier" /> that talks to the given address.</returns>
        public IHamnetQuerier Create(string hostNameOrAddress, IQuerierOptions options)
        {
            if (string.IsNullOrWhiteSpace(hostNameOrAddress))
            {
                throw new ArgumentNullException(nameof(hostNameOrAddress), "host name or address is null, empty or white-space-only");
            }

            if (!hostNameOrAddress.TryGetResolvedConnecionIPAddress(out IPAddress address))
            {
                throw new HamnetSnmpException($"Host name or address '{hostNameOrAddress}' cannot be resolved to a valid IP address", hostNameOrAddress);
            }

            return this.Create(address, options);
        }

        /// <summary>
        /// Creates a new querier to the given address using default options and given cache usage setting.
        /// </summary>
        /// <param name="address">The IP address of the device to query.</param>
        /// <returns>An <see cref="IHamnetQuerier" /> that talks to the given address.</returns>
        public IHamnetQuerier Create(IPAddress address)
        {
            return this.Create(address, null);
        }

        /// <summary>
        /// Creates a new querier to the given address using the given options.
        /// </summary>
        /// <param name="address">The IP address of the device to query.</param>
        /// <param name="options">The options for the query.</param>
        /// <returns>An <see cref="IHamnetQuerier" /> that talks to the given address.</returns>
        public IHamnetQuerier Create(IPAddress address, IQuerierOptions options)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address), "address is null");
            }

            ISnmpLowerLayer lowerLayer = new SnmpLowerLayer(new IpAddress(address), options);

            return this.Create(lowerLayer, options);
        }

        /// <summary>
        /// Creates a new querier using the given lower layer and options.
        /// </summary>
        /// <param name="lowerLayer">The IP address of the device to query.</param>
        /// <param name="options">The options for the query.</param>
        /// <returns>An <see cref="IHamnetQuerier" /> that talks to the given address.</returns>
#pragma warning disable CA1822 // API
        internal IHamnetQuerier Create(ISnmpLowerLayer lowerLayer, IQuerierOptions options)
#pragma warning restore
        {
            if (lowerLayer == null)
            {
                throw new ArgumentNullException(nameof(lowerLayer), "lowerLayer is null");
            }

            IHamnetQuerier querier = null;
            if (options.EnableCaching)
            {
                querier = new CachingHamnetQuerier(lowerLayer, options);
            }
            else
            {
                var detector = new DeviceDetector(lowerLayer);

                IDeviceHandler handler = detector.Detect(options);

                if (handler == null)
                {
                    var errorInfo = $"Cannot obtain a feasible device handler for device '{lowerLayer.Address}'";
                    log.Error(errorInfo);
                    throw new HamnetSnmpException(errorInfo, lowerLayer.Address?.ToString());
                }

                querier = new HamnetQuerier(handler, lowerLayer.Options);
            }

            return querier;
        }
    }
}
