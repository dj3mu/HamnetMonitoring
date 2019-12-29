﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Worker class for lazy-loading interface details.
    /// /// </summary>
    internal class LazyLoadingAlixWirelessPeerInfos : LazyLoadingGenericWirelessPeerInfos
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Field to sum up the query duration on each "Populate" call.
        /// </summary>
        private TimeSpan localQueryDuration = TimeSpan.Zero;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        public LazyLoadingAlixWirelessPeerInfos(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup)
            : base(lowerSnmpLayer, oidLookup)
        {
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration => this.localQueryDuration;

        /// <inheritdoc />
        protected override bool RetrievePeerInfo()
        {
            Stopwatch durationWatch = Stopwatch.StartNew();

            this.PeerInfosBacking.Clear();

            // Note: The MAC address serves as an index in nested OIDs for MikroTik devices.
            //       So this way, we get the amount of peers as well as an index to them.
            var valueToQuery = RetrievableValuesEnum.WlanRemoteMacAddressWalkRoot;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.OidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid) || interfaceIdRootOid.Oid.IsNull)
            {
                return false;
            }

            var interfaceVbs = this.LowerSnmpLayer.DoWalk(interfaceIdRootOid.Oid);

            durationWatch.Stop();

            this.localQueryDuration = durationWatch.Elapsed;

            log.Debug($"RetrievePeerInfo: Received {interfaceVbs.Count} peer MAC addresses from '{this.DeviceAddress}' using OID {interfaceIdRootOid}");

            foreach (Vb item in interfaceVbs)
            {
                IEnumerable<uint> macOidFragments = item.Value.ToString().HexStringToByteArray(' ').Select(b => Convert.ToUInt32(b));
                int interfaceId = Convert.ToInt32(item.Oid.Last());

                var isAccessPoint = this.CheckIsAccessPoint(interfaceId, out int? numberOfClients);
                this.PeerInfosBacking.Add(
                    new LazyLoadingAlixWirelessPeerInfo(
                        this.LowerSnmpLayer,
                        this.OidLookup,
                        macOidFragments.ToHexString(),
                        interfaceId, // last element of OID contains the interface ID on which this peer is connected
                        isAccessPoint,
                        numberOfClients
                    ));
            }

            return true;
        }

        /// <inheritdoc />
        protected override bool? CheckIsAccessPoint(int interfaceId, out int? numberOfClients)
        {
            var valueToQuery = RetrievableValuesEnum.WirelessClientCount;
            DeviceSpecificOid wirelessClientCountRootOid;
            numberOfClients = null;
            if (this.OidLookup.TryGetValue(valueToQuery, out wirelessClientCountRootOid) && !wirelessClientCountRootOid.Oid.IsNull)
            {
                // finally we need to get the count of registered clients
                // if it's 0, this must be a client (this method will only be called if the registration table
                // contains at least one entry)
                // need to append the interface ID to the client count OID
                var queryOid = wirelessClientCountRootOid.Oid + new Oid(new int[] { interfaceId });

                VbCollection returnCollection = null;
                try
                {
                    returnCollection = this.LowerSnmpLayer.Query(queryOid);
                }
                catch(HamnetSnmpException hmnex)
                {
                    // no wireless client count --> no access point
                    log.Info($"AP / client distinction for wireless interface #{interfaceId} of device '{this.LowerSnmpLayer.Address}': Assuming Client: Query of OID for '{valueToQuery}' threw exception: {hmnex.Message}");
                    return false;
                }

                if (returnCollection.Count == 0)
                {
                    log.Warn($"Unable to get AP / client distinction for wireless interface #{interfaceId} of device '{this.LowerSnmpLayer.Address}': Query of OID for '{valueToQuery}' returned empty result");
                    return null;
                }

                var returnValue = returnCollection[queryOid];

                if (returnValue.Value.Type == 128)
                {
                    // no wireless client count --> no access point
                    log.Info($"AP / client distinction for wireless interface #{interfaceId} of device '{this.LowerSnmpLayer.Address}': Assuming Client: Query of OID for '{valueToQuery}' returned a value of type {returnValue.Value.Type}");
                    return false;
                }

                int snmpNumberOfClients = 0;
                if (!returnValue.Value.TryToInt(out snmpNumberOfClients))
                {
                    return false;
                }
                
                numberOfClients = snmpNumberOfClients;

                return snmpNumberOfClients > 0;
            }

            valueToQuery = RetrievableValuesEnum.WirelessMode;
            if (this.OidLookup.TryGetValue(valueToQuery, out wirelessClientCountRootOid) && !wirelessClientCountRootOid.Oid.IsNull)
            {
                try
                {
                    int wirelessModeInt = this.LowerSnmpLayer.QueryAsInt(wirelessClientCountRootOid.Oid, "Wireless mode");

                    return wirelessModeInt == 1;
                }
                catch(HamnetSnmpException hmnex)
                {
                    // no wireless mode --> dunno
                    log.Info($"AP / client distinction for wireless interface #{interfaceId} of device '{this.LowerSnmpLayer.Address}': Can't determine mode: Query of OID for '{valueToQuery}' threw exception: {hmnex.Message}");
                    return null;
                }
            }

            log.Warn($"Unable to get AP / client distinction for wireless interface #{interfaceId} of device '{this.LowerSnmpLayer.Address}': No OID for value of type '{valueToQuery}' available");

            return null;
        }
    }
}
