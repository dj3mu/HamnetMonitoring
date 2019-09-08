using System;
using System.Collections.Generic;
using System.Diagnostics;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class LazyLoadingUbiquitiInterfaceDetail : LazyLoadingGenericInterfaceDetail
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Field to sum up the query duration on each "Populate" call.
        /// </summary>
        private TimeSpan localQueryDuration = TimeSpan.Zero;

        /// <summary>
        /// Field to indicate whether we've already queried (and assigned) interface type and name (for UBNT done in a single query).
        /// </summary>
        private bool typeAndNameQueried = false;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="interfaceId">The ID of the interface (i.e. the value to append to interface-specific OIDs).</param>
        public LazyLoadingUbiquitiInterfaceDetail(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup, int interfaceId)
            : base(lowerSnmpLayer, oidLookup, interfaceId)
        {
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration => this.localQueryDuration;

        /// <inheritdoc />
        protected override bool RetrieveInterfaceName()
        {
            if (this.typeAndNameQueried)
            {
                return true;
            }

            // for UBNT we fetch type and name in a single query
            return this.RetrieveInterfaceType();
        }

        /// <inheritdoc />
        protected override bool RetrieveInterfaceType()
        {
            if (this.typeAndNameQueried)
            {
                return true;
            }

            // we want to combine the query for type and name for performance reasons
            List<Oid> oidsToQuery = new List<Oid>();

            var valueToQuery = RetrievableValuesEnum.InterfaceTypeWalkRoot;
            DeviceSpecificOid interfaceTypeRootOid;
            if (!this.OidLookup.TryGetValue(valueToQuery, out interfaceTypeRootOid))
            {
                log.Warn($"Cannot find OID to get interface type for UBNT device '{this.DeviceAddress}'.");
                this.InterfaceTypeBacking = IanaInterfaceType.NotAvailable;
                return true;
            }

            var interfaceTypeOid = (Oid)interfaceTypeRootOid.Oid.Clone();
            interfaceTypeOid.Add(this.InterfaceId);
            oidsToQuery.Add(interfaceTypeOid);

            valueToQuery = RetrievableValuesEnum.InterfaceNameWalkRoot;
            DeviceSpecificOid interfaceNameRootOid;
            Oid interfaceNameOid = null;
            if (!this.OidLookup.TryGetValue(valueToQuery, out interfaceNameRootOid))
            {
                log.Warn($"Cannot find OID to get interface name for UBNT device '{this.DeviceAddress}'. Will continue, but cannot ensure that the interface type is actually correct.");
            }
            else
            {
                interfaceNameOid = (Oid)interfaceNameRootOid.Oid.Clone();
                interfaceNameOid.Add(this.InterfaceId);
                oidsToQuery.Add(interfaceNameOid);
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var retrievedValues = this.LowerSnmpLayer.Query(oidsToQuery);

            IanaInterfaceType retrievedType = (IanaInterfaceType)retrievedValues[interfaceTypeOid].Value.ToInt();
            
            durationWatch.Stop();

            // Ubiquiti IS CRAZY: They're reporting all interfaces as type 6 "ethernetcsmacd" ...
            // We have to use a hack and see the interface name to detect (and finally report back) the actual type
            if ((interfaceNameOid != null) && retrievedValues.ContainsOid(interfaceNameOid))
            {
                this.InterfaceTypeBacking = this.ConvertUbntTypeToIeee80211typeForWifiInterfaces(retrievedType, retrievedValues[interfaceNameOid].Value.ToString());
            }
            else
            {
                log.Warn($"Interface name for UBNT device '{this.DeviceAddress}' not received. Cannot ensure that the interface type '{retrievedType}' is actually correct.");
                this.InterfaceTypeBacking = retrievedType;
            }

            this.localQueryDuration += durationWatch.Elapsed;

            return true;
        }

        /// <summary>
        /// Convert the UBNT returned type to the correct type by checking the interface name string (kind of hacky but the only way).
        /// </summary>
        /// <param name="retrievedType">The (bad) type as retrieved from UBNT device.</param>
        /// <param name="interfaceName">The interface name.</param>
        /// <returns>The (good) type reflecting what the interface really is.</returns>
        private IanaInterfaceType ConvertUbntTypeToIeee80211typeForWifiInterfaces(IanaInterfaceType retrievedType, string interfaceName)
        {
            if (retrievedType != IanaInterfaceType.EthernetCsmacd)
            {
                // Buggy UBNT devices return EthernetCsmacd for all interfaces.
                // If we find anything else we must assume that the device is right with the type.
                return retrievedType;
            }

            if (interfaceName.ToUpperInvariant().Contains("WIFI"))
            {
                return IanaInterfaceType.Ieee80211;
            }

            if (interfaceName.ToUpperInvariant().Contains("BR"))
            {
                return IanaInterfaceType.Bridge;
            }

            return retrievedType;
        }
    }
}