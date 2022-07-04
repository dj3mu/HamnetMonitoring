using System;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    internal class VolatileFetchingInterfaceDetail : IInterfaceDetail
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
#pragma warning disable IDE0052 // for future use
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ISnmpLowerLayer lowerLayer;
#pragma warning restore

        private readonly IInterfaceDetail underlyingInterfaceDetail;

        private readonly TimeSpan queryDurationBacking = TimeSpan.Zero;

        public VolatileFetchingInterfaceDetail(IInterfaceDetail underlyingInterfaceDetail, ISnmpLowerLayer lowerLayer)
        {
            this.underlyingInterfaceDetail = underlyingInterfaceDetail ?? throw new System.ArgumentNullException(nameof(underlyingInterfaceDetail));
            this.lowerLayer = lowerLayer ?? throw new System.ArgumentNullException(nameof(lowerLayer));
        }

        public int InterfaceId => this.underlyingInterfaceDetail.InterfaceId;

        public IanaInterfaceType InterfaceType => this.underlyingInterfaceDetail.InterfaceType;

        public string MacAddressString => this.underlyingInterfaceDetail.MacAddressString;

        public string InterfaceName => this.underlyingInterfaceDetail.InterfaceName;

        public IpAddress DeviceAddress => this.underlyingInterfaceDetail.DeviceAddress;

        public string DeviceModel => this.underlyingInterfaceDetail.DeviceModel;

        public TimeSpan QueryDuration => this.queryDurationBacking;

        public void ForceEvaluateAll()
        {
            // NOP here - the volatile values are supposed to be queried every time
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder returnBuilder = new StringBuilder(128);

            returnBuilder.Append("Interface #").Append(this.InterfaceId).Append(" (").Append(string.IsNullOrWhiteSpace(this.InterfaceName) ? "not available" : this.InterfaceName).AppendLine("):");
            returnBuilder.Append("  - Type: ").AppendLine(this.InterfaceType.ToString());
            returnBuilder.Append("  - MAC : ").Append(string.IsNullOrWhiteSpace(this.MacAddressString) ? "not available" : this.MacAddressString);

            return returnBuilder.ToString();
        }
    }
}