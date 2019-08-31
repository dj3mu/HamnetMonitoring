using System;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Implementation of <see cref="IDeviceSystemData" /> that does as
    /// lazy as possible loading of the property (i.e. on first use).
    /// </summary>
    internal class LazyLoadingDeviceSystemData : IDeviceSystemData
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The system description.
        /// </summary>
        private string systemDescrition;

        /// <summary>
        /// Field indicating whether the system description has been queried.<br/>
        /// The field is necessary as, by interface definition, null can be a valid
        /// return value if description is not available.
        /// </summary>
        private bool systemDescriptionQueried = false;

        /// <summary>
        /// Construct taking the lower layer to use for lazy-querying the data.
        /// </summary>
        /// <param name="lowerSnmpLayer"></param>
        public LazyLoadingDeviceSystemData(ISnmpLowerLayer lowerSnmpLayer)
        {
            this.LowerSnmpLayer = lowerSnmpLayer;
        }

        /// <summary>
        /// Gets the communication layer in use.
        /// </summary>
        public ISnmpLowerLayer LowerSnmpLayer { get; }

        /// <inheritdoc />
        public string Description
        {
            get
            {
                if (!this.systemDescriptionQueried)
                {
                    this.systemDescrition = this.LowerSnmpLayer.QueryAsString(new Oid(".1.3.6.1.2.1.1.1.0"), "system description");
                    this.systemDescriptionQueried = true;
                }

                return this.systemDescrition;
            }
        }

        /// <inheritdoc />
        public Oid EnterpriseObjectId => throw new NotImplementedException();

        /// <inheritdoc />
        public string Contact => throw new NotImplementedException();

        /// <inheritdoc />
        public string Location => throw new NotImplementedException();

        /// <inheritdoc />
        public string Name => throw new NotImplementedException();

        /// <inheritdoc />
        public TimeSpan? Uptime => throw new NotImplementedException();

        /// <inheritdoc />
        public string Model => throw new NotImplementedException();

        /// <inheritdoc />
        public string SoftwareVersionString => throw new NotImplementedException();
    }
}
