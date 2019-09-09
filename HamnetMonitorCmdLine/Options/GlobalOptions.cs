namespace HamnetMonitorCmdLine
{
    using System.Collections.Generic;
    using CommandLine;

    /// <summary>
    /// Base class for global options (which are applicable to all verbs).
    /// </summary>
    public class GlobalOptions
    {
        /// <summary>
        /// Construct taking all the parameters.
        /// </summary>
        /// <param name="hostsOrAddresses">List of host names or IP addresses to query.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use for the queries.</param>
        /// <param name="printStats">A value indicating whether to print PDU stats before terminating.</param>
        public GlobalOptions(IEnumerable<string> hostsOrAddresses, int? snmpVersion, bool printStats)
        {
            this.HostsOrAddresses = hostsOrAddresses;
            this.SnmpVersion = snmpVersion;
            this.PrintStats = printStats;
        }

        /// <summary>
        /// Gets a list of host names or IP addresses to query.
        /// </summary>
        [Option('h', "host", Min = 1, Required = true, HelpText = "List of host names or IP addresses that shall be queried.")]
        public IEnumerable<string> HostsOrAddresses { get; }

        /// <summary>
        /// Gets the SNMP protocol version to use.
        /// </summary>
        [Option('s', "snmpversion", Required = false, HelpText = "The version of the SNMP protocol to use. Defaults to version 2.")]
        public int? SnmpVersion { get; } = null;

        /// <summary>
        /// Gets a value indicating whether to print PDU stats before terminating.
        /// </summary>
        [Option("stats", Required = false, HelpText = "Display request and response SNMP PDU stats before terminating.")]
        public bool PrintStats { get; } = false;
    }
}
