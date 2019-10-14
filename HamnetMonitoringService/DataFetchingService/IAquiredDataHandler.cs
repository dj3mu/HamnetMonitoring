using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HamnetDbAbstraction;
using SnmpAbstraction;

namespace RestService.DataFetchingService
{
    /// <summary>
    /// Interface to a handler that does &quot;something&quot; with aquired data.
    /// </summary>
    internal interface IAquiredDataHandler : IDisposable
    {
        /// <summary>
        /// Gets the name (i.e. an identifier) of the handler.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Requests the handler to prepare for a new aquisition.
        /// </summary>
        void PrepareForNewAquisition();

        /// <summary>
        /// Records a failing RSSI query.
        /// </summary>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <param name="inputData">The input data that caused the failure.</param>
        void RecordFailingRssiQuery(Exception exception, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData);

        /// <summary>
        /// Records a failing RSSI query asynchronuously in a separate task.
        /// </summary>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <param name="inputData">The input data that caused the failure.</param>
        /// <returns>The task that is asynchronuously executing the recording.</returns>
        Task RecordFailingRssiQueryAsync(Exception exception, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData);

        /// <summary>
        /// Records a failing BGP query.
        /// </summary>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <param name="host">The host that caused the failure.</param>
        void RecordFailingBgpQuery(Exception exception, IHamnetDbHost host);

        /// <summary>
        /// Records a failing BGP query asynchronuously in a separate task.
        /// </summary>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <param name="host">The host that caused the failure.</param>
        /// <returns>The task that is asynchronuously executing the recording.</returns>
        Task RecordFailingBgpQueryAsync(Exception exception, IHamnetDbHost host);

        /// <summary>
        /// Records the link details in the database.
        /// </summary>
        /// <param name="inputData">The input data to record link details for.</param>
        /// <param name="linkDetails">The link details to record.</param>
        /// <param name="queryTime">The time of the data aquisition (recorded with the data).</param>
        void RecordRssiDetailsInDatabase(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData, ILinkDetails linkDetails, DateTime queryTime);

        /// <summary>
        /// Records the link details in the database asynchronuously in a separate task.
        /// </summary>
        /// <param name="inputData">The input data to record link details for.</param>
        /// <param name="linkDetails">The link details to record.</param>
        /// <param name="queryTime">The time of the data aquisition (recorded with the data).</param>
        /// <returns>The task that is asynchronuously executing the recording.</returns>
        Task RecordRssiDetailsInDatabaseAsync(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData, ILinkDetails linkDetails, DateTime queryTime);

        /// <summary>
        /// Records the link details in the database.
        /// </summary>
        /// <param name="host">The host that the peers are for.</param>
        /// <param name="peers">The BGP peers to record.</param>
        /// <param name="queryTime">The time of the data aquisition (recorded with the data).</param>
        void RecordDetailsInDatabase(IHamnetDbHost host, IBgpPeers peers, DateTime queryTime);

        /// <summary>
        /// Records the link details in the database asynchronuously in a separate task.
        /// </summary>
        /// <param name="host">The host that the peers are for.</param>
        /// <param name="peers">The BGP peers to record.</param>
        /// <param name="queryTime">The time of the data aquisition (recorded with the data).</param>
        /// <returns>The task that is asynchronuously executing the recording.</returns>
        Task RecordDetailsInDatabaseAsync(IHamnetDbHost host, IBgpPeers peers, DateTime queryTime);

        /// <summary>
        /// Tells the data handler that aquisition has now finished and it can perform cleanup tasks as needed.
        /// </summary>
        void AquisitionFinished();
    }
}