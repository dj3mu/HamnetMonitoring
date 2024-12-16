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
    internal class HamnetQuerier : IHamnetQuerier
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
        /// To detect redundant calls to <see cref="Dispose()" />.
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Creates a new instance using the specified device handler.
        /// </summary>
        /// <param name="handler">The device handler to use for obtaining SNMP data.</param>
        /// <param name="options">The options for the query.</param>
        public HamnetQuerier(IDeviceHandler handler, IQuerierOptions options)
        {
            this.handler = handler ?? throw new ArgumentNullException(nameof(handler), "The device handler is null");
            this.options = options ?? throw new ArgumentNullException(nameof(options), "The query options are null");
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
        // ~HamnetQuerier()
        // {
        //   // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
        //   Dispose(false);
        // }

        /// <inheritdoc />
        public IDeviceSystemData SystemData => this.handler.SystemData;

        /// <inheritdoc />
        public IInterfaceDetails NetworkInterfaceDetails => this.handler.NetworkInterfaceDetails;

        /// <inheritdoc />
        public IWirelessPeerInfos WirelessPeerInfos => this.handler.WirelessPeerInfos;

        /// <inheritdoc />
        public IpAddress Address => this.handler.Address;

        /// <inheritdoc />
        public QueryApis Api => this.handler.SupportedApi;

        /// <inheritdoc />
        public Type HandlerType => this.handler.GetType();

        /// <inheritdoc />
        public IBgpPeers FetchBgpPeers(string remotePeerIp)
        {
            return this.handler.FetchBgpPeers(remotePeerIp);
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
                if (!remoteHostNamesOrIp.TryGetResolvedConnecionIPAddress(out IPAddress outAddress))
                {
                    log.Error($"Cannot resolve host name or IP string '{remoteHostNamesOrIp}' to a valid IPAddress. Skipping that remote for link detail fetching");
                }

                return SnmpQuerierFactory.Instance.Create(outAddress, this.options);
            }).ToList();

            if (remoteQueriers.Count == 0)
            {
                throw new InvalidOperationException($"No remote IP address available at all after resolving {remoteHostNamesOrIps.Length} host name or address string to IP addresses");
            }

            var linkDetails = FetchLinkDetails(remoteQueriers.ToArray());

            linkDetails.ForceEvaluateAll();

            foreach (var querier in remoteQueriers)
            {
                querier.Dispose();
            }

            remoteQueriers.Clear();

            return linkDetails;
        }

        /// <inheritdoc />
        public ILinkDetails FetchLinkDetails(params IHamnetQuerier[] remoteQueriers)
        {
            List<ILinkDetail> fetchedDetails = new List<ILinkDetail>(remoteQueriers.Length);
            foreach (var remoteQuerier in remoteQueriers)
            {
                var linkDetectionAlgorithm = new LinkDetectionAlgorithm(this, remoteQuerier);
                fetchedDetails.AddRange(linkDetectionAlgorithm.DoQuery().Details);
            }

            return new LinkDetails(fetchedDetails, this.Address, this.SystemData.DeviceModel);
        }

        /// <inheritdoc />
        public ITracerouteResult Traceroute(string remoteHostNameOrIp, uint count, TimeSpan timeout, int maxHops)
        {
            if (!remoteHostNameOrIp.TryGetResolvedConnecionIPAddress(out IPAddress outAddress))
            {
                throw new HamnetSnmpException($"Cannot resolve host name or IP string '{remoteHostNameOrIp}' to a valid IPAddress.");
            }

            return this.handler.Traceroute(new IpAddress(outAddress), count, timeout, maxHops);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{this.Address} ({this.SystemData?.DeviceModel})";
        }

        public void Dispose()
        {
            this.Dispose(true);

            // TODO: Uncomment only if finalizer is overloaded above.
            // GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.handler != null)
                    {
                        this.handler.Dispose();
                        this.handler = null;
                    }
                }

                this.disposedValue = true;
            }
        }
    }
}
