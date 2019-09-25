using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Worker class for lazy-loading interface details.
    /// </summary>
    /// <remarks>
    /// This class is intentionally not abstract. Its default implementation works using IANA-default
    /// MIB tree and hence the class can work with all kinds of devices that stick to the IANA standard.
    /// </remarks>
    internal class LazyLoadingGenericInterfaceDetails : LazyHamnetSnmpQuerierResultBase, IInterfaceDetails
    {
        /// <summary>
        /// Field indicating whether the interface details has been queried.<br/>
        /// The field is necessary as, by interface definition, null can be a valid
        /// return value if interface details is not available.
        /// </summary>
        private bool interfaceDetailsQueried = false;

        /// <summary>
        /// Field to sum up the query duration on each "Populate" call.
        /// </summary>
        private TimeSpan localQueryDuration = TimeSpan.Zero;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        public LazyLoadingGenericInterfaceDetails(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup)
            : base(lowerSnmpLayer)
        {
            this.OidLookup = oidLookup;
        }

        /// <inheritdoc />
        public IReadOnlyList<IInterfaceDetail> Details
        {
            get
            {
                this.PopulateInterfaceDetails();

                return this.InterfaceDetailsBacking;
            }
        }

        /// <inheritdoc />
        public override TimeSpan GetQueryDuration()
        {
            if (this.InterfaceDetailsBacking == null)
            {
                return TimeSpan.Zero;
            }

            return this.InterfaceDetailsBacking.Aggregate(this.localQueryDuration, (value, detail) => value += detail.GetQueryDuration());
        }

        /// <summary>
        /// Gets access to the list of interface details for deriving classes.
        /// </summary>
        protected List<IInterfaceDetail> InterfaceDetailsBacking { get; } = new List<IInterfaceDetail>();

        /// <summary>
        /// Gets the OID lookup table into the deriving classes.
        /// </summary>
        protected IDeviceSpecificOidLookup OidLookup { get; }

        /// <inheritdoc />
        public override void ForceEvaluateAll()
        {
            this.PopulateInterfaceDetails();

            if (this.InterfaceDetailsBacking == null)
            {
                 return;
            }
            
            foreach (var item in this.InterfaceDetailsBacking)
            {
                item.ForceEvaluateAll();
            }
        }

        /// <inheritdoc />
        public IEnumerator<IInterfaceDetail> GetEnumerator()
        {
            return this.Details.GetEnumerator();
        }

        /// <inheritdoc />
        public override string ToTextString()
        {
            StringBuilder returnBuilder = new StringBuilder((this.InterfaceDetailsBacking?.Count ?? 1) * 128);

            returnBuilder.Append("Interface Details:");

            if (!this.interfaceDetailsQueried)
            {
                returnBuilder.Append(" Not yet queried");
            }
            else
            {
                if (this.InterfaceDetailsBacking == null)
                {
                    returnBuilder.Append(" Not yet retrieved");
                }
                else
                {
                    foreach (var item in this.InterfaceDetailsBacking)
                    {
                        returnBuilder.AppendLine().AppendLine(SnmpAbstraction.IndentLines(item.ToConsoleString()));
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
        /// Retrieve and fill the interface details list (can be overridden by deriving classes).
        /// </summary>
        /// <remarks>Alternatively inheriting classes can choose to keep using this method and only override the instantiation
        /// by implementing <see cref="InstantiateInterfaceDetail" /></remarks>.
        protected virtual bool RetrieveInterfaceDetails()
        {
            this.InterfaceDetailsBacking.Clear();

            var valueToQuery = RetrievableValuesEnum.InterfaceIdWalkRoot;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.OidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                return true;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfaceVbs = this.LowerSnmpLayer.DoWalk(interfaceIdRootOid.Oid);

            durationWatch.Stop();

            this.localQueryDuration = durationWatch.Elapsed;

            foreach (Vb item in interfaceVbs)
            {
                this.InterfaceDetailsBacking.Add(this.InstantiateInterfaceDetail(this.LowerSnmpLayer, this.OidLookup, item.Value.ToInt()));
            }

            return true;
        }

        /// <summary>
        /// Allow overriding of instanatiation of the actual interface detail implementation.
        /// </summary>
        /// <param name="lowerSnmpLayer">The communication layer to use for talking to the device.</param>
        /// <param name="oidLookup">The OID lookup table for the device.</param>
        /// <param name="interfaceId">The interface ID.</param>
        /// <returns>The instantiated interface details container.</returns>
        /// <remarks>Alternatively inheriting classes can choose to right away override the complete interface list retrieval
        /// by implementing <see cref="RetrieveInterfaceDetails" /></remarks>.
        protected virtual IInterfaceDetail InstantiateInterfaceDetail(ISnmpLowerLayer lowerSnmpLayer, IDeviceSpecificOidLookup oidLookup, int interfaceId)
        {
            return new LazyLoadingGenericInterfaceDetail(this.LowerSnmpLayer, this.OidLookup, interfaceId);
        }

        /// <summary>
        /// Populates the interface details list.
        /// </summary>
        private void PopulateInterfaceDetails()
        {
            if (this.interfaceDetailsQueried)
            {
                return;
            }

            this.interfaceDetailsQueried = this.RetrieveInterfaceDetails();
        }
    }
}
