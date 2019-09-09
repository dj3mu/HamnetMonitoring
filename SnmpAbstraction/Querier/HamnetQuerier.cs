using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// The main SNMP Querier implementation.
    /// </summary>
    internal partial class HamnetQuerier : IHamnetQuerier
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The device handler to use for obtaining SNMP data.
        /// </summary>
        private IDeviceHandler handler;

        /// <summary>
        /// The options for the query.
        /// </summary>
        private readonly IQuerierOptions options;

        /// <summary>
        /// Creates a new instance using the specified device handler.
        /// </summary>
        /// <param name="handler">The device handler to use for obtaining SNMP data.</param>
        /// <param name="options">The options for the query.</param>
        public HamnetQuerier(IDeviceHandler handler, IQuerierOptions options)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler), "The device handler is null");
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), "The query options are null");
            }

            this.handler = handler;
            this.options = options;
        }

        /// <inheritdoc />
        public IDeviceSystemData SystemData => this.handler.SystemData;

        /// <inheritdoc />
        public IInterfaceDetails NetworkInterfaceDetails => this.handler.NetworkInterfaceDetails;

        /// <inheritdoc />
        public IWirelessPeerInfos WirelessPeerInfos => this.handler.WirelessPeerInfos;

        /// <inheritdoc />
        public IpAddress Address => this.handler.Address;

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

            return new LinkDetails(fetchedDetails, this.Address);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Address.ToString();
        }
    }
}
