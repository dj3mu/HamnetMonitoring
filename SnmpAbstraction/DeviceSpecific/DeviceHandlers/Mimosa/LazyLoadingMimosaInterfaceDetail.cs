using System;

namespace SnmpAbstraction
{
    internal class LazyLoadingMimosaInterfaceDetail : LazyLoadingGenericInterfaceDetail
    {
        ////private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="interfaceId">The ID of the interface (i.e. the value to append to interface-specific OIDs).</param>
        public LazyLoadingMimosaInterfaceDetail(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup, int interfaceId)
            : base(lowerSnmpLayer, oidLookup, interfaceId)
        {
        }

        protected override bool RetrieveMacAddressString()
        {
            base.RetrieveMacAddressString();
            base.RetrieveInterfaceName();

            this.CheckForWifiMacHack();

            return true;
        }

        protected override bool RetrieveInterfaceType()
        {
            base.RetrieveInterfaceType();
            base.RetrieveInterfaceName();

            this.CheckForWifiMacHack();

            return true;
        }

        /// <summary>
        /// To be overridden by deriving classes in order to actually populate the interface name.
        /// </summary>
        protected override bool RetrieveInterfaceName()
        {
            base.RetrieveInterfaceName();

            this.CheckForWifiMacHack();

            return true;
        }

        private void CheckForWifiMacHack()
        {
            if (this.InterfaceNameBacking.Contains("wifi0", StringComparison.InvariantCultureIgnoreCase))
            {
                this.MacAddressStringBacking = "00:00:00:00:00:00";
                this.InterfaceTypeBacking = IanaInterfaceType.Ieee80211;
            }
        }
   }
}