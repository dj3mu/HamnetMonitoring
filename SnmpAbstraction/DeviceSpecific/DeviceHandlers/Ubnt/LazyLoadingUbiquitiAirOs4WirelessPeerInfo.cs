﻿using System;
using System.Linq;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class LazyLoadingUbiquitiAirOs4WirelessPeerInfo : LazyLoadingGenericWirelessPeerInfo
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Field to sum up the query duration on each "Populate" call.
        /// </summary>
        private TimeSpan localQueryDuration = TimeSpan.Zero;

        /// <summary>
        /// Stores the collection generated by the walk operation that triggered creation of this peer info.
        /// </summary>
        private VbCollection walkCollection;
        
        /// <summary>
        /// Backing field storing the peer's MAC address.
        /// </summary>
        private string peerMacBacking = null;

        /// <summary>
        /// Field indicating whether we have already (or tried to) retieved the remote MAC address.
        /// </summary>
        private bool allValuesRetrieved = false;

        /// <summary>
        /// Backing field for IsAccessPoint property.
        /// </summary>
        private bool? isApBacking;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="interfaceId">The interface ID (to use in SNMP get).</param>
        /// <param name="peerWalkCollection">The collection generated by the walk operation that triggered creation of this peer info.</param>
        public LazyLoadingUbiquitiAirOs4WirelessPeerInfo(
            ISnmpLowerLayer lowerSnmpLayer,
            IDeviceSpecificOidLookup oidLookup,
            int interfaceId,
            VbCollection peerWalkCollection)
            : base(
                lowerSnmpLayer,
                oidLookup,
                null,
                interfaceId,
                null)
        {
            this.walkCollection = peerWalkCollection;
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration => this.localQueryDuration;

        /// <inheritdoc />
        protected override string PeerMac
        {
            get
            {
                this.RetrieveAllAvailableValues();

                return this.peerMacBacking;
            }
        }

        /// <inheritdoc />
        public override bool? IsAccessPoint => this.isApBacking;

        /// <summary>
        /// Retrieves (or tries to retrieve) the remote peer's MAC address.
        /// </summary>
        private bool RetrieveAllAvailableValues()
        {
            if (this.allValuesRetrieved)
            { 
                return true;
            }

            var valueToQuery = RetrievableValuesEnum.RxSignalStrengthApAppendMacAndInterfaceId;
            DeviceSpecificOid rxStrengthRootOid;
            if (!this.OidLookup.TryGetValue(valueToQuery, out rxStrengthRootOid))
            {
                log.Warn($"Failed to obtain OID for '{valueToQuery}' ==> Cannot obtain any peer data for device '{this.DeviceAddress}'");
                this.allValuesRetrieved = true;
                this.isApBacking = null;
                this.peerMacBacking = null;
                this.RxSignalStrengthBacking = double.NaN;
                return true;
            }

            Vb rxSignalStrengthEntry = this.walkCollection.SingleOrDefault(v => rxStrengthRootOid.Oid.IsRootOf(v.Oid));
            if (rxSignalStrengthEntry == null)
            {
                log.Warn($"Failed to obtain SNMP response entry for '{valueToQuery}' ==> Cannot obtain any peer data for device '{this.DeviceAddress}'");
                this.allValuesRetrieved = true;
                this.isApBacking = null;
                this.peerMacBacking = null;
                this.RxSignalStrengthBacking = double.NaN;
                return true;
            }

            var macOidFragments = rxSignalStrengthEntry.Oid.Skip(rxStrengthRootOid.Oid.Length).Take(6);
            this.peerMacBacking = macOidFragments.ToHexString();
            this.RxSignalStrengthBacking = rxSignalStrengthEntry.Value.ToInt();

            valueToQuery = RetrievableValuesEnum.WlanRemoteMacAddressAppendInterfaceId;
            DeviceSpecificOid wlanRemoteMacAddressRootOid;
            if (!this.OidLookup.TryGetValue(valueToQuery, out wlanRemoteMacAddressRootOid))
            {
                log.Warn($"Failed to obtain OID for '{valueToQuery}' ==> Cannot obtain IsAccessPoint for device '{this.DeviceAddress}'");
                this.allValuesRetrieved = true;
                this.isApBacking = null;
                return true;
            }

            Oid macPeerMacFieldOid = wlanRemoteMacAddressRootOid.Oid + rxSignalStrengthEntry.Oid.Last();

            this.isApBacking = this.walkCollection.ContainsOid(macPeerMacFieldOid);

            this.allValuesRetrieved = true;

            return true;
        }

        /// <inheritdoc />
        protected override bool RetrieveTxSignalStrength()
        {
            // this value is simply not available for UBNT devices, at least not via SNMP (but for some models even not in Web GUI)
            return false;
        }

        /// <inheritdoc />
        protected override bool RetrieveRxSignalStrength()
        {
            return this.RetrieveAllAvailableValues();
        }

        /// <inheritdoc />
        protected override bool RetrieveLinkUptime()
        {
            // this value is simply not available for UBNT AirOS v 4.x devices, at least not via SNMP (but for some models even not in Web GUI)
            return false;
        }
    }
}