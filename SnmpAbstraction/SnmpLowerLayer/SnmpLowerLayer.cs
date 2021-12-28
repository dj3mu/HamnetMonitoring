using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
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
        /// To create strictly incrementing request ID for every request. Read by means of Interlocked.Increment.
        /// </summary>
        private static int nextRequestId = 0;

        /// <summary>
        /// The query parameters.
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

            this.SetVersionAndCommunity(this.Options.ProtocolVersion, this.Options.Community);
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

        /// <summary>
        /// Gets access to the real system data container (mainly for device detector to add model and version).
        /// </summary>
        internal LazyLoadingDeviceSystemData InternalSystemData => this.cachedSystemData;

        /// <summary>
        /// Gets the SNMP protocol version that is currently in use.
        /// </summary>
        public SnmpVersion ProtocolVersionInUse => this.QueryParameters.Version;

        /// <inheritdoc />
        public VbCollection DoWalk(Oid interfaceIdWalkRootOid)
        {
            if (this.ProtocolVersionInUse == SnmpVersion.Ver1)
            {
                return this.DoWalkGetNext(interfaceIdWalkRootOid);
            }
            else if ((this.ProtocolVersionInUse == SnmpVersion.Ver2) || (this.ProtocolVersionInUse == SnmpVersion.Ver3))
            {
                return this.DoWalkBulk(interfaceIdWalkRootOid);
            }
            else
            {
                throw new InvalidOperationException($"Don't know how to do an SNMP walk for SNMP protocol version '{this.Options.ProtocolVersion}'");
            }
        }

        /// <inheritdoc />
        public VbCollection Query(IEnumerable<Oid> oids)
        {
            if (this.disposedValue)
            {
                throw new ObjectDisposedException(nameof(SnmpLowerLayer), "The object is already disposed off. Cannot execute any more commands on it.");
            }

            if (oids == null)
            {
                throw new ArgumentNullException(nameof(oids), "The list of OIDs to query is null");
            }

            SnmpPacket response = this.SendRequest(oids);

            if (response == null)
            {
                throw new HamnetSnmpException($"Query for {oids.Count()} OIDs from {this.Address} produced 'null' response", this.Address?.ToString());
            }

            // ErrorStatus other then 0 is an error returned by the Agent - see SnmpConstants for error definitions
            if (response.Pdu.ErrorStatus != 0)
            {
                // agent reported an error with the request
                var ex = new HamnetSnmpException($"Error in SNMP reply from device '{this.Address}': Error status {response.Pdu.ErrorStatus} at index {response.Pdu.ErrorIndex}, requested OIDs were '{string.Join(", ", response.Pdu.VbList.Select(o => o.Oid.ToString()))}'", this.Address?.ToString());
                log.Info(ex.Message);
                throw ex;
            }

            return response.Pdu.VbList;
        }

        /// <summary>
        /// Adjusts the SNMP version in use away from that in options to the one given here.
        /// </summary>
        /// <param name="snmpVersion">The new SNMP version to use.</param>
        public void AdjustSnmpVersion(SnmpVersion snmpVersion)
        {
            this.SetVersionAndCommunity(snmpVersion, this.Options.Community);
        }

        /// <inheritdoc />
        public VbCollection Query(Oid firstOid, params Oid[] oids)
        {
            IEnumerable<Oid> oidEnum = new Oid[] { firstOid };
            if (oids != null)
            {
                oidEnum = oidEnum.Concat(oids);
            }

            return this.Query(oidEnum);
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
            var pduType = PduType.Get;

            Interlocked.CompareExchange(ref nextRequestId, 0, int.MaxValue); // wrap the request ID
            var requestId = Interlocked.Increment(ref nextRequestId);
            Pdu pdu = new Pdu(pduType)
            {
                RequestId = requestId // 0 --> generate random ID on encode, anyhting else --> use that value for request ID
            };

            foreach (Oid item in oids)
            {
                pdu.VbList.Add(item);
            }

            log.Debug($"Using request ID '{requestId}' for PDU with {pdu.VbCount} elements to {this.Address} - VBs are:{Environment.NewLine}{string.Join(Environment.NewLine, pdu.VbList.Select(vb => vb.Oid))}");

            using(var target = new UdpTarget((IPAddress)this.Address, this.Options.Port, Convert.ToInt32(this.Options.Timeout.TotalMilliseconds), this.Options.Retries))
            {
                SnmpPacket result = target.Request(pdu, this.QueryParameters);

                SnmpAbstraction.RecordSnmpRequest(this.Address, pdu, result);

                return result;
            }
        }

        /// <summary>
        /// Sets the given protocol version and community.
        /// </summary>
        /// <param name="protocolVersion">The new protocol version to use.</param>
        /// <param name="community">The new communitiy to use.</param>
        private void SetVersionAndCommunity(SnmpVersion protocolVersion, OctetString community)
        {
            if ((this.QueryParameters?.Version == protocolVersion) && (this.QueryParameters?.Community == community))
            {
#if DEBUG
                log.Debug($"Device '{this.Address}': SNMP community '{community}' and protocol version '{protocolVersion}' already set. No change.");
#endif
                return;
            }

#if DEBUG
            log.Debug($"Device '{this.Address}': From now on using SNMP community '{community}' and protocol version '{protocolVersion}'");
#endif
            this.QueryParameters = new AgentParameters(community)
            {
                Version = protocolVersion,
                DisableReplySourceCheck = true
            };
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
        /// <returns>A <see cref="VbCollection" /> with the received data.</returns>
        /// <remarks>Derived from example code at <see href="http://www.snmpsharpnet.com/?page_id=108" />.</remarks>
        private VbCollection DoWalkBulk(Oid interfaceIdWalkRootOid)
        {
            // This Oid represents last Oid returned by the SNMP agent
            Oid lastOid = (Oid)interfaceIdWalkRootOid.Clone();

            Interlocked.CompareExchange(ref nextRequestId, 0, int.MaxValue); // wrap the request ID
            var requestId = Interlocked.Increment(ref nextRequestId);

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
                Interlocked.CompareExchange(ref nextRequestId, 0, int.MaxValue); // wrap the request ID
                pdu.RequestId = Interlocked.Increment(ref nextRequestId);

                // Clear Oids from the Pdu class.
                pdu.VbList.Clear();

                // Initialize request PDU with the last retrieved Oid
                pdu.VbList.Add(lastOid);

                SnmpV2Packet result;
                using(var target = new UdpTarget((IPAddress)this.Address, this.Options.Port, Convert.ToInt32(this.Options.Timeout.TotalMilliseconds), this.Options.Retries))
                {
                    // Make SNMP request
                    result = (SnmpV2Packet)target.Request(pdu, this.QueryParameters);
                }

                SnmpAbstraction.RecordSnmpRequest(this.Address, pdu, result);

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
                        log.Warn($"Error in SNMP bulk walk reply of device '{this.Address}': Error status {result.Pdu.ErrorStatus} at index {result.Pdu.ErrorIndex}");
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
                    throw new HamnetSnmpException($"No response received from SNMP agent for device '{this.Address}'", this.Address?.ToString());
                }
            }

            return returnCollection;
        }

        /// <summary>
        /// Performs SNMP walk using Ver 1 GetNext operation.
        /// </summary>
        /// <param name="interfaceIdWalkRootOid">The retrievable value to start walking at. The actual OID is resolved from the device database.</param>
        /// <returns>A <see cref="VbCollection" /> with the received data.</returns>
        /// <remarks>Derived from example code at <see href="http://www.snmpsharpnet.com/?page_id=108" />.</remarks>
        private VbCollection DoWalkGetNext(Oid interfaceIdWalkRootOid)
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
                Interlocked.CompareExchange(ref nextRequestId, 0, int.MaxValue); // wrap the request ID
                pdu.RequestId = Interlocked.Increment(ref nextRequestId);

                // Clear Oids from the Pdu class.
                pdu.VbList.Clear();

                // Initialize request PDU with the last retrieved Oid
                pdu.VbList.Add(lastOid);

                SnmpV1Packet result;
                using(var target = new UdpTarget((IPAddress)this.Address, this.Options.Port, Convert.ToInt32(this.Options.Timeout.TotalMilliseconds), this.Options.Retries))
                {
                    // Make SNMP request
                    result = (SnmpV1Packet)target.Request(pdu, this.QueryParameters);
                }

                SnmpAbstraction.RecordSnmpRequest(this.Address, pdu, result);

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
                        log.Warn($"Error in SNMP GetNextWalk reply of device '{this.Address}': Error status {result.Pdu.ErrorStatus} at index {result.Pdu.ErrorIndex}");
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
                    throw new HamnetSnmpException($"No response received from SNMP agent for device '{this.Address}'", this.Address?.ToString());
                }
            }

            return returnCollection;
        }
    }
}
