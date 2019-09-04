using System;
using System.Diagnostics;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Implementation of <see cref="IDeviceSystemData" /> that does as
    /// lazy as possible loading of the property (i.e. on first use).
    /// </summary>
    /// <remarks>This SHALL BE THE ONLY class that has hard-coded OIDs. All other classes shall use
    /// the database. But we have to use hard-coded OIDs here because this is the class that is used
    /// to identify a device. Putting the OIDs needed for this into the database we would end up with a chicken-and-egg problem.
    /// </remarks>
    internal class LazyLoadingDeviceSystemData : HamnetSnmpQuerierResultBase, IDeviceSystemData
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        /// Field to sum up the query duration on each "Populate" call.
        /// </summary>
        private TimeSpan queryDurationBacking;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        public LazyLoadingDeviceSystemData(ISnmpLowerLayer lowerSnmpLayer)
            : base(lowerSnmpLayer)
        {
        }

        /// <inheritdoc />
        public string Description
        {
            get
            {
                this.PopulateSystemDescription();

                return this.systemDescrition;
            }
        }

        /// <inheritdoc />
        public Oid EnterpriseObjectId
        {
            get
            {
                this.PopulateEnterpriseOid();

                return this.enterpriseObjectId;
            }
        }

        /// <inheritdoc />
        public string Contact
        {
            get
            {
                this.PopulateAdminContact();

                return this.systemAdminContact;
            }
        }

        /// <inheritdoc />
        public string Location
        {
            get
            {
                this.PopulateLocation();

                return this.systemLocation;
            }
        }

        /// <inheritdoc />
        public string Name
        {
            get
            {
                this.PopulateSystemName();

                return this.systemName;
            }
        }

        /// <inheritdoc />
        public TimeSpan? Uptime
        {
            get
            {
                this.PopulateUptime();

                return this.uptime;
            }
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration => this.queryDurationBacking;

        /// <inheritdoc />
        public void ForceEvaluateAll()
        {
            this.PopulateAdminContact();
            this.PopulateEnterpriseOid();
            this.PopulateLocation();
            this.PopulateSystemDescription();
            this.PopulateSystemName();
            this.PopulateUptime();
        }

        /// <inheritdoc />
        public override string ToTextString()
        {
            StringBuilder returnBuilder = new StringBuilder(256);

            returnBuilder.Append("  - System Name        (queried=").Append(this.systemNameQueried).Append("): ").AppendLine(this.systemName);
            returnBuilder.Append("  - System location    (queried=").Append(this.systemLocationQueried).Append("): ").AppendLine(this.systemLocation);
            returnBuilder.Append("  - System description (queried=").Append(this.systemDescriptionQueried).Append("): ").AppendLine(this.systemDescrition);
            returnBuilder.Append("  - System admin       (queried=").Append(this.systemAdminContactQueried).Append("): ").AppendLine(this.systemAdminContact);
            returnBuilder.Append("  - System uptime      (queried=").Append(this.uptimeQueried).Append("): ").AppendLine(this.uptime?.ToString());
            returnBuilder.Append("  - System root OID    (queried=").Append(this.enterpriseObjectIdQueried).Append("): ").Append(this.enterpriseObjectId?.ToString());

            return returnBuilder.ToString();
        }

        /// <summary>
        /// Populates the <see cref="systemDescrition" /> cache field if not yet done.
        /// </summary>
        private void PopulateSystemDescription()
        {
            if (this.systemDescriptionQueried)
            {
                return;
            }
            
            this.AddQueryDuration( () => {
                this.systemDescrition = this.LowerSnmpLayer.QueryAsString(new Oid(".1.3.6.1.2.1.1.1.0"), "system description");
            });

            this.systemDescriptionQueried = true;
        }

        /// <summary>
        /// Populates the <see cref="enterpriseObjectId" /> cache field if not yet done.
        /// </summary>
        private void PopulateEnterpriseOid()
        {
            if (this.enterpriseObjectIdQueried)
            {
                return;
            }

            this.AddQueryDuration( () => {
                this.enterpriseObjectId = this.LowerSnmpLayer.QueryAsOid(new Oid(".1.3.6.1.2.1.1.2.0"), "system enterprise OID");
            });

            this.enterpriseObjectIdQueried = true;
        }

        /// <summary>
        /// Populates the <see cref="systemAdminContact" /> cache field if not yet done.
        /// </summary>
        private void PopulateAdminContact()
        {
            if (this.systemAdminContactQueried)
            {
                return;
            }
            
            this.AddQueryDuration( () => {
                this.systemAdminContact = this.LowerSnmpLayer.QueryAsString(new Oid(".1.3.6.1.2.1.1.4.0"), "system adminstrative contact");
            });

            this.systemAdminContactQueried = true;
        }

        /// <summary>
        /// Populates the <see cref="systemLocation" /> cache field if not yet done.
        /// </summary>
        private void PopulateLocation()
        {
            if (this.systemLocationQueried)
            {
                return;
            }

            this.AddQueryDuration( () => {
                this.systemLocation = this.LowerSnmpLayer.QueryAsString(new Oid(".1.3.6.1.2.1.1.6.0"), "system location");
            });

            this.systemLocationQueried = true;
        }

        /// <summary>
        /// Populates the <see cref="systemName" /> cache field if not yet done.
        /// </summary>
        private void PopulateSystemName()
        {
            if (this.systemNameQueried)
            {
                return;
            }

            this.AddQueryDuration( () => {
                this.systemName = this.LowerSnmpLayer.QueryAsString(new Oid(".1.3.6.1.2.1.1.5.0"), "system name");
            });

            this.systemNameQueried = true;
        }

        /// <summary>
        /// Times the given operation and adds the duration to the global query duration.
        /// </summary>
        /// <param name="action">The action to time.</param>
        private void AddQueryDuration(Action action)
        {
            Stopwatch durationWatch = Stopwatch.StartNew();

            action();

            durationWatch.ToString();

            this.queryDurationBacking = this.queryDurationBacking + durationWatch.Elapsed;
        }

        /// <summary>
        /// Populates the <see cref="uptime" /> cache field if not yet done.
        /// </summary>
        private void PopulateUptime()
        {
            if (this.uptimeQueried)
            {
                return;
            }

            this.AddQueryDuration( () => {
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
            });

            this.uptimeQueried = true;
        }
    }
}
