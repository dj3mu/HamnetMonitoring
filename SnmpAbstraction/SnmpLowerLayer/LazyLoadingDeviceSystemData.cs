using System;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Implementation of <see cref="IDeviceSystemData" /> that does as
    /// lazy as possible loading of the property (i.e. on first use).
    /// </summary>
    internal class LazyLoadingDeviceSystemData : IDeviceSystemData
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The cache field for the system description.
        /// </summary>
        private string systemDescrition;

        /// <summary>
        /// Field indicating whether the system description has been queried.<br/>
        /// The field is necessary as, by interface definition, null can be a valid
        /// return value if description is not available.
        /// </summary>
        private bool systemDescriptionQueried = false;

        /// <summary>
        /// The cache field for the system OID.
        /// </summary>
        private Oid enterpriseObjectId = null;

        /// <summary>
        /// Field indicating whether the system OID has been queried.<br/>
        /// The field is necessary as, by interface definition, null can be a valid
        /// return value if OID is not available.
        /// </summary>
        private bool enterpriseObjectIdQueried = false;

        /// <summary>
        /// The cache field for the system's admin contact.
        /// </summary>
        private string systemAdminContact = null;

        /// <summary>
        /// Field indicating whether the system admin contact has been queried.<br/>
        /// The field is necessary as, by interface definition, null can be a valid
        /// return value if admin contact is not available.
        /// </summary>
        private bool systemAdminContactQueried = false;

        /// <summary>
        /// The cache field for the system location.
        /// </summary>
        private string systemLocation;

        /// <summary>
        /// Field indicating whether the system admin contact has been queried.<br/>
        /// The field is necessary as, by interface definition, null can be a valid
        /// return value if system location is not available.
        /// </summary>
        private bool systemLocationQueried = false;

        /// <summary>
        /// The cache field for the system name.
        /// </summary>
        private string systemName;

        /// <summary>
        /// Field indicating whether the system admin contact has been queried.<br/>
        /// The field is necessary as, by interface definition, null can be a valid
        /// return value if system name is not available.
        /// </summary>
        private bool systemNameQueried = false;

        /// <summary>
        /// The cache field for the system uptime.
        /// </summary>
        private TimeSpan? uptime;

        /// <summary>
        /// Field indicating whether the system admin contact has been queried.<br/>
        /// The field is necessary as, by interface definition, null can be a valid
        /// return value if system uptime is not available.
        /// </summary>
        private bool uptimeQueried;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer"></param>
        public LazyLoadingDeviceSystemData(ISnmpLowerLayer lowerSnmpLayer)
        {
            this.LowerSnmpLayer = lowerSnmpLayer;
        }

        /// <summary>
        /// Gets the communication layer in use.
        /// </summary>
        public ISnmpLowerLayer LowerSnmpLayer { get; }

        /// <inheritdoc />
        public string Description
        {
            get
            {
                if (!this.systemDescriptionQueried)
                {
                    this.systemDescrition = this.LowerSnmpLayer.QueryAsString(new Oid(".1.3.6.1.2.1.1.1.0"), "system description");
                    this.systemDescriptionQueried = true;
                }

                return this.systemDescrition;
            }
        }

        /// <inheritdoc />
        public Oid EnterpriseObjectId
        {
            get
            {
                if (!this.enterpriseObjectIdQueried)
                {
                    this.enterpriseObjectId = this.LowerSnmpLayer.QueryAsOid(new Oid(".1.3.6.1.2.1.1.2.0"), "system enterprise OID");
                    this.enterpriseObjectIdQueried = true;
                }

                return this.enterpriseObjectId;
            }
        }

        /// <inheritdoc />
        public string Contact
        {
            get
            {
                if (!this.systemAdminContactQueried)
                {
                    this.systemAdminContact = this.LowerSnmpLayer.QueryAsString(new Oid(".1.3.6.1.2.1.1.4.0"), "system adminstrative contact");
                    this.systemAdminContactQueried = true;
                }

                return this.systemAdminContact;
            }
        }

        /// <inheritdoc />
        public string Location
        {
            get
            {
                if (!this.systemLocationQueried)
                {
                    this.systemLocation = this.LowerSnmpLayer.QueryAsString(new Oid(".1.3.6.1.2.1.1.6.0"), "system location");
                    this.systemLocationQueried = true;
                }

                return this.systemLocation;
            }
        }

        /// <inheritdoc />
        public string Name
        {
            get
            {
                if (!this.systemNameQueried)
                {
                    this.systemName = this.LowerSnmpLayer.QueryAsString(new Oid(".1.3.6.1.2.1.1.5.0"), "system name");
                    this.systemNameQueried = true;
                }

                return this.systemName;
            }
        }

        /// <inheritdoc />
        public TimeSpan? Uptime
        {
            get
            {
                if (!this.uptimeQueried)
                {
                    TimeTicks uptimeTicks = this.LowerSnmpLayer.QueryAsTimeTicks(new Oid(".1.3.6.1.2.1.1.3.0"), "system uptime");
                    if (uptimeTicks != null)
                    {
                        this.uptime = TimeSpan.FromMilliseconds(uptimeTicks.Milliseconds);
                    }
                    else
                    {
                        log.Debug($"Querying for system uptime of '{this.LowerSnmpLayer.Address}' produced null or empty uptime string");
                        this.uptime = null;
                    }

                    this.uptimeQueried = true;
                }

                return this.uptime;
            }
        }
        
        /// <inheritdoc />
        public string Model => throw new NotImplementedException();

        /// <inheritdoc />
        public string SoftwareVersionString => throw new NotImplementedException();

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder returnBuilder = new StringBuilder(256);

            returnBuilder.Append("System ").Append(this.LowerSnmpLayer.Address).AppendLine(":");
            returnBuilder.Append("  - System Name        (queried=").Append(this.systemNameQueried).Append("): ").AppendLine(this.systemName);
            returnBuilder.Append("  - System location    (queried=").Append(this.systemLocationQueried).Append("): ").AppendLine(this.systemLocation);
            returnBuilder.Append("  - System description (queried=").Append(this.systemDescriptionQueried).Append("): ").AppendLine(this.systemDescrition);
            returnBuilder.Append("  - System admin       (queried=").Append(this.systemAdminContactQueried).Append("): ").AppendLine(this.systemAdminContact);
            returnBuilder.Append("  - System uptime      (queried=").Append(this.uptimeQueried).Append("): ").AppendLine(this.uptime?.ToString());
            returnBuilder.Append("  - System root OID    (queried=").Append(this.enterpriseObjectIdQueried).Append("): ").AppendLine(this.enterpriseObjectId?.ToString());
            returnBuilder.Append("  - System model       (queried=").Append(false).Append("): ").AppendLine("Querying not yet implemented");
            returnBuilder.Append("  - System SW version  (queried=").Append(false).Append("): ").AppendLine("Querying not yet implemented");

            return returnBuilder.ToString();
        }
    }
}
