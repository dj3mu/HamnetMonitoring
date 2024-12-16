using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Implementation of the performance counter.
    /// </summary>
    internal class PerformanceCounter : IPerformanceCounter
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Access to the mutable results per device.
        /// </summary>
        private readonly ConcurrentDictionary<IPAddress, PerformanceSingleResultSet> resultsPerDeviceMutable = new ConcurrentDictionary<IPAddress, PerformanceSingleResultSet>();

        /// <summary>
        /// Access to the mutable results per request type.
        /// </summary>
        private readonly ConcurrentDictionary<string, PerformanceSingleResultSet> resultsPerRequestTypeMutable = new ConcurrentDictionary<string, PerformanceSingleResultSet>();

        /// <summary>
        /// Backing field for <see cref="ResultsPerDevice" />.
        /// </summary>
        private readonly ConcurrentDictionary<IPAddress, IPerformanceSingleResultSet> resultsPerDeviceInterfaced = new ConcurrentDictionary<IPAddress, IPerformanceSingleResultSet>();

        /// <summary>
        /// Backing field for <see cref="ResultsPerRequestType" />.
        /// </summary>
        private readonly ConcurrentDictionary<string, IPerformanceSingleResultSet> resultsPerRequestTypeInterfaced = new ConcurrentDictionary<string, IPerformanceSingleResultSet>();

        /// <summary>
        /// Backing field for the overall results.
        /// </summary>
        private readonly PerformanceSingleResultSet overallResultsBacking = new PerformanceSingleResultSet();

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IPerformanceSingleResultSet> ResultsPerRequestType => this.resultsPerRequestTypeInterfaced;

        /// <inheritdoc />
        public IReadOnlyDictionary<IPAddress, IPerformanceSingleResultSet> ResultsPerDevice => this.resultsPerDeviceInterfaced;

        public IPerformanceSingleResultSet OverallResults => this.overallResultsBacking;

        /// <summary>
        /// Records an SNMP in performance counter.
        /// </summary>
        /// <param name="destinationAddress">The destination IP address that this request has been sent to.</param>
        /// <param name="request">The request.</param>
        /// <param name="result">The result.</param>
        internal void Record(IpAddress destinationAddress, Pdu request, SnmpPacket result)
        {
            try
            {
                if (destinationAddress == null)
                {
                    log.Error("Failed to record for performance count: destinationAddress == null");
                    return;
                }

                if (request == null)
                {
                    log.Error("Failed to record for performance count: request == null");
                    return;
                }

                if (result == null)
                {
                    log.Error("Failed to record for performance count: result == null");
                    return;
                }

                this.overallResultsBacking.Record(request, result);

                // per address results:
                if (!this.resultsPerDeviceMutable.TryGetValue((IPAddress)destinationAddress, out PerformanceSingleResultSet singleResultSet))
                {
                    singleResultSet = new PerformanceSingleResultSet();

                    // Note: It's crucial that we put exactly the same object in both dictionaries !
                    this.resultsPerDeviceMutable.TryAdd((IPAddress)destinationAddress, singleResultSet);
                    this.resultsPerDeviceInterfaced.TryAdd((IPAddress)destinationAddress, singleResultSet);
                }

                singleResultSet.Record(request, result);

                // per request type results:
                if (!this.resultsPerRequestTypeMutable.TryGetValue(request.Type.ToString(), out singleResultSet))
                {
                    singleResultSet = new PerformanceSingleResultSet();

                    // Note: It's crucial that we put exactly the same object in both dictionaries !
                    this.resultsPerRequestTypeMutable.TryAdd(request.Type.ToString(), singleResultSet);
                    this.resultsPerRequestTypeInterfaced.TryAdd(request.Type.ToString(), singleResultSet);
                }

                singleResultSet.Record(request, result);
            }
            catch(Exception ex)
            {
                // eat it up - this is crude but the simple small recording of stats shall not interrupt anything useful
                log.Error("Exception during statistics recording", ex);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder returnBuilder = new StringBuilder(1024);

            returnBuilder.AppendLine("Overall statistics:");
            returnBuilder.AppendLine("===================");
            returnBuilder.AppendLine(SnmpAbstraction.IndentLines(this.overallResultsBacking.ToString()));

            returnBuilder.AppendLine().AppendLine("Per request-type statistics:");
            returnBuilder.AppendLine("============================");
            foreach (var item in this.resultsPerRequestTypeInterfaced)
            {
                returnBuilder.AppendLine().Append(item.Key).AppendLine(":");
                returnBuilder.AppendLine(SnmpAbstraction.IndentLines(item.Value.ToString()));
            }

            returnBuilder.AppendLine().AppendLine("Per device statistics:");
            returnBuilder.AppendLine("======================");
            foreach (var item in this.resultsPerDeviceInterfaced)
            {
                returnBuilder.AppendLine().Append(item.Key).AppendLine(":");
                returnBuilder.AppendLine(SnmpAbstraction.IndentLines(item.Value.ToString()));
            }

            return returnBuilder.ToString();
        }

        /// <summary>
        /// Container for a single result set.
        /// </summary>
        private class PerformanceSingleResultSet : IPerformanceSingleResultSet
        {
            /// <inheritdoc />
            public int TotalNumberOfRequestPdus { get; private set; }

            /// <inheritdoc />
            public int TotalNumberOfResponsePdus { get; private set; }

            /// <inheritdoc />
            public int TotalNumberOfErrorResponses { get; private set; }

            /// <summary>
            /// Records an SNMP in performance counter.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="result">The result.</param>
            public void Record(Pdu request, SnmpPacket result)
            {
                this.TotalNumberOfRequestPdus += request.VbCount;
                this.TotalNumberOfResponsePdus += result.Pdu.VbCount;

                if (result.Pdu.ErrorStatus != 0)
                {
                    ++this.TotalNumberOfErrorResponses;
                }
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return $"Total Requests : {this.TotalNumberOfRequestPdus}{Environment.NewLine}Total Responses: {this.TotalNumberOfResponsePdus}{Environment.NewLine}Total Errors   : {this.TotalNumberOfErrorResponses}";
            }
        }
    }
}