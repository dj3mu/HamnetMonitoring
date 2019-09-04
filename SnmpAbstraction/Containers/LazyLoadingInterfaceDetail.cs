using System;
using System.Diagnostics;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class LazyLoadingInterfaceDetail : HamnetSnmpQuerierResultBase, IInterfaceDetail
    {
        /// <summary>
        /// The OID lookup table.
        /// </summary>
        private readonly DeviceSpecificOidLookup oidLookup;
        
        /// <summary>
        /// The ID of the interface (i.e. the value to append to interface-specific OIDs).
        /// </summary>
        private readonly int interfaceId;

        /// <summary>
        /// Field to sum up the query duration on each "Populate" call.
        /// </summary>
        private TimeSpan localQueryDuration = TimeSpan.Zero;

        /// <summary>
        /// Backing field for <see cref="InterfaceType" /> property.
        /// </summary>
        private int interfaceTypeBacking = int.MinValue;

        /// <summary>
        /// Field indicating whether the interface type has been queried.<br/>
        /// The field is necessary as, by interface definition, null can be a valid
        /// return value if interface type is not available.
        /// </summary>
        private bool interfaceTypeQueried = false;

        /// <summary>
        /// Backing field for <see cref="MacAddressString" /> property.
        /// </summary>
        private string macAddressStringBacking = null;

        /// <summary>
        /// Field indicating whether the MAC address has been queried.<br/>
        /// The field is necessary as, by interface definition, null can be a valid
        /// return value if MAC address is not available.
        /// </summary>
        private bool macAddressStringQueried = false;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="interfaceId">The ID of the interface (i.e. the value to append to interface-specific OIDs).</param>
        public LazyLoadingInterfaceDetail(ISnmpLowerLayer lowerSnmpLayer, DeviceSpecificOidLookup oidLookup, int interfaceId)
            : base(lowerSnmpLayer)
        {
            this.oidLookup = oidLookup;
            this.interfaceId = interfaceId;
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration => this.localQueryDuration;

        /// <inheritdoc />
        public int InterfaceType
        {
            get
            {
                this.PopulateInterfaceType();

                return this.interfaceTypeBacking;
            }
        }

        /// <inheritdoc />
        public string MacAddressString
        {
            get
            {
                this.PopulateMacAddressString();

                return this.macAddressStringBacking;
            }
        }

        /// <inheritdoc />
        public void ForceEvaluateAll()
        {
            this.PopulateInterfaceType();
            this.PopulateMacAddressString();
        }

        /// <inheritdoc />
        public override string ToTextString()
        {
            StringBuilder returnBuilder = new StringBuilder(128);

            returnBuilder.Append("Interface #").Append(this.interfaceId).AppendLine(":");
            returnBuilder.Append("  - Type: ").AppendLine(this.interfaceTypeQueried ? this.interfaceTypeBacking.ToString() : "Not yet queried");
            returnBuilder.Append("  - MAC : ").AppendLine(this.macAddressStringQueried ? this.macAddressStringBacking?.ToString() : "Not yet queried");

            return returnBuilder.ToString();
        }

        private void PopulateMacAddressString()
        {
            if (this.macAddressStringQueried)
            {
                return;
            }

            var valueToQuery = RetrievableValuesEnum.InterfaceMacAddressWalkRoot;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.oidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                this.interfaceTypeBacking = int.MinValue;
                this.macAddressStringQueried = true;
                return;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfactTypeOid = (Oid)interfaceIdRootOid.Oid.Clone();
            interfactTypeOid.Add(this.interfaceId);

            this.macAddressStringBacking = this.LowerSnmpLayer.QueryAsString(interfactTypeOid, "mac address");

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            this.macAddressStringQueried = true;
        }

        private void PopulateInterfaceType()
        {
            if (this.interfaceTypeQueried)
            {
                return;
            }

            var valueToQuery = RetrievableValuesEnum.InterfaceTypeWalkRoot;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.oidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                this.interfaceTypeBacking = int.MinValue;
                this.interfaceTypeQueried = true;
                return;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfactTypeOid = (Oid)interfaceIdRootOid.Oid.Clone();
            interfactTypeOid.Add(this.interfaceId);

            this.interfaceTypeBacking = this.LowerSnmpLayer.QueryAsInt(interfactTypeOid, "interface type");

            durationWatch.Stop();

            this.localQueryDuration += durationWatch.Elapsed;

            this.interfaceTypeQueried = true;
        }
    }
}