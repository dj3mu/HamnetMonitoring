using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class LazyLoadingMimosaInterfaceDetail : LazyLoadingGenericInterfaceDetail
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// A list of string that are used to detect whether the interface is actually related to the wireless link.
        /// </summary>
        private readonly IEnumerable<string> wirelessInterfaceNames;

        /// <summary>
        /// Field to sum up the query duration on each "Populate" call.
        /// </summary>
        private TimeSpan localQueryDuration = TimeSpan.Zero;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="interfaceId">The ID of the interface (i.e. the value to append to interface-specific OIDs).</param>
        /// <param name="wirelessInterfaceNames">A list of string that are used to detect whether the interface is actually related to the wireless link.</param>
        public LazyLoadingMimosaInterfaceDetail(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup, int interfaceId, IEnumerable<string> wirelessInterfaceNames)
            : base(lowerSnmpLayer, oidLookup, interfaceId)
        {
            this.wirelessInterfaceNames = wirelessInterfaceNames;
        }

        /// <summary>
        /// To be overridden by deriving classes in order to actually populate the interface name.
        /// </summary>
        protected override bool RetrieveInterfaceName()
        {
            base.RetrieveInterfaceName();

            if (this.InterfaceNameBacking.Contains("wifi0", StringComparison.InvariantCultureIgnoreCase))
            {
                this.MacAddressStringBacking = "00:00:00:00:00:00";
                this.InterfaceTypeBacking = IanaInterfaceType.Ieee80211;
            }

            return true;
        }
   }
}