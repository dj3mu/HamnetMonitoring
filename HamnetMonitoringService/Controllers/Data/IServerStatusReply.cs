using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using SnmpAbstraction;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Interface to the information returned by the status REST command.
    /// </summary>
    public interface IServerStatusReply
    {
        /// <summary>
        /// Gets the highes supported API version of this server.
        /// </summary>
        int MaximumSupportedApiVersion { get; }

        /// <summary>
        /// Gets the server version information.
        /// </summary>
        string ServerVersion { get; }

        /// <summary>
        /// Gets the processe's start time.
        /// </summary>
        DateTime ProcessStartTime { get; }

        /// <summary>
        /// Gets the uptime of the current server process.
        /// </summary>
        TimeSpan ProcessUptime { get; }

        /// <summary>
        /// Gets the CPU time consumed by the current service process.
        /// </summary>
        TimeSpan ProcessCpuTime { get; }

        /// <summary>
        /// Gets the processe's working set size.
        /// </summary>
        long ProcessWorkingSet { get; }

        /// <summary>
        /// Gets the processe's private set size.
        /// </summary>
        long ProcessPrivateSet { get; }

        /// <summary>
        /// Gets the processe's number of threads.
        /// </summary>
        int ProcessThreads { get; }

        /// <summary>
        /// Gets the database statistics
        /// </summary>
        IReadOnlyDictionary<string, IDatabasestatistic> DatabaseStatistic { get; }

        /// <summary>
        /// Gets the configuration settings of the application.
        /// </summary>
        IReadOnlyDictionary<string, IConfigurationInfo> Configurations { get; }

        /// <summary>
        /// Gets the values of the performance counter.
        /// </summary>
        IPerformanceCounter PerformanceCounter { get; }
    }

    /// <summary>
    /// Interface to a single database statistic.
    /// </summary>
    public interface IDatabasestatistic : IReadOnlyDictionary<string, string>
    {
    }

    /// <summary>
    /// Interface to a single configuration dictionary.
    /// </summary>
    public interface IConfigurationInfo : IReadOnlyDictionary<string, string>
    {
    }
}
