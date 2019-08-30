using System;
using System.Net;
using System.Runtime.CompilerServices;
using SnmpSharpNet;

[assembly: InternalsVisibleTo("SnmpAbstractionTests")]
namespace SnmpAbstraction
{
    /// <summary>
    /// Implementation of the lower layer SNMP parts.<br/>
    /// Mainly encapsulates the necessary 
    /// /// </summary>
    internal class SnmpLowerLayer : ISnmpLowerLayer, IDisposable
    {
        /// <summary>
        /// The target that we talk to.
        /// </summary>
        protected UdpTarget Target { get; private set; } = null;

        /// <summary>
        /// The query parameters. Initialized by call to <see cref="InitializeTarget" />
        /// </summary>
        protected AgentParameters QueryParameters { get; private set; } = null;

        /// <summary>
        /// Detect redundant calls to Dispose
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Cached System Data object.
        /// </summary>
        private LazyLoadingDeviceSystemData cachedSystemData;

        /// <summary>
        /// Construct from address and use default options.
        /// </summary>
        /// <param name="address">The address to talk to.</param>
        public SnmpLowerLayer(IpAddress address)
            : this(address, QuerierOptions.Default)
        {
        }

        /// <summary>
        /// Construct from address and options.
        /// </summary>
        /// <param name="address">The address to talk to.</param>
        /// <param name="options">The options to use.</param>
        public SnmpLowerLayer(IpAddress address, IQuerierOptions options)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address), "address to talk to is null");
            }

            this.Address = address;
            this.Options = options ?? QuerierOptions.Default;
        }

        /// <summary>
        /// Gets the address that this lower layer talks to.
        /// </summary>
        public IpAddress Address { get; }

        /// <summary>
        /// Gets the options that are used by this lower layer.
        /// </summary>
        public IQuerierOptions Options { get; }

        /// <inheritdoc />
        public IDeviceSystemData SystemData
        {
            get
            {
                if (this.cachedSystemData == null)
                {
                    this.InitSystemData();
                }

                return this.cachedSystemData;
            }
        }

        /// <inheritdoc />
        public VbCollection Query(params Oid[] oids)
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(SnmpLowerLayer), "The object is already disposed off. Cannot execute any more commands on it.");
            }

            if (oids == null)
            {
                throw new ArgumentNullException(nameof(oids), "The OIDs to query is null");
            }

            if (oids.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(oids), oids.Length, "The list OIDs to query must does not contain any element");
            }

            this.InitializeTarget();
            
            SnmpPacket response = this.SendRequest(oids);

            if (response == null)
            {
                throw new HamnetSnmpException($"Query for {oids.Length} OIDs from {this.Address} produced 'null' response");
            }

            // ErrorStatus other then 0 is an error returned by the Agent - see SnmpConstants for error definitions
            if (response.Pdu.ErrorStatus != 0)
            {
                // agent reported an error with the request
                throw new HamnetSnmpException($"Error in SNMP reply. Error {response.Pdu.ErrorStatus} index {response.Pdu.ErrorIndex}");
            }

            return response.Pdu.VbList;
        }

        private SnmpPacket SendRequest(Oid[] oids)
        {
            Pdu pdu = new Pdu(PduType.Get);
            foreach (Oid item in oids)
            {
                pdu.VbList.Add(item);
            }

            return Target.Request(pdu, this.QueryParameters);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Disposes the instance.
        /// </summary>
        /// <param name="disposing"><c>true</c> if called by <see cref="Dispose" /> method.<br/>
        /// <c>false</c> if called by finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    if (this.Target != null)
                    {
                        this.Target.Dispose();
                        this.Target = null;
                    }
                }

                // Free unmanaged resources here and set large fields to null.

                this.disposedValue = true;
            }
        }

        /// <summary>
        /// Implements lazy initialization of the SnmpSharpNet target instance.<br/>
        /// </summary>
        private void InitializeTarget()
        {
            if (this.Target != null)
            {
                return;
            }

            OctetString community = new OctetString(this.Options.Community);
            this.QueryParameters = new AgentParameters(community)
            {
                Version = this.Options.ProtocolVersion
            };

            this.Target = new UdpTarget((IPAddress)this.Address, this.Options.Port, Convert.ToInt32(this.Options.Timeout.TotalMilliseconds), this.Options.Retries);
        }
        
        /// <summary>
        /// Initializes the device system data container.
        /// </summary>
        private void InitSystemData()
        {
            throw new NotImplementedException();
        }
    }
}
