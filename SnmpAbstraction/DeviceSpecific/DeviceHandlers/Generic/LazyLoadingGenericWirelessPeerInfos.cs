using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SnmpAbstraction
{
    /// <summary>
    /// Worker class with generic parts for lazy-loading wireless peer infos.
    /// /// </summary>
    internal abstract class LazyLoadingGenericWirelessPeerInfos : LazyHamnetSnmpQuerierResultBase, IWirelessPeerInfos
    {
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Field indicating whether the peer infos have been queried.<br/>
        /// The field is necessary as, by interface definition, null can be a valid
        /// return value if peer infos are not available.
        /// </summary>
        private bool peerInfosQueried = false;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        protected LazyLoadingGenericWirelessPeerInfos(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup)
            : base(lowerSnmpLayer)
        {
            this.OidLookup = oidLookup;
        }

        /// <inheritdoc />
        public IReadOnlyList<IWirelessPeerInfo> Details
        {
            get
            {
                this.PopulatePeerInfos();

                return this.PeerInfosBacking;
            }
        }

        /// <inheritdoc />
        public override void ForceEvaluateAll()
        {
            this.PopulatePeerInfos();
            foreach (var item in this.PeerInfosBacking)
            {
                item.ForceEvaluateAll();
            }
        }

        /// <inheritdoc />
        public IEnumerator<IWirelessPeerInfo> GetEnumerator()
        {
            return this.Details.GetEnumerator();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder returnBuilder = new StringBuilder((this.PeerInfosBacking?.Count ?? 1) * 128);

            returnBuilder.Append("Peer Infos:");

            if (!this.peerInfosQueried)
            {
                returnBuilder.Append(" Not yet queried");
            }
            else
            {
                if (this.PeerInfosBacking == null)
                {
                    returnBuilder.Append(" Not yet retrieved");
                }
                else
                {
                    foreach (var item in this.PeerInfosBacking)
                    {
                        returnBuilder.AppendLine().AppendLine(SnmpAbstraction.IndentLines(item.ToString()));
                    }
                }
            }

            return returnBuilder.ToString();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Details.GetEnumerator();
        }

        /// <summary>
        /// Gets the backing field for deriving classes to populate it.
        /// </summary>
        protected List<IWirelessPeerInfo> PeerInfosBacking { get; } = new List<IWirelessPeerInfo>();

        /// <summary>
        /// Gets the OID lookup table.
        /// </summary>
        protected IDeviceSpecificOidLookup OidLookup { get; private set; }

        /// <summary>
        /// Populates the interface details list.
        /// </summary>
        private void PopulatePeerInfos()
        {
            if (this.peerInfosQueried)
            {
                return;
            }

            this.peerInfosQueried = this.RetrievePeerInfo();
        }

        /// <summary>
        /// To be implemented by deriving classes in order to actually populate the peer info list.
        /// </summary>
        protected abstract bool RetrievePeerInfo();

        /// <summary>
        /// Check if the interface of the given ID is an access point or a client.
        /// </summary>
        /// <param name="interfaceId">The interface ID to check.</param>
        /// <returns><c>true</c> if the interface is an access point.</returns>
        protected abstract bool? CheckIsAccessPoint(int interfaceId);
    }
}
