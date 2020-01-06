using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using SnmpSharpNet;
using tik4net;
using tik4net.Objects.Routing.Bgp;

namespace SnmpAbstraction
{
    /// <summary>
    /// Lazy-loading Container for the data of a single BGP peer.
    /// </summary>
    internal class LazyLoadingMikrotikBgpPeerInfo : LazyMtikApiQuerierResultBase, IBgpPeer
    {
        /// <summary>
        /// Regex to parse a Mikrotik timespan string (e.g. &quot;8w6d8h51m16s&quot;).
        /// </summary>
        private static readonly Regex MtikTimespanParseRegex = new Regex(@"((?<week>\d+)w)?((?<day>\d+)d)?((?<hour>\d+)h)?((?<minute>\d+)m)?((?<second>\d+)s)?", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// Characters that split lists in MTik responses.
        /// </summary>
        private static readonly char[] MtikListConcatChars = new char[]{ ' ', ',', ';' };

        /// <summary>
        /// Construct for a given peer. We can right-away extract the values. What is lazy here is that for some properties (e.g. Uptime)
        /// we trigger a re-query.
        /// </summary>
        /// <param name="address">The address of the device that we're querying.</param>
        /// <param name="tikConnection">The tik4Net connection to use for talking to the device.</param>
        /// <param name="bgpPeer">The BGP peer to construct for.</param>
        public LazyLoadingMikrotikBgpPeerInfo(IpAddress address, ITikConnection tikConnection, BgpPeer bgpPeer)
            : base(address, tikConnection)
        {
            if (bgpPeer is null)
            {
                throw new ArgumentNullException(nameof(bgpPeer), "bgpPeer is null when constructing a LazyLoadingMikrotikBgpPeerInfo");
            }

            this.SetValuesFromTikPeerContainer(bgpPeer);
        }

        /// <summary>
        /// Assigns the value to our properties from the given tik4net BgpPeer conatiner.
        /// </summary>
        /// <param name="bgpPeer">The tik4net BgpPeer container to take the params from.</param>
        private void SetValuesFromTikPeerContainer(BgpPeer bgpPeer)
        {
            this.Id = bgpPeer.Id;
            this.Name = bgpPeer.Name;
            this.Instance = bgpPeer.Instance;

            if (IPAddress.TryParse(bgpPeer.RemoteAddress, out IPAddress remoteAddress))
            {
                this.RemoteAddress = remoteAddress;
            }

            this.RemoteAs = bgpPeer.RemoteAs;
            this.NexthopChoice = bgpPeer.NexthopChoice;
            this.Multihop = bgpPeer.Multihop;
            this.RouteReflect = bgpPeer.RouteReflect;
            this.HoldTime = this.TryConvertToTimeSpanOrMinimum(bgpPeer.HoldTime);
            this.Ttl = bgpPeer.Ttl;
            this.AddressFamilies = this.TryParseFamilies(bgpPeer.AddressFamilies);
            this.DefaultOriginate = bgpPeer.DefaultOriginate;
            this.RemovePrivateAs = bgpPeer.RemovePrivateAs;
            this.AsOverride = bgpPeer.AsOverride;
            this.Passive = bgpPeer.Passive;
            this.UseBfd = bgpPeer.UseBfd;
            this.RemoteId = bgpPeer.RemoteId;

            if (IPAddress.TryParse(bgpPeer.LocalAddress, out IPAddress localAddress))
            {
                this.LocalAddress = localAddress;
            }

            this.Uptime = this.TryConvertToTimeSpanOrMinimum(bgpPeer.Uptime);
            this.PrefixCount = bgpPeer.PrefixCount;
            this.UpdatesSent = bgpPeer.UpdatesSent;
            this.UpdatesReceived = bgpPeer.UpdatesReceived;
            this.WithdrawnSent = bgpPeer.WithdrawnSent;
            this.WithdrawnReceived = bgpPeer.WithdrawnReceived;
            this.RemoteHoldTime = this.TryConvertToTimeSpanOrMinimum(bgpPeer.RemoteHoldTime);
            this.UsedHoldTime = this.TryConvertToTimeSpanOrMinimum(bgpPeer.UsedHoldTime);
            this.UsedKeepaliveTime = this.TryConvertToTimeSpanOrMinimum(bgpPeer.UsedKeepaliveTime);
            this.RefreshCapability = bgpPeer.RefreshCapability;
            this.As4Capability = bgpPeer.As4Capability;
            this.State = bgpPeer.State;
            this.StateEnumeration = bgpPeer.State.ToBgpStateEnumeration();
            this.Established = bgpPeer.Established;
            this.Disabled = bgpPeer.Disabled;
        }

        
        public string Id { get; private set; }

        /// <inheritdoc />
        public string Name { get; private set; }

        /// <inheritdoc />
        public string Instance { get; private set; }

        /// <inheritdoc />
        public IPAddress RemoteAddress { get; private set; }

        /// <inheritdoc />
        public long RemoteAs { get; private set; }

        /// <inheritdoc />
        public string NexthopChoice { get; private set; }

        /// <inheritdoc />
        public bool Multihop { get; private set; }

        /// <inheritdoc />
        public bool RouteReflect { get; private set; }

        /// <inheritdoc />
        public TimeSpan HoldTime { get; private set; }

        /// <inheritdoc />
        public string Ttl { get; private set; }

        /// <inheritdoc />
        public IEnumerable<AddressFamily> AddressFamilies { get; private set; }

        /// <inheritdoc />
        public string DefaultOriginate { get; private set; }

        /// <inheritdoc />
        public bool RemovePrivateAs { get; private set; }

        /// <inheritdoc />
        public bool AsOverride { get; private set; }

        /// <inheritdoc />
        public bool Passive { get; private set; }

        /// <inheritdoc />
        public bool UseBfd { get; private set; }

        /// <inheritdoc />
        public string RemoteId { get; private set; }

        /// <inheritdoc />
        public IPAddress LocalAddress { get; private set; }

        /// <inheritdoc />
        public TimeSpan Uptime { get; private set; }

        /// <inheritdoc />
        public long PrefixCount { get; private set; }

        /// <inheritdoc />
        public long UpdatesSent { get; private set; }

        /// <inheritdoc />
        public long UpdatesReceived { get; private set; }

        /// <inheritdoc />
        public long WithdrawnSent { get; private set; }

        /// <inheritdoc />
        public long WithdrawnReceived { get; private set; }

        /// <inheritdoc />
        public TimeSpan RemoteHoldTime { get; private set; }

        /// <inheritdoc />
        public TimeSpan UsedHoldTime { get; private set; }

        /// <inheritdoc />
        public TimeSpan UsedKeepaliveTime { get; private set; }

        /// <inheritdoc />
        public bool RefreshCapability { get; private set; }

        /// <inheritdoc />
        public bool As4Capability { get; private set; }

        /// <inheritdoc />
        public string State { get; private set; }

        /// <inheritdoc />
        public bool Established { get; private set; }

        /// <inheritdoc />
        public bool Disabled { get; private set; }

        /// <inheritdoc />
        public PeeringState StateEnumeration { get; private set; }

        /// <inheritdoc />
        public override void ForceEvaluateAll()
        {
            // NOP here - we have assigned from input for now - but later we might want to re-get the values (so uptime gets refreshed)
        }
 
        /// <summary>
        /// Tries to convert the MTik-formatted time (e.g. &quot;8w6d8h51m16s&quot;) to a TimeSpan object.
        /// </summary>
        /// <param name="mtikTimeString">The time span in the Mikrotik format.</param>
        /// <returns>A TimeSpan object with the value of the time string or <see cref="TimeSpan.MinValue" /> if not convertible.</returns>
        private TimeSpan TryConvertToTimeSpanOrMinimum(string mtikTimeString)
        {
            if (string.IsNullOrWhiteSpace(mtikTimeString))
            {
                return TimeSpan.MinValue;
            }

            var match = MtikTimespanParseRegex.Match(mtikTimeString);
            if (!match.Success)
            {
                return TimeSpan.MinValue;
            }

            var weekMatch = match.Groups["week"];
            var week = weekMatch.Success ? Convert.ToInt32(weekMatch.Value) : 0;

            var dayMatch = match.Groups["day"];
            var day = dayMatch.Success ? Convert.ToInt32(dayMatch.Value) : 0;

            var hourMatch = match.Groups["hour"];
            var hour = hourMatch.Success ? Convert.ToInt32(hourMatch.Value) : 0;

            var minuteMatch = match.Groups["minute"];
            var minute = minuteMatch.Success ? Convert.ToInt32(minuteMatch.Value) : 0;

            var secondMatch = match.Groups["second"];
            var second = secondMatch.Success ? Convert.ToInt32(secondMatch.Value) : 0;

            return new TimeSpan((week * 7) + day, hour, minute, second);
        }

        /// <summary>
        /// Parses the given string as an address family.
        /// </summary>
        /// <param name="addressFamilies">The string to parse (usually a comma-separated list of strings)</param>
        /// <returns>The parsed address families or Enumerable.Empty.</returns>
        private IEnumerable<AddressFamily> TryParseFamilies(string addressFamilies)
        {
            if (string.IsNullOrWhiteSpace(addressFamilies))
            {
                return Enumerable.Empty<AddressFamily>();
            }

            var splitFamilies = addressFamilies.Split(MtikListConcatChars, StringSplitOptions.RemoveEmptyEntries);
            List<AddressFamily> families = new List<AddressFamily>(splitFamilies.Length);
            foreach (string family in splitFamilies)
            {
                AddressFamily foundFamily;
                if (!Enum.TryParse<AddressFamily>(family, true, out foundFamily))
                {
                    switch(family.ToUpperInvariant())
                    {
                        case "IP":
                        case "IP4":
                        case "IPV4":
                            foundFamily = AddressFamily.InterNetwork;
                            break;

                        case "IP6":
                        case "IPV6":
                            foundFamily = AddressFamily.InterNetworkV6;
                            break;

                        default:
                            foundFamily = AddressFamily.Unknown;
                            break;
                    }
                }

                families.Add(foundFamily);
            }

            return families;
        }
   }
}