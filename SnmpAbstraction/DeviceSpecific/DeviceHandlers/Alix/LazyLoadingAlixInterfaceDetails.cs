using System;

namespace SnmpAbstraction
{
    /// <summary>
    /// Worker class for lazy-loading interface details.
    /// </summary>
    /// <remarks>
    /// This class is intentionally not abstract. Its default implementation works using IANA-default
    /// MIB tree and hence the class can work with all kinds of devices that stick to the IANA standard.
    /// </remarks>
    internal class LazyLoadingAlixInterfaceDetails : LazyLoadingGenericInterfaceDetails
    {
        /// <summary>
        /// Field to sum up the query duration on each "Populate" call.
        /// </summary>
        private TimeSpan localQueryDuration = TimeSpan.Zero;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        public LazyLoadingAlixInterfaceDetails(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup)
            : base(lowerSnmpLayer, oidLookup)
        {
        }

        /// <inheritdoc />
        protected override IInterfaceDetail InstantiateInterfaceDetail(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup, int interfaceId)
        {
            return new LazyLoadingAlixInterfaceDetail(this.LowerSnmpLayer, this.OidLookup, interfaceId, new string[] { "WLAN" });
        }
    }
}
