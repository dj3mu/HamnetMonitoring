using System;
using System.IO;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using SnmpSharpNet;
using tik4net;
using tik4net.Objects;
using tik4net.Objects.System;

namespace SnmpAbstraction
{
    /// <summary>
    /// Detectable device implementation for MikroTik devices
    /// </summary>
    internal class MikrotikApiDetectableDevice : DetectableDeviceBase
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// String in system description to detect RouterOS
        /// </summary>
        private const string RouterOsDetectionString = "RouterOS";

        private static readonly Regex OsVersionExtractionRegex = new Regex(RouterOsDetectionString + @"\s+([0-9.]+)\s+");

        private ITikConnection tikConnection = null;

        private TikConnectionType apiInUse = TikConnectionType.Api_v2;

        private SystemResource sysResource = null;

        private SystemRouterboard sysRouterboard = null;

        private SystemIdentity sysIdent = null;

        /// <summary>
        /// Default-construct
        /// </summary>
        public MikrotikApiDetectableDevice()
        {
            this.Priority = 200;
        }

        /// <inheritdoc />
        public override QueryApis SupportedApi { get; } = QueryApis.VendorSpecific;

        /// <inheritdoc />
        public override bool IsApplicableSnmp(ISnmpLowerLayer snmpLowerLayer, IQuerierOptions options)
        {
            // we're explicitly applicable to vendor-specific API only
            return false;
        }

        /// <inheritdoc />
        public override bool IsApplicableVendorSpecific(IpAddress address, IQuerierOptions options)
        {
            try
            {
                this.tikConnection = ConnectionFactory.CreateConnection(this.apiInUse);

                // following MUST be called before Open()
                this.tikConnection.SendTimeout = Convert.ToInt32(options.Timeout.TotalMilliseconds);
                this.tikConnection.ReceiveTimeout = Convert.ToInt32(options.Timeout.TotalMilliseconds);

                this.tikConnection.Open(address.ToString(), options.LoginUser ?? string.Empty, options.LoginPassword ?? string.Empty);

                this.QuerySystemData();

                return true;
            }
            catch (TikCommandException tikConnectionException)
            {
                this.CollectException("MTik APIv2", tikConnectionException);

                log.Info($"Device {address}: Mikrotik APIv2 connection failed: {tikConnectionException.Message}. Trying APIv1");

                if (this.tikConnection != null)
                {
                    this.tikConnection.Dispose();
                    this.tikConnection = null;
                }
            }
            catch(TikConnectionException tikConnectionException)
            {
                this.CollectException("MTik APIv2", tikConnectionException);

                log.Info($"Device {address}: Mikrotik APIv2 connection failed: {tikConnectionException.Message}. Trying APIv1");

                if (this.tikConnection != null)
                {
                    this.tikConnection.Dispose();
                    this.tikConnection = null;
                }
            }
            catch(IOException ioEx)
            {
                this.CollectException("MTik APIv2 (not falling back to v1 due to IOException being api-version-independent)", ioEx);

                log.Info($"Device {address}: I/O Exception in Mikrotik API connection: {ioEx.Message}. Considering device as not applicable");

                if (this.tikConnection != null)
                {
                    this.tikConnection.Dispose();
                    this.tikConnection = null;
                }

                return false;
            }
            catch(SocketException socketEx)
            {
                this.CollectException("MTik APIv2 (not falling back to v1 due to SocketException being api-version-independent)", socketEx);

                log.Info($"Device {address}: Socket Exception in Mikrotik API connection: {socketEx.Message}. Considering device as not applicable");

                if (this.tikConnection != null)
                {
                    this.tikConnection.Dispose();
                    this.tikConnection = null;
                }

                return false;
            }

            this.apiInUse = TikConnectionType.Api;
            try
            {
                this.tikConnection = ConnectionFactory.CreateConnection(this.apiInUse);

                // following MUST be called before Open()
                this.tikConnection.SendTimeout = Convert.ToInt32(options.Timeout.TotalMilliseconds);
                this.tikConnection.ReceiveTimeout = Convert.ToInt32(options.Timeout.TotalMilliseconds);

                this.tikConnection.Open(address.ToString(), options.LoginUser ?? string.Empty, options.LoginPassword ?? string.Empty);

                this.QuerySystemData();

                return true;
            }
            catch(TikCommandException tikConnectionException)
            {
                this.CollectException("MTik APIv1", tikConnectionException);

                log.Info($"Device {address}: Mikrotik APIv1 connection failed: {tikConnectionException.Message}. Assuming Mikrotik API NOT AVAILABLE");

                if (this.tikConnection != null)
                {
                    this.tikConnection.Dispose();
                    this.tikConnection = null;
                }
                
                return false;
            }
            catch(TikConnectionException tikConnectionException)
            {
                this.CollectException("MTik APIv1", tikConnectionException);

                log.Info($"Device {address}: Mikrotik APIv1 connection failed: {tikConnectionException.Message}. Assuming Mikrotik API NOT AVAILABLE");

                if (this.tikConnection != null)
                {
                    this.tikConnection.Dispose();
                    this.tikConnection = null;
                }
                
                return false;
            }
        }

        /// <inheritdoc />
        public override IDeviceHandler CreateHandler(IpAddress address, IQuerierOptions options)
        {
            if (this.tikConnection == null)
            {
                var ex = new InvalidOperationException($"Device {address}: No Mikrotik API connection available in CreateHandler. Did you call IsApplicableVendorSpecific and receive back true from that call?");
                this.CollectException("MtikApi: MTik connection in CreateHandler(IpAddress, IQuerierOptions)", ex);
                throw ex;
            }

            return new MikrotikApiDeviceHandler(address, this.apiInUse, this.tikConnection, options, this.sysIdent, this.sysResource, this.sysRouterboard);
        }

        /// <summary>
        /// Queries the system data via API.<br/>
        /// This implicitly checks whether the connection is open and authentication was successful.
        /// </summary>
        private void QuerySystemData()
        {
            this.sysResource = this.tikConnection.LoadSingle<SystemResource>();
            this.sysRouterboard = this.tikConnection.LoadSingle<SystemRouterboard>();
            this.sysIdent = this.tikConnection.LoadSingle<SystemIdentity>();
        }
    }
}
