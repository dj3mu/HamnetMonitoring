using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Implementation of the lower layer SNMP parts.<br/>
    /// Mainly encapsulates the necessary 
    /// /// </summary>
    internal class SnmpLowerLayer : ISnmpLowerLayer, IDisposable
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
            this.Address = address ?? throw new ArgumentNullException(nameof(address), "address to talk to is null");
            this.Options = options ?? QuerierOptions.Default;
        }

        /// <inheritdoc />
        public IpAddress Address { get; }

        /// <inheritdoc />
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
        public VbCollection DoWalk(Oid interfaceIdWalkRootOid, int depth)
        {
            this.InitializeTarget();

            if (this.Options.ProtocolVersion == SnmpVersion.Ver1)
            {
                return this.DoWalkGetNext(interfaceIdWalkRootOid, depth);
            }
            else if ((this.Options.ProtocolVersion == SnmpVersion.Ver2) || (this.Options.ProtocolVersion == SnmpVersion.Ver3))
            {
                return this.DoWalkBulk(interfaceIdWalkRootOid, depth);
            }
            else
            {
                throw new InvalidOperationException($"Don't know how to do an SNMP walk for SNMP protocol version '{this.Options.ProtocolVersion}'");
            }
        }

        /// <inheritdoc />
        public VbCollection Query(Oid firstOid, params Oid[] oids)
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(SnmpLowerLayer), "The object is already disposed off. Cannot execute any more commands on it.");
            }

            if (firstOid == null)
            {
                throw new ArgumentNullException(nameof(firstOid), "firstOid to query is null - but at least one OID must be provided");
            }

            if (oids == null)
            {
                throw new ArgumentNullException(nameof(oids), "The OIDs to query is null");
            }

            this.InitializeTarget();
            
            SnmpPacket response = this.SendRequest(new Oid[] { firstOid }.Concat(oids));

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

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Disposes the instance.
        /// </summary>
        /// <param name="disposing"><c>true</c> if called by <see cref="SnmpLowerLayer.Dispose()" /> method.<br/>
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
        /// Sends an SNMP request for the given OIDs.
        /// </summary>
        /// <param name="oids">The OIDs to request via SNMP.</param>
        /// <returns>The response SnmpPacket with the data.</returns>
        private SnmpPacket SendRequest(IEnumerable<Oid> oids)
        {
            Pdu pdu = new Pdu(PduType.Get);
            foreach (Oid item in oids)
            {
                pdu.VbList.Add(item);
            }

            return this.Target.Request(pdu, this.QueryParameters);
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
            this.cachedSystemData = new LazyLoadingDeviceSystemData(this);
        }

        /// <summary>
        /// Performs SNMP walk using Ver 2c GetBulk operation.
        /// </summary>
        /// <param name="interfaceIdWalkRootOid">The retrievable value to start walking at. The actual OID is resolved from the device database.</param>
        /// <param name="depth">The recursion depth. A value of 0 means not to recurse at all and just return the direct children.</param>
        /// <returns>A <see cref="VbCollection" /> with the received data.</returns>
        /// <remarks>Derived from example code at <see href="http://www.snmpsharpnet.com/?page_id=108" />.</remarks>
        private VbCollection DoWalkBulk(Oid interfaceIdWalkRootOid, int depth)
        {
            // This Oid represents last Oid returned by the SNMP agent
            Oid lastOid = (Oid)interfaceIdWalkRootOid.Clone();
 
            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.GetBulk);
 
            // In this example, set NonRepeaters value to 0
            pdu.NonRepeaters = this.Options.Ver2cMaximumValuesPerRequest;

            // MaxRepetitions tells the agent how many Oid/Value pairs to return in the response.
            pdu.MaxRepetitions = this.Options.Ver2cMaximumRequests;
 
            VbCollection returnCollection = new VbCollection();

            // Loop through results
            while (lastOid != null)
            {
                // When Pdu class is first constructed, RequestId is set to 0
                // and during encoding id will be set to the random value
                // for subsequent requests, id will be set to a value that
                // needs to be incremented to have unique request ids for each
                // packet
                if (pdu.RequestId != 0)
                {
                    pdu.RequestId += 1;
                }

                // Clear Oids from the Pdu class.
                pdu.VbList.Clear();

                // Initialize request PDU with the last retrieved Oid
                pdu.VbList.Add(lastOid);

                // Make SNMP request
                SnmpV2Packet result = (SnmpV2Packet)this.Target.Request(pdu, this.QueryParameters);

                // You should catch exceptions in the Request if using in real application.
                // [DJ3MU] : Yeah - cool idea - but I still wouldn't know what else to do with them.
                //           So for now, let fly them out to our caller.
 
                // If result is null then agent didn't reply or we couldn't parse the reply.
                if (result != null)
                {
                    // ErrorStatus other then 0 is an error returned by 
                    // the Agent - see SnmpConstants for error definitions
                    if (result.Pdu.ErrorStatus != 0)
                    {
                        // agent reported an error with the request
                        log.Error($"Error in SNMP reply. Error {result.Pdu.ErrorStatus} index {result.Pdu.ErrorIndex}");
                        lastOid = null;
                        break;
                    }
                    else
                    {
                        // Walk through returned variable bindings
                        foreach (Vb v in result.Pdu.VbList)
                        {
                            // Check that retrieved Oid is "child" of the root OID
                            if (interfaceIdWalkRootOid.IsRootOf(v.Oid))
                            {
                                returnCollection.Add(v);

                                if (v.Value.Type == SnmpConstants.SMI_ENDOFMIBVIEW)
                                {
                                    lastOid = null;
                                }
                                else
                                {
                                    lastOid = v.Oid;
                                }
                            }
                            else
                            {
                                // we have reached the end of the requested
                                // MIB tree. Set lastOid to null and exit loop
                                lastOid = null;
                            }
                        }
                    }
                }
                else
                {
                    throw new HamnetSnmpException($"No response received from SNMP agent for device '{this.Address}'");
                }
            }
            
            return returnCollection;
        }

        /// <summary>
        /// Performs SNMP walk using Ver 1 GetNext operation.
        /// </summary>
        /// <param name="interfaceIdWalkRootOid">The retrievable value to start walking at. The actual OID is resolved from the device database.</param>
        /// <param name="depth">The recursion depth. A value of 0 means not to recurse at all and just return the direct children.</param>
        /// <returns>A <see cref="VbCollection" /> with the received data.</returns>
        /// <remarks>Derived from example code at <see href="http://www.snmpsharpnet.com/?page_id=108" />.</remarks>
        private VbCollection DoWalkGetNext(Oid interfaceIdWalkRootOid, int depth)
        {
            // This Oid represents last Oid returned by
            //  the SNMP agent
            Oid lastOid = (Oid)interfaceIdWalkRootOid.Clone();
 
            // Pdu class used for all requests
            Pdu pdu = new Pdu(PduType.GetNext);
 
            VbCollection returnCollection = new VbCollection();

            // Loop through results
            while (lastOid != null)
            {
                // When Pdu class is first constructed, RequestId is set to a random value
                // that needs to be incremented on subsequent requests made using the
                // same instance of the Pdu class.
                if (pdu.RequestId != 0)
                {
                    pdu.RequestId += 1;
                }

                // Clear Oids from the Pdu class.
                pdu.VbList.Clear();

                // Initialize request PDU with the last retrieved Oid
                pdu.VbList.Add(lastOid);

                // Make SNMP request
                SnmpV1Packet result = (SnmpV1Packet)this.Target.Request(pdu, this.QueryParameters);

                // You should catch exceptions in the Request if using in real application.
                // [DJ3MU] : Yeah - cool idea - but I still wouldn't know what else to do with them.
                //           So for now, let fly them out to our caller.

                // If result is null then agent didn't reply or we couldn't parse the reply.
                if (result != null)
                {
                    // ErrorStatus other then 0 is an error returned by 
                    // the Agent - see SnmpConstants for error definitions
                    if (result.Pdu.ErrorStatus != 0)
                    {
                        // agent reported an error with the request
                        log.Error($"Error in SNMP reply. Error {result.Pdu.ErrorStatus} index {result.Pdu.ErrorIndex}");
                        lastOid = null;
                        break;
                    }
                    else
                    {
                        // Walk through returned variable bindings
                        foreach (Vb v in result.Pdu.VbList)
                        {
                            // Check that retrieved Oid is "child" of the root OID
                            if (interfaceIdWalkRootOid.IsRootOf(v.Oid))
                            {
                                returnCollection.Add(v);

                                lastOid = v.Oid;
                            }
                            else
                            {
                                // we have reached the end of the requested
                                // MIB tree. Set lastOid to null and exit loop
                                lastOid = null;
                            }
                        }
                    }
                }
                else
                {
                    throw new HamnetSnmpException($"No response received from SNMP agent for device '{this.Address}'");
                }
            }

            return returnCollection;
        }
    }
}
