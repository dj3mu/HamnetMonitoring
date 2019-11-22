using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class VolatileFetchingSystemData : IDeviceSystemData
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IDeviceSystemData underlyingSystemData;

        private readonly ISnmpLowerLayer lowerLayer;

        private TimeSpan queryDurationBacking = TimeSpan.Zero;

        public VolatileFetchingSystemData(IDeviceSystemData underlyingSystemData, ISnmpLowerLayer lowerLayer)
        {
            this.underlyingSystemData = underlyingSystemData ?? throw new System.ArgumentNullException(nameof(underlyingSystemData));
            this.lowerLayer = lowerLayer ?? throw new System.ArgumentNullException(nameof(lowerLayer));
        }

        /// <inheritdoc />
        public string Description => this.underlyingSystemData.Description;

        /// <inheritdoc />
        public Oid EnterpriseObjectId => this.underlyingSystemData.EnterpriseObjectId;

        /// <inheritdoc />
        public string Contact => this.underlyingSystemData.Contact;

        /// <inheritdoc />
        public string Location => this.underlyingSystemData.Location;

        /// <inheritdoc />
        public string Name => this.underlyingSystemData.Name;

        /// <inheritdoc />
        public TimeSpan? Uptime
        {
            get
            {
                var neededValue = CachableValueMeanings.SystemUptime;
                ICachableOid queryOid = null;
                if (!this.underlyingSystemData.Oids.TryGetValue(neededValue, out queryOid))
                {
                    log.Warn($"Cannot obtain an OID for querying {neededValue} from {this.DeviceAddress} ({this.DeviceModel}): Returning <null> for uptime");
                    return null;
                }
                
                if (queryOid.IsSingleOid && (queryOid.Oid.First() == 0))
                {
                    // value is not available for this device
                    return null;
                }

                var queryResult = this.lowerLayer.QueryAsTimeSpan(queryOid.Oid, "System Uptime");

                return queryResult;
            }
        }

        /// <inheritdoc />
        public string Model => this.underlyingSystemData.Model;

        /// <inheritdoc />
        public SemanticVersion Version => this.underlyingSystemData.Version;

        /// <inheritdoc />
        public SnmpVersion MaximumSnmpVersion => this.underlyingSystemData.MaximumSnmpVersion;

        /// <inheritdoc />
        public DeviceSupportedFeatures SupportedFeatures => this.underlyingSystemData.SupportedFeatures;

        /// <inheritdoc />
        public IpAddress DeviceAddress => this.underlyingSystemData.DeviceAddress;

        /// <inheritdoc />
        public string DeviceModel => this.underlyingSystemData.DeviceModel;

        /// <inheritdoc />
        public TimeSpan QueryDuration => this.queryDurationBacking;

        /// <inheritdoc />
        public IReadOnlyDictionary<CachableValueMeanings, ICachableOid> Oids => this.underlyingSystemData.Oids;

        /// <inheritdoc />
        public void ForceEvaluateAll()
        {
            // NOP here - the volatile values are supposed to be queried every time
        }

        /// <inheritdoc />
        public override string ToString()
        {
           StringBuilder returnBuilder = new StringBuilder(256);

            returnBuilder.Append("  - System Model      : ").AppendLine(string.IsNullOrWhiteSpace(this.Model) ? "not available" : this.Model);
            returnBuilder.Append("  - System SW Version : ").AppendLine((this.Version == null) ? "not available" : this.Version.ToString());
            returnBuilder.Append("  - Supported Features: ").Append(this.SupportedFeatures);
            returnBuilder.Append("  - System Name       : ").AppendLine(this.Name);
            returnBuilder.Append("  - System location   : ").AppendLine(this.Location);
            returnBuilder.Append("  - System description: ").AppendLine(this.Description);
            returnBuilder.Append("  - System admin      : ").AppendLine(this.Contact);
            returnBuilder.Append("  - System uptime     : ").AppendLine(this.Uptime?.ToString());
            returnBuilder.Append("  - System root OID   : ").Append(this.EnterpriseObjectId?.ToString());
            returnBuilder.Append("  - Max. SNMP version : ").Append(this.MaximumSnmpVersion);

            return returnBuilder.ToString();
         }
    }
}