using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SnmpAbstraction
{
    /// <summary>
    /// Enumeration of peering state.
    /// </summary>
    public enum PeeringState
    {
        /// <summary>
        /// The peering state is currently unknown.
        /// </summary>
        Unknown = 0x0,

        /// <summary>
        /// The peering is idle (e.g. configured but administratively disabled).
        /// </summary>
        Idle = 0x1,
        
        /// <summary>
        /// The peering is trying to connect to remote.
        /// </summary>
        Connect = 0x2,
        
        /// <summary>
        /// The peering is active.
        /// </summary>
        Active = 0x4,
        
        /// <summary>
        /// The peering has an open sent.
        /// </summary>
        Opensent = 0x8,
        
        /// <summary>
        /// The peering has an open confirmation.
        /// </summary>
        Openconfirm = 0x9,
        
        /// <summary>
        /// The peering is established and all data has been exchanged successfully.
        /// </summary>
        Established = 0xF
    }

    /// <summary>
    /// Interface to the data of a single BGP peer.
    /// </summary>
    public interface IBgpPeer : ILazyEvaluated
    {
        /// <summary>
        /// Gets a unique ID of the peer entry.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the name of the peer.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the name of the BGP instance that this peeer is associated with.
        /// </summary>
        string Instance { get; }

        /// <summary>
        /// Gets the IP address of the remote peer.
        /// </summary>
        IPAddress RemoteAddress { get; }

        /// <summary>
        /// Gets the AS number of the remote peer.
        /// </summary>
        long RemoteAs { get; }

        /// <summary>
        /// Gets the choice for the next hop.
        /// </summary>
        string NexthopChoice { get; }

        /// <summary>
        /// Gets a value indicating whether this is a multi-hop peer.
        /// </summary>
        bool Multihop { get; }

        /// <summary>
        /// Gets a value indicating whether to reflect this route.
        /// </summary>
        /// <value></value>
        bool RouteReflect { get; }

        /// <summary>
        /// Gets the hold time of this route.
        /// </summary>
        TimeSpan HoldTime { get; }

        /// <summary>
        /// Gets the TTL setting.
        /// </summary>
        string Ttl { get; }

        /// <summary>
        /// Gets the address families for which routing is exchanged with this peer.
        /// </summary>
        IEnumerable<AddressFamily> AddressFamilies { get; }

        /// <summary>
        /// Gets the setting whether to originate by default.
        /// </summary>
        string DefaultOriginate { get; }

        /// <summary>
        /// Gets a value indicating whether the remote is a private AS.
        /// </summary>
        bool RemovePrivateAs { get; }

        /// <summary>
        /// Gets a value indicating whether the AS is overridden.
        /// </summary>
        bool AsOverride { get; }
        
        /// <summary>
        /// Gets a value indicating whether the peer connection is in passive mode.
        /// </summary>
        bool Passive { get; }

        /// <summary>
        /// Gets a value indicating whether BFD is used.
        /// </summary>
        bool UseBfd { get; }

        /// <summary>
        /// Gets a string identifying the remote peer.
        /// </summary>
        string RemoteId { get; }

        /// <summary>
        /// Gets the local address used when talking to the remote peer.
        /// </summary>
        IPAddress LocalAddress { get; }

        /// <summary>
        /// Gets the uptime of the BGP connection to this peer.
        /// </summary>
        TimeSpan Uptime { get; }

        /// <summary>
        /// Gets the number of prefixes routed to?/exchanged with? this peer?
        /// </summary>
        long PrefixCount { get; }

        /// <summary>
        /// Gets the number of BGP updates sent to this peer.
        /// </summary>
        long UpdatesSent { get; }

        /// <summary>
        /// Gets the number of BGP updates received from this peer.
        /// </summary>
        long UpdatesReceived { get; }

        /// <summary>
        /// Gets the number of BGP withraws sent to this peer.
        /// </summary>
        long WithdrawnSent { get; }

        /// <summary>
        /// Gets the number of BGP withraws received from this peer.
        /// </summary>
        long WithdrawnReceived { get; }

        /// <summary>
        /// Gets the remote hold time of this peer.
        /// </summary>
        TimeSpan RemoteHoldTime { get; }

        /// <summary>
        /// Gets the used hold time with this peer.
        /// </summary>
        TimeSpan UsedHoldTime { get; }

        /// <summary>
        /// Gets the used keep-alive time with this peer.
        /// </summary>
        TimeSpan UsedKeepaliveTime { get; }

        /// <summary>
        /// Gets a value indicating whether this peer connection has the refresh capability.
        /// </summary>
        bool RefreshCapability { get; }

        /// <summary>
        /// Gets a value indicating whether this peer connection has the AS4 capability.
        /// </summary>
        bool As4Capability { get; }

        /// <summary>
        /// Gets the state of this peer.
        /// </summary>
        string State { get; }

        /// <summary>
        /// Gets the peering state from <see cref="State" /> as enumeration.
        /// </summary>
        PeeringState StateEnumeration { get; }

        /// <summary>
        /// Gets a value indicating whether the connection to this peer is currently established.
        /// </summary>
        bool Established { get; }

        /// <summary>
        /// Gets a value indicating whether this peers is currently disabled.
        /// </summary>
        bool Disabled { get; }
    }
}
