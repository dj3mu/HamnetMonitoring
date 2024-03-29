﻿using System;
using SemVersion;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Device Handler class for Ubiquiti AirFiber AFLTU devices.
    /// </summary>
    internal class UbiquitiAirFiberAfltuDeviceHandler : GenericDeviceHandler
    {
        /// <summary>
        /// Constructs for the given lower layer.
        /// </summary>
        /// <param name="lowerLayer">The lower layer for talking to this device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="osVersion">The SW version of the device.</param>
        /// <param name="model">The device's model name. Shall be the same name as used for the device name during OID database lookups.</param>
        /// <param name="options">The options to use.</param>
        public UbiquitiAirFiberAfltuDeviceHandler(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup, SemanticVersion osVersion, string model, IQuerierOptions options)
            : base(lowerLayer, oidLookup, osVersion, model, options)
        {
            if ((options.AllowedApis & this.SupportedApi) == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options), $"This device handler doesn't support any of the APIs allowed by the IQuerierOptions (allowed: {options.AllowedApis}, supported {this.SupportedApi}).");
            }

            if (lowerLayer.SystemData is LazyLoadingDeviceSystemData llsd)
            {
                if ((oidLookup.TryGetValue(RetrievableValuesEnum.RxSignalStrengthCh0AppendMacAndInterfaceId, out DeviceSpecificOid oid0) && !oid0.Oid.IsNull)
                   || (oidLookup.TryGetValue(RetrievableValuesEnum.RxSignalStrengthCh1AppendMacAndInterfaceId, out DeviceSpecificOid oid1) && !oid1.Oid.IsNull))
                {
                    // for AirFiber devices we currently only support RSSI querying
                    llsd.SupportedFeatures = DeviceSupportedFeatures.Rssi;
                }
            }
        }

        /// <inheritdoc />
        public override QueryApis SupportedApi { get; } = QueryApis.Snmp;

        /// <inheritdoc />
        public override IBgpPeers FetchBgpPeers(string remotePeerIp)
        {
            throw new System.NotSupportedException("Getting BGP peers is currently not supported for Ubiquiti AirFiber AFLTU devices");
        }

        /// <inheritdoc />
        public override ITracerouteResult Traceroute(IpAddress remoteIp, uint count, TimeSpan timeout, int maxHops)
        {
            throw new System.NotSupportedException("Traceroute is currently not supported for Ubiquiti AirFiber AFLTU devices");
        }

        /// <inheritdoc />
        protected override IInterfaceDetails RetrieveDeviceInterfaceDetails(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup)
        {
            return new LazyLoadingUbiquitiInterfaceDetails(this.LowerLayer, this.OidLookup);
        }

        /// <inheritdoc />
        protected override IWirelessPeerInfos RetrieveWirelessPeerInfos(ISnmpLowerLayer lowerLayer, IDeviceSpecificOidLookup oidLookup)
        {
            return new LazyLoadingUbiquitiAirFiberAfltuWirelessPeerInfos(this.LowerLayer, this.OidLookup);
        }
    }
}
