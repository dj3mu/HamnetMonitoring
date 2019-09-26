using System;
using System.Diagnostics;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Generic implementation of lazy retrieval of interface details.
    /// </summary>
    /// <remarks>
    /// This class is intentionally not abstract. Its default implementation works using IANA-default
    /// MIB tree and hence the class can work with all kinds of devices that stick to the IANA standard.
    /// </remarks>
    internal class LazyLoadingGenericInterfaceDetail : LazyHamnetSnmpQuerierResultBase, IInterfaceDetail
    {
        /// <summary>
        /// Field to sum up the query duration on each "Populate" call.
        /// </summary>
        private TimeSpan localQueryDuration = TimeSpan.Zero;

        /// <summary>
        /// Field indicating whether the interface type has been queried.<br/>
        /// The field is necessary as, by interface definition, null can be a valid
        /// return value if interface type is not available.
        /// </summary>
        private bool interfaceTypeQueried = false;

        /// <summary>
        /// Field indicating whether the MAC address has been queried.<br/>
        /// The field is necessary as, by interface definition, null can be a valid
        /// return value if MAC address is not available.
        /// </summary>
        private bool macAddressStringQueried = false;

        /// <summary>
        /// Field indicating whether the interface name has been queried.<br/>
        /// The field is necessary as, by interface definition, null can be a valid
        /// return value if MAC address is not available.
        /// </summary>
        private bool interfaceNameQueried = false;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="interfaceId">The ID of the interface (i.e. the value to append to interface-specific OIDs).</param>
        public LazyLoadingGenericInterfaceDetail(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup, int interfaceId)
            : base(lowerSnmpLayer)
        {
            this.OidLookup = oidLookup;
            this.InterfaceId = interfaceId;
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration => this.localQueryDuration;

        /// <inheritdoc />
        public IanaInterfaceType InterfaceType
        {
            get
            {
                this.PopulateInterfaceType();

                return this.InterfaceTypeBacking;
            }
        }

        /// <inheritdoc />
        public string MacAddressString
        {
            get
            {
                this.PopulateMacAddressString();

                return this.MacAddressStringBacking;
            }
        }

        /// <inheritdoc />
        public string InterfaceName
        {
            get
            {
                this.PopulateInterfaceName();

                return this.InterfaceNameBacking;
            }
        }

        /// <inheritdoc />
        public int InterfaceId { get; }

        /// <summary>
        /// Gets the IANA interface type backing field into our deriving classes.
        /// </summary>
        protected IanaInterfaceType InterfaceTypeBacking { get; set; }

        /// <summary>
        /// Gets the OID lookup table.
        /// </summary>
        protected IDeviceSpecificOidLookup OidLookup { get; }

        /// <summary>
        /// Gets the MAC address backing field into our deriving classes.
        /// </summary>
        protected string MacAddressStringBacking { get; set; }

        /// <summary>
        /// Gets the interface name backing field into our deriving classes.
        /// </summary>
        protected string InterfaceNameBacking { get; set; }

        /// <inheritdoc />
        public override void ForceEvaluateAll()
        {
            this.PopulateInterfaceType();
            this.PopulateMacAddressString();
            this.PopulateInterfaceName();
        }

        /// <inheritdoc />
        public override string ToTextString()
        {
            StringBuilder returnBuilder = new StringBuilder(128);

            returnBuilder.Append("Interface #").Append(this.InterfaceId).Append(" (").Append(this.interfaceNameQueried ? this.InterfaceNameBacking : "not available").AppendLine("):");
            returnBuilder.Append("  - Type: ").AppendLine(this.interfaceTypeQueried ? this.InterfaceTypeBacking.ToString() : "not available");
            returnBuilder.Append("  - MAC : ").Append(this.macAddressStringQueried ? this.MacAddressStringBacking?.ToString() : "not available");

            return returnBuilder.ToString();
        }

        /// <summary>
        /// To be overridden by deriving classes in order to actually populate the MAC address string.
        /// </summary>
        protected virtual bool RetrieveMacAddressString()
        {
            var valueToQuery = RetrievableValuesEnum.InterfaceMacAddressWalkRoot;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.OidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                this.MacAddressStringBacking = null;
                return true;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfaceMacOid = interfaceIdRootOid.Oid + new Oid(new int[] { this.InterfaceId });

            VbCollection macQueryResult = this.LowerSnmpLayer.Query(interfaceMacOid);

            var macString = macQueryResult[interfaceMacOid].Value.ToString();
            if (macString.Length <= 6)
            {
                this.MacAddressStringBacking = Encoding.ASCII.GetBytes(macString).ToHexString();
            }
            else if (macString.Split(new char[] { ':', ' ' }, StringSplitOptions.None).Length == 6)
            {
                this.MacAddressStringBacking = macString.Replace(' ', ':');
            }

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }

        /// <summary>
        /// To be overridden by deriving classes in order to actually populate the interface type.
        /// </summary>
        protected virtual bool RetrieveInterfaceType()
        {
            var valueToQuery = RetrievableValuesEnum.InterfaceTypeWalkRoot;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.OidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                this.InterfaceTypeBacking = IanaInterfaceType.NotAvailable;
                return true;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfaceTypeOid = interfaceIdRootOid.Oid + new Oid(new int[] { this.InterfaceId });

            this.InterfaceTypeBacking = (IanaInterfaceType)this.LowerSnmpLayer.QueryAsInt(interfaceTypeOid, "interface type");

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }

        /// <summary>
        /// To be overridden by deriving classes in order to actually populate the interface name.
        /// </summary>
        protected virtual bool RetrieveInterfaceName()
        {
            var valueToQuery = RetrievableValuesEnum.InterfaceNameWalkRoot;
            DeviceSpecificOid interfaceNameRootOid;
            if (!this.OidLookup.TryGetValue(valueToQuery, out interfaceNameRootOid))
            {
                this.InterfaceNameBacking = null;
                return true;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfaceNameOid = interfaceNameRootOid.Oid + new Oid(new int[] { this.InterfaceId });

            this.InterfaceNameBacking = this.LowerSnmpLayer.QueryAsString(interfaceNameOid, "interface name");

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }

        private void PopulateInterfaceName()
        {
            if (this.interfaceNameQueried)
            {
                return;
            }

            this.interfaceNameQueried = this.RetrieveInterfaceName();
        }

        private void PopulateInterfaceType()
        {
            if (this.interfaceTypeQueried)
            {
                return;
            }

            this.interfaceTypeQueried = this.RetrieveInterfaceType();
        }

        private void PopulateMacAddressString()
        {
            if (this.macAddressStringQueried)
            {
                return;
            }

            this.macAddressStringQueried = this.RetrieveMacAddressString();
        }
    }
}