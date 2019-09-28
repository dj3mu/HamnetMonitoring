using System;
using System.Text;
using Newtonsoft.Json;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Serializable container for interface details.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut, ItemRequired = Required.Always)]
    internal class SerializableInterfaceDetail : IInterfaceDetail
    {
        /// <summary>
        /// Default-construct
        /// </summary>
        public SerializableInterfaceDetail()
        {
        }

        /// <summary>
        /// Copy-construct.
        /// </summary>
        /// <param name="source">The source to construct from.</param>
        public SerializableInterfaceDetail(IInterfaceDetail source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source), "Source IInterfaceDetail is null");
            }

            this.InterfaceId = source.InterfaceId;
            this.InterfaceType = source.InterfaceType;
            this.MacAddressString = source.MacAddressString;
            this.InterfaceName = source.InterfaceName;
            this.DeviceAddress = source.DeviceAddress;
            this.DeviceModel = source.DeviceModel;

            // we're intentionally not setting QueryDuration.
        }

        /// <inheritdoc />
        public int InterfaceId { get; set; }

        /// <inheritdoc />
        public IanaInterfaceType InterfaceType { get; set; }

        /// <inheritdoc />
        public string MacAddressString { get; set; }

        /// <inheritdoc />
        public string InterfaceName { get; set; }

        /// <inheritdoc />
        public IpAddress DeviceAddress { get; set; }

        /// <inheritdoc />
        public string DeviceModel { get; set; }

        private TimeSpan queryDuration = TimeSpan.Zero;

        /// <inheritdoc />
        public TimeSpan QueryDuration => queryDuration;

        /// <inheritdoc />
        public void SetQueryDuration(TimeSpan value)
        {
            queryDuration = value;
        }

        /// <inheritdoc />
        public void ForceEvaluateAll()
        {
            // NOP - we're not lazy
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder returnBuilder = new StringBuilder(128);

            returnBuilder.Append("Interface #").Append(this.InterfaceId).Append(" (").Append(this.InterfaceName).AppendLine("):");
            returnBuilder.Append("  - Type: ").AppendLine(this.InterfaceType.ToString());
            returnBuilder.Append("  - MAC : ").Append(this.MacAddressString?.ToString());

            return returnBuilder.ToString();
        }
   }
}