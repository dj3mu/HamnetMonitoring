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
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="queryDuration">The duration of the query.</param>
        protected HamnetSnmpQuerierResultBase(ISnmpLowerLayer lowerSnmpLayer, TimeSpan queryDuration)
        {
            if (lowerSnmpLayer == null)
            {
                throw new ArgumentNullException(nameof(lowerSnmpLayer), "The handle to the lower layer interface is null");
            }

            this.LowerSnmpLayer = lowerSnmpLayer;
            this.QueryDuration = queryDuration;
        }

        /// <summary>
        /// Construct for the given device address.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        protected HamnetSnmpQuerierResultBase(ISnmpLowerLayer lowerSnmpLayer)
            : this(lowerSnmpLayer, TimeSpan.Zero)
        {
        }

        /// <inheritdoc />
        public IpAddress DeviceAddress => this.LowerSnmpLayer.Address;

        /// <inheritdoc />
        public virtual TimeSpan QueryDuration { get; }

        /// <summary>
        /// Gets the communication layer in use.
        /// </summary>
        protected ISnmpLowerLayer LowerSnmpLayer { get; }

        /// <inheritdoc />
        public string ToConsoleString()
        {
            StringBuilder returnBuilder = new StringBuilder(128);
            returnBuilder.Append("Device ").Append(this.DeviceAddress).AppendLine(":");
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
