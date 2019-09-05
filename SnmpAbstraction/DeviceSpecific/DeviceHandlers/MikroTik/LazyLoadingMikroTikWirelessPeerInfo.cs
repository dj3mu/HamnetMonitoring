using System;
using System.Text;

namespace SnmpAbstraction
{
    internal class LazyLoadingMikroTikWirelessPeerInfo : LazyHamnetSnmpQuerierResultBase, IWirelessPeerInfo
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
        /// The MAC address of the peer (serves as index in OIDs for MikroTik devices).
        /// </summary>
        private readonly string peerMac;

        /// <summary>
        /// Field to sum up the query duration on each "Populate" call.
        /// </summary>
        private TimeSpan localQueryDuration = TimeSpan.Zero;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="macAddress">The MAC address of the peer (serves as index in OIDs for MikroTik devices).</param>
        /// <param name="interfaceId">The ID of the interface (i.e. the value to append to interface-specific OIDs).</param>
        public LazyLoadingMikroTikWirelessPeerInfo(ISnmpLowerLayer lowerSnmpLayer, DeviceSpecificOidLookup oidLookup, string macAddress, int interfaceId)
            : base(lowerSnmpLayer)
        {
            this.oidLookup = oidLookup;
            this.peerMac = macAddress;
            this.interfaceId = interfaceId;
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration => this.localQueryDuration;

        /// <inheritdoc />
        public string RemoteMacString => this.peerMac;

        /// <inheritdoc />
        public int InterfaceId => this.interfaceId;

        /// <inheritdoc />
        public override void ForceEvaluateAll()
        {
        }

        /// <inheritdoc />
        public override string ToTextString()
        {
            StringBuilder returnBuilder = new StringBuilder(128);

            returnBuilder.Append("Peer ").Append(this.peerMac).AppendLine(":");
            returnBuilder.Append("  - On interface ID: ").Append(this.interfaceId);
            //returnBuilder.Append("  - MAC : ").Append(this.macAddressStringQueried ? this.macAddressStringBacking?.ToString() : "Not yet queried");

            return returnBuilder.ToString();
        }
    }
}
