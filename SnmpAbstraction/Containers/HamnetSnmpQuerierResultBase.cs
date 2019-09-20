using System;
using System.IO;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Base class for all <see cref="IHamnetSnmpQuerierResult" />.
    /// </summary>
    internal abstract class HamnetSnmpQuerierResultBase : IHamnetSnmpQuerierResult
    {
        /// <summary>
        /// Construct for the given device address and query duration.
        /// </summary>
        /// <param name="address">The IP address of the device that produced this result.</param>
        /// <param name="deviceModel">The model and version of the device.</param>
        /// <param name="queryDuration">The duration of the query.</param>
        protected HamnetSnmpQuerierResultBase(IpAddress address, string deviceModel, TimeSpan queryDuration)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address), "The IP address of the device producing this result is null");
            }

            this.DeviceAddress = address;
            this.QueryDuration = queryDuration;
            this.DeviceModel = deviceModel;
        }

        /// <inheritdoc />
        public IpAddress DeviceAddress { get; }

        /// <inheritdoc />
        public virtual TimeSpan QueryDuration { get; }

        /// <inheritdoc />
        public virtual string DeviceModel { get; }

        /// <inheritdoc />
        public string ToConsoleString()
        {
            StringBuilder returnBuilder = new StringBuilder(128);
            returnBuilder.Append("Device ").Append(this.DeviceAddress).Append(" (").Append(this.DeviceModel).AppendLine("):");
            returnBuilder.AppendLine(SnmpAbstraction.IndentLines(this.ToTextString()));
            returnBuilder.Append(SnmpAbstraction.IndentLines("--> Query took ")).Append(this.QueryDuration.TotalMilliseconds).Append(" ms");

            return returnBuilder.ToString();
        }
        
        /// <summary>
        /// Prints the result body to the given <see cref="TextWriter" /> using
        /// human-readable formatting.
        /// </summary>
        /// <returns>A string with the human readable data.</returns>
        public abstract string ToTextString();
        
        /// <inheritdoc />
        public override string ToString()
        {
            return this.ToConsoleString();
        }
    }
}
