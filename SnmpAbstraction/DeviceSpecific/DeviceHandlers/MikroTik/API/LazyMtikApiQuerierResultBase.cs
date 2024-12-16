using System;
using SnmpSharpNet;
using tik4net;

namespace SnmpAbstraction
{
    /// <summary>
    /// Base class for all <see cref="IHamnetSnmpQuerierResult" />.
    /// </summary>
    internal abstract class LazyMtikApiQuerierResultBase : HamnetSnmpQuerierResultBase, ILazyEvaluated
    {
        /// <summary>
        /// Construct for the given device address and query duration.
        /// </summary>
        /// <param name="address">The address of the device that we're querying.</param>
        /// <param name="tikConnection">The communication layer to use for talking to the device.</param>
        /// <param name="queryDuration">The duration of the query.</param>
        protected LazyMtikApiQuerierResultBase(IpAddress address, ITikConnection tikConnection, TimeSpan queryDuration)
            : this(address, tikConnection, queryDuration, "Model currently unknown for MTik API")
        {
        }

        /// <summary>
        /// Construct for the given device address and query duration.
        /// </summary>
        /// <param name="address">The address of the device that we're querying.</param>
        /// <param name="tikConnection">The communication layer to use for talking to the device.</param>
        /// <param name="queryDuration">The duration of the query.</param>
        /// <param name="deviceModel">The model and version of the device.</param>
        protected LazyMtikApiQuerierResultBase(IpAddress address, ITikConnection tikConnection, TimeSpan queryDuration, string deviceModel)
            : base(address, deviceModel, queryDuration)
        {
            this.TikConnection = tikConnection ?? throw new ArgumentNullException(nameof(tikConnection), "The handle to the Mikrotik API interface is null");
        }

        /// <summary>
        /// Construct for the given device address.
        /// </summary>
        /// <param name="address">The address of the device that we're querying.</param>
        /// <param name="tikConnection">The communication layer to use for talking to the device.</param>
        protected LazyMtikApiQuerierResultBase(IpAddress address, ITikConnection tikConnection)
            : this(address, tikConnection, TimeSpan.Zero)
        {
        }

        /// <summary>
        /// Gets the communication layer in use.
        /// </summary>
        protected ITikConnection TikConnection { get; }

        /// <inheritdoc />
        public abstract void ForceEvaluateAll();
    }
}
