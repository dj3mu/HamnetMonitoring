﻿using System;
using System.Diagnostics;
using System.Linq;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Worker class for lazy-loading interface details for AirFiber AFLTU devices.
    /// /// </summary>
    internal class LazyLoadingUbiquitiAirFiber60WirelessPeerInfos : LazyLoadingGenericWirelessPeerInfos
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
        public LazyLoadingUbiquitiAirFiber60WirelessPeerInfos(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup)
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
            if (!this.OidLookup.TryGetValue(valueToQuery, out DeviceSpecificOid interfaceIdRootOid))
            {
                return false;
            }

            var interfaceVbs = this.LowerSnmpLayer.DoWalk(interfaceIdRootOid.Oid);

            durationWatch.Stop();

            this.localQueryDuration = durationWatch.Elapsed;

            log.Debug($"RetrievePeerInfo: Received peer MAC addresses from '{this.DeviceAddress}' using OID {interfaceIdRootOid}");

            int index = Convert.ToInt32(interfaceVbs[0].Oid.Last());
            this.PeerInfosBacking.Add(
                new LazyLoadingUbiquitiAirFiber60WirelessPeerInfo(
                    this.LowerSnmpLayer,
                    this.OidLookup,
                    new Oid(interfaceVbs[0].Oid.Skip(interfaceVbs[0].Oid.Length - 7).Take(7).ToArray()),
                    interfaceVbs.Count));

            return true;
        }

        /// <inheritdoc />
        protected override bool? CheckIsAccessPoint(int interfaceId, out int? numberOfClients)
        {
            // NOP:
            // Method CheckIsAccessPoint is not supported for LazyLoadingUbiquitiAirOs4WirelessPeerInfos.
            // Parameter is determined internally in single LazyLoadingUbiquitiAirOs4WirelessPeerInfo.
            numberOfClients = null;
            return null;
        }
    }
}