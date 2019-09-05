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
    /// /// </summary>
    internal class LazyLoadingMikroTikDeviceInterfaceDetails : LazyHamnetSnmpQuerierResultBase, IInterfaceDetails
    {
        /// <summary>
        /// The OID lookup table.
        /// </summary>
        private readonly DeviceSpecificOidLookup oidLookup;

        /// <summary>
        /// Backing field for the lazy access to the interface details.
        /// </summary>
        /// <remarks><c>null</c> means not initialized, empty list means no interfaces found.</remarks>
        private List<IInterfaceDetail> interfaceDetailsBacking = null;

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
        public LazyLoadingMikroTikDeviceInterfaceDetails(ISnmpLowerLayer lowerSnmpLayer, DeviceSpecificOidLookup oidLookup)
            : base(lowerSnmpLayer)
        {
            this.oidLookup = oidLookup;
        }

        /// <inheritdoc />
        public IReadOnlyList<IInterfaceDetail> Details
        {
            get
            {
                this.PopulateInterfaceDetails();

                return this.interfaceDetailsBacking;
            }
        }

        /// <inheritdoc />
        public override TimeSpan QueryDuration
        {
            get
            {
                if (this.interfaceDetailsBacking == null)
                {
                    return TimeSpan.Zero;
                }

                return this.interfaceDetailsBacking.Aggregate(this.localQueryDuration, (value, detail) => value += detail.QueryDuration);
            }
        }

        /// <inheritdoc />
        public override void ForceEvaluateAll()
        {
            this.PopulateInterfaceDetails();
            foreach (var item in this.interfaceDetailsBacking)
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
            StringBuilder returnBuilder = new StringBuilder((this.interfaceDetailsBacking?.Count ?? 1) * 128);

            returnBuilder.Append("Interface Details:");

            if (!this.interfaceDetailsQueried)
            {
                returnBuilder.Append(" Not yet queried");
            }
            else
            {
                if (this.interfaceDetailsBacking == null)
                {
                    returnBuilder.Append(" Not yet retrieved");
                }
                else
                {
                    foreach (var item in this.interfaceDetailsBacking)
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
        /// Populates the interface details list.
        /// </summary>
        private void PopulateInterfaceDetails()
        {
            if (this.interfaceDetailsQueried)
            {
                return;
            }

            var valueToQuery = RetrievableValuesEnum.InterfaceIdWalkRoot;
            DeviceSpecificOid interfaceIdRootOid;
            if (!this.oidLookup.TryGetValue(valueToQuery, out interfaceIdRootOid))
            {
                this.interfaceDetailsBacking = null;
                this.interfaceDetailsQueried = true;
                return;
            }

            Stopwatch durationWatch = Stopwatch.StartNew();

            var interfaceVbs = this.LowerSnmpLayer.DoWalk(interfaceIdRootOid.Oid, 0);

            durationWatch.Stop();

            this.localQueryDuration = durationWatch.Elapsed;

            this.interfaceDetailsBacking = new List<IInterfaceDetail>();

            foreach (Vb item in interfaceVbs)
            {
                this.interfaceDetailsBacking.Add(new LazyLoadingMikroTikInterfaceDetail(this.LowerSnmpLayer, this.oidLookup, item.Value.ToInt()));
            }

            this.interfaceDetailsQueried = true;
        }
    }
}
