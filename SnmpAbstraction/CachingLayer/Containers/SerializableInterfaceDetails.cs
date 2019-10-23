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
    /// A simple container for storing a serializable version of the interface details
    /// </summary>
    [JsonObject(MemberSerialization.OptOut, ItemRequired = Required.Always)]
    internal class SerializableInterfaceDetails : IInterfaceDetails
    {
        [JsonProperty("Details")]
        private List<IInterfaceDetail> interfaceDetailsBacking;

        /// <summary>
        /// Construct from IWirelessPeerInfos.
        /// </summary>
        public SerializableInterfaceDetails(IInterfaceDetails inputInfos)
        {
            if (inputInfos is null)
            {
                throw new ArgumentNullException(nameof(inputInfos), "The IInterfaceDetails to make serializable is null");
            }

            this.interfaceDetailsBacking = inputInfos.Details.Select(p => new SerializableInterfaceDetail(p) as IInterfaceDetail).ToList();
            this.DeviceModel = inputInfos.DeviceModel;
            this.DeviceAddress = inputInfos.DeviceAddress;
        }

        /// <summary>
        /// Prevent default-construction from outside.
        /// </summary>
        internal SerializableInterfaceDetails()
        {
        }

        /// <inheritdoc />
        [JsonIgnore]
        public IReadOnlyList<IInterfaceDetail> Details
        {
            get
            {
                return this.interfaceDetailsBacking;
            }

            set
            {
                this.interfaceDetailsBacking = value as List<IInterfaceDetail> ?? value.ToList();
            }
        }

        /// <inheritdoc />
        public IpAddress DeviceAddress { get; set; }

        /// <inheritdoc />
        public string DeviceModel { get; set; }

        /// <inheritdoc />
        public TimeSpan QueryDuration { get; set; } = TimeSpan.Zero;

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder returnBuilder = new StringBuilder((this.interfaceDetailsBacking?.Count ?? 1) * 128);

            returnBuilder.Append("Interface Details:");

            if (this.interfaceDetailsBacking == null)
            {
                returnBuilder.Append(" Not yet retrieved");
            }
            else
            {
                foreach (var item in this.interfaceDetailsBacking)
                {
                    returnBuilder.AppendLine().AppendLine(SnmpAbstraction.IndentLines(item.ToString()));
                }
            }

            return returnBuilder.ToString();
        }

        /// <inheritdoc />
        public void ForceEvaluateAll()
        {
            // NOP here - we're not lazy
        }

        /// <inheritdoc />
        public IEnumerator<IInterfaceDetail> GetEnumerator()
        {
            return this.interfaceDetailsBacking.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.interfaceDetailsBacking.GetEnumerator();
        }
    }
}