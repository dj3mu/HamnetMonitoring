using System;
using System.Text;
using Newtonsoft.Json;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// A simple container for storing a serializable version of the system data.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut, ItemRequired = Required.Always)]
    internal class SerializableSystemData : IDeviceSystemData
    {
        /// <summary>
        /// Construct from device address.
        /// </summary>
        public SerializableSystemData(IDeviceSystemData inputSystemData)
        {
            if (inputSystemData is null)
            {
                throw new ArgumentNullException(nameof(inputSystemData), "The IDeviceSystemData to make serializable is null");
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

            // we intentionally do not copy the query duration as after deserializting it will have no more meaning
            // because there was in fact no query. So Zero seems much more correct in this context.
        }

        /// <summary>
        /// Prevent default-construction from outside.
        /// </summary>
        internal SerializableSystemData()
        {
        }

        /// <inheritdoc />
        public string Description { get; set; } = "<description not available>";

        /// <inheritdoc />
        public Oid EnterpriseObjectId { get; set; } = null;

        /// <inheritdoc />
        public string Contact { get; set; } = "<contact not available>";

        /// <inheritdoc />
        public string Location { get; set; } = "<location not available>";

        /// <inheritdoc />
        public string Name { get; set; } = "<name not available>";

        /// <inheritdoc />
        [JsonIgnore]
        public TimeSpan? Uptime { get; set; } = null;

        /// <inheritdoc />
        public string Model { get; set; } = "<model not available>";

        /// <inheritdoc />
        public SemanticVersion Version { get; set; } = new SemanticVersion(0, 0, 0, string.Empty, string.Empty);

        /// <inheritdoc />
        public IpAddress DeviceAddress { get; set; }

        /// <inheritdoc />
        public string DeviceModel { get; set; } = "<device model not available>";

        /// <inheritdoc />
        [JsonIgnore]
        public TimeSpan QueryDuration { get; set; } = TimeSpan.Zero;

        public SnmpVersion MaximumSnmpVersion { get; set; }

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