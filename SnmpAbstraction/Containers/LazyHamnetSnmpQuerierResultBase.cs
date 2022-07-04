using System;

namespace SnmpAbstraction
{
    /// <summary>
    /// Base class for all <see cref="IHamnetSnmpQuerierResult" />.
    /// </summary>
    internal abstract class LazyHamnetSnmpQuerierResultBase : HamnetSnmpQuerierResultBase, ILazyEvaluated
    {
        /// <summary>
        /// Construct for the given device address and query duration.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="queryDuration">The duration of the query.</param>
        protected LazyHamnetSnmpQuerierResultBase(ISnmpLowerLayer lowerSnmpLayer, TimeSpan queryDuration)
            : this(lowerSnmpLayer, queryDuration, lowerSnmpLayer.SystemData?.DeviceModel)
        {
            this.LowerSnmpLayer = lowerSnmpLayer ?? throw new ArgumentNullException(nameof(lowerSnmpLayer), "The handle to the lower layer interface is null");
        }

        /// <summary>
        /// Construct for the given device address and query duration.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="queryDuration">The duration of the query.</param>
        /// <param name="deviceModel">The model and version of the device.</param>
        protected LazyHamnetSnmpQuerierResultBase(ISnmpLowerLayer lowerSnmpLayer, TimeSpan queryDuration, string deviceModel)
            : base(lowerSnmpLayer.Address, deviceModel, queryDuration)
        {
            this.LowerSnmpLayer = lowerSnmpLayer ?? throw new ArgumentNullException(nameof(lowerSnmpLayer), "The handle to the lower layer interface is null");
        }

        /// <summary>
        /// Construct for the given device address.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        protected LazyHamnetSnmpQuerierResultBase(ISnmpLowerLayer lowerSnmpLayer)
            : this(lowerSnmpLayer, TimeSpan.Zero)
        {
        }

        /// <summary>
        /// Gets the communication layer in use.
        /// </summary>
        protected ISnmpLowerLayer LowerSnmpLayer { get; }

        /// <inheritdoc />
        public abstract void ForceEvaluateAll();
    }
}
