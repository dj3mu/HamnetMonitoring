using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// A simple container for storing a serializable version of the wireless peer infos
    /// </summary>
    /// [JsonObject(MemberSerialization.OptOut, ItemRequired = Required.Always)]
    [JsonObject(MemberSerialization.OptOut, ItemRequired = Required.Always)]
    internal class SerializableWirelessPeerInfos : IWirelessPeerInfos
    {
        [JsonProperty("Details")]
        private List<IWirelessPeerInfo> peerInfosBacking;

        /// <summary>
        /// Construct from IWirelessPeerInfos.
        /// </summary>
        public SerializableWirelessPeerInfos(IWirelessPeerInfos inputInfos)
        {
            if (inputInfos is null)
            {
                throw new ArgumentNullException(nameof(inputInfos), "The IWirelessPeerInfos to make serializable is null");
            }

            this.peerInfosBacking = inputInfos.Details.Select(p => new SerializableWirelessPeerInfo(p) as IWirelessPeerInfo).ToList();
        }

        /// <summary>
        /// Prevent default-construction from outside.
        /// </summary>
        internal SerializableWirelessPeerInfos()
        {
        }

        /// <inheritdoc />
        [JsonIgnore]
        public IReadOnlyList<IWirelessPeerInfo> Details
        {
            get
            {
                return this.peerInfosBacking;
            }

            set
            {
                this.peerInfosBacking = value as List<IWirelessPeerInfo> ?? value.ToList();
            }
        }

        /// <inheritdoc />
        public IpAddress DeviceAddress { get; set; }

        /// <inheritdoc />
        public string DeviceModel { get; set; }

        private readonly TimeSpan queryDuration = TimeSpan.Zero;

        /// <inheritdoc />
        public TimeSpan GetQueryDuration()
        {
            return queryDuration;
        }

        public string ToConsoleString()
        {
            StringBuilder returnBuilder = new StringBuilder((this.peerInfosBacking?.Count ?? 1) * 128);

            returnBuilder.Append("Peer Infos:");

            if (this.peerInfosBacking == null)
            {
                returnBuilder.Append(" Not yet retrieved");
            }
            else
            {
                foreach (var item in this.peerInfosBacking)
                {
                    returnBuilder.AppendLine().AppendLine(SnmpAbstraction.IndentLines(item.ToConsoleString()));
                }
            }

            return returnBuilder.ToString();
        }

        public void ForceEvaluateAll()
        {
            // NOP here - we're not lazy
        }

        /// <inheritdoc />
        public IEnumerator<IWirelessPeerInfo> GetEnumerator()
        {
            return this.peerInfosBacking.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.peerInfosBacking.GetEnumerator();
        }
    }
}