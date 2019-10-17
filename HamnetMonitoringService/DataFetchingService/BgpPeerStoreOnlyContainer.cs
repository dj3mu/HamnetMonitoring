using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SnmpAbstraction;

namespace RestService.DataFetchingService
{
    internal class BgpPeerStoreOnlyContainer : IBgpPeer
    {
        /// <summary>
        /// Copy-construct from an IBgpPeer.
        /// </summary>
        /// <param name="bgpPeer">The source to copy.</param>
        public BgpPeerStoreOnlyContainer(IBgpPeer bgpPeer)
        {
            this.Id = bgpPeer.Id;
            this.Name = bgpPeer.Name;
            this.Instance = bgpPeer.Instance;
            this.RemoteAddress = bgpPeer.RemoteAddress;
            this.RemoteAs = bgpPeer.RemoteAs;
            this.NexthopChoice = bgpPeer.NexthopChoice;
            this.Multihop = bgpPeer.Multihop;
            this.RouteReflect = bgpPeer.RouteReflect;
            this.HoldTime = bgpPeer.HoldTime;
            this.Ttl = bgpPeer.Ttl;
            this.AddressFamilies = bgpPeer.AddressFamilies;
            this.DefaultOriginate = bgpPeer.DefaultOriginate;
            this.RemovePrivateAs = bgpPeer.RemovePrivateAs;
            this.AsOverride = bgpPeer.AsOverride;
            this.Passive = bgpPeer.Passive;
            this.UseBfd = bgpPeer.UseBfd;
            this.RemoteId = bgpPeer.RemoteId;
            this.LocalAddress = bgpPeer.LocalAddress;
            this.Uptime = bgpPeer.Uptime;
            this.PrefixCount = bgpPeer.PrefixCount;
            this.UpdatesSent = bgpPeer.UpdatesSent;
            this.UpdatesReceived = bgpPeer.UpdatesReceived;
            this.WithdrawnSent = bgpPeer.WithdrawnSent;
            this.WithdrawnReceived = bgpPeer.WithdrawnReceived;
            this.RemoteHoldTime = bgpPeer.RemoteHoldTime;
            this.UsedHoldTime = bgpPeer.UsedHoldTime;
            this.UsedKeepaliveTime = bgpPeer.UsedKeepaliveTime;
            this.RefreshCapability = bgpPeer.RefreshCapability;
            this.As4Capability = bgpPeer.As4Capability;
            this.State = bgpPeer.State;
            this.Established = bgpPeer.Established;
            this.Disabled = bgpPeer.Disabled;
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Instance { get; }

        /// <inheritdoc />
        public IPAddress RemoteAddress { get; }

        /// <inheritdoc />
        public long RemoteAs { get; }

        /// <inheritdoc />
        public string NexthopChoice { get; }

        /// <inheritdoc />
        public bool Multihop { get; }

        /// <inheritdoc />
        public bool RouteReflect { get; }

        /// <inheritdoc />
        public TimeSpan HoldTime { get; }

        /// <inheritdoc />
        public string Ttl { get; }

        /// <inheritdoc />
        public IEnumerable<AddressFamily> AddressFamilies { get; }

        /// <inheritdoc />
        public string DefaultOriginate { get; }

        /// <inheritdoc />
        public bool RemovePrivateAs { get; }

        /// <inheritdoc />
        public bool AsOverride { get; }

        /// <inheritdoc />
        public bool Passive { get; }

        /// <inheritdoc />
        public bool UseBfd { get; }

        /// <inheritdoc />
        public string RemoteId { get; }

        /// <inheritdoc />
        public IPAddress LocalAddress { get; }

        /// <inheritdoc />
        public TimeSpan Uptime { get; }

        /// <inheritdoc />
        public long PrefixCount { get; }

        /// <inheritdoc />
        public long UpdatesSent { get; }

        /// <inheritdoc />
        public long UpdatesReceived { get; }

        /// <inheritdoc />
        public long WithdrawnSent { get; }

        /// <inheritdoc />
        public long WithdrawnReceived { get; }

        /// <inheritdoc />
        public TimeSpan RemoteHoldTime { get; }

        /// <inheritdoc />
        public TimeSpan UsedHoldTime { get; }

        /// <inheritdoc />
        public TimeSpan UsedKeepaliveTime { get; }

        /// <inheritdoc />
        public bool RefreshCapability { get; }

        /// <inheritdoc />
        public bool As4Capability { get; }

        /// <inheritdoc />
        public string State { get; }

        /// <inheritdoc />
        public bool Established { get; }

        /// <inheritdoc />
        public bool Disabled { get; }

        /// <inheritdoc />
        public void ForceEvaluateAll()
        {
            // NOP here
        }
    }
}