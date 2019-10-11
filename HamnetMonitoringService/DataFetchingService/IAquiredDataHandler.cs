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
        /// Records a failing query.
        /// </summary>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <param name="inputData">The input data that caused the failure.</param>
        void RecordFailingQuery(Exception exception, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData);

        /// <summary>
        /// Records a failing query asynchronuously in a separate task.
        /// </summary>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <param name="inputData">The input data that caused the failure.</param>
        /// <returns>The task that is asynchronuously executing the recording.</returns>
        Task RecordFailingQueryAsync(Exception exception, KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData);

        /// <summary>
        /// Records the link details in the database.
        /// </summary>
        /// <param name="inputData">The input data to record link details for.</param>
        /// <param name="linkDetails">The link details to record.</param>
        /// <param name="queryTime">The time of the data aquisition (recorded with the data).</param>
        void RecordDetailsInDatabase(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData, ILinkDetails linkDetails, DateTime queryTime);

        /// <summary>
        /// Records the link details in the database asynchronuously in a separate task.
        /// </summary>
        /// <param name="inputData">The input data to record link details for.</param>
        /// <param name="linkDetails">The link details to record.</param>
        /// <param name="queryTime">The time of the data aquisition (recorded with the data).</param>
        /// <returns>The task that is asynchronuously executing the recording.</returns>
        Task RecordDetailsInDatabaseAsync(KeyValuePair<IHamnetDbSubnet, IHamnetDbHosts> inputData, ILinkDetails linkDetails, DateTime queryTime);

        /// <summary>
        /// Tells the data handler that aquisition has now finished and it can perform cleanup tasks as needed.
        /// </summary>
        void AquisitionFinished();
    }
}