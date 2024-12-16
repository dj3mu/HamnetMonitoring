using System;
using System.Collections.Generic;
using System.Text;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// A simple container for just storing a system data.
    /// </summary>
    internal class SystemDataStoreOnlyContainer : IDeviceSystemData
    {
        /// <summary>
        /// Construct from device address.
        /// </summary>
        public SystemDataStoreOnlyContainer(IDeviceSystemData inputSystemData)
        {
            if (inputSystemData is null)
            {
                throw new ArgumentNullException(nameof(inputSystemData), "The IDeviceSystemData to 'store only' is null");
            }

            // as we'll anyway need all values, we trigger immediate aquisition hoping for better performance
            inputSystemData.ForceEvaluateAll();

            this.Description = inputSystemData.Description;
            this.EnterpriseObjectId = inputSystemData.EnterpriseObjectId;
            this.Contact = inputSystemData.Contact;
            this.Location = inputSystemData.Location;
            this.Name = inputSystemData.Name;
            this.Uptime = inputSystemData.Uptime;
            this.Model = inputSystemData.Model;
            this.Version = inputSystemData.Version;
            this.DeviceAddress = inputSystemData.DeviceAddress;
            this.DeviceModel = inputSystemData.DeviceModel;
            this.SupportedFeatures = inputSystemData.SupportedFeatures;
            this.Oids = inputSystemData.Oids;
            this.MinimumSnmpVersion = inputSystemData.MinimumSnmpVersion;
            this.MaximumSnmpVersion = inputSystemData.MaximumSnmpVersion;

            // we intentionally do not copy the query duration as after deserializting it will have no more meaning
            // because there was in fact no query. So Zero seems much more correct in this context.
        }

        /// <summary>
        /// Prevent default-construction from outside.
        /// </summary>
        private SystemDataStoreOnlyContainer()
        {
        }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public Oid EnterpriseObjectId { get; }

        /// <inheritdoc />
        public string Contact { get; }

        /// <inheritdoc />
        public string Location { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public TimeSpan? Uptime { get; }

        /// <inheritdoc />
        public string Model { get; }

        /// <inheritdoc />
        public SemanticVersion Version { get; }

        /// <inheritdoc />
        public IpAddress DeviceAddress { get; }

        /// <inheritdoc />
        public string DeviceModel { get; }

        /// <inheritdoc />
        public TimeSpan QueryDuration { get; }

        /// <inheritdoc />
        public SnmpVersion MaximumSnmpVersion { get; }

        /// <inheritdoc />
        public DeviceSupportedFeatures SupportedFeatures { get; }

        /// <inheritdoc />
        public IReadOnlyDictionary<CachableValueMeanings, ICachableOid> Oids { get; }

        /// <inheritdoc />
        public SnmpVersion MinimumSnmpVersion { get; }

        /// <inheritdoc />
        public void ForceEvaluateAll()
        {
            // Nothing to be done here
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
            returnBuilder.Append("  - Min. SNMP version : ").Append(this.MinimumSnmpVersion);
            returnBuilder.Append("  - Max. SNMP version : ").Append(this.MaximumSnmpVersion);

            return returnBuilder.ToString();
        }
    }
}