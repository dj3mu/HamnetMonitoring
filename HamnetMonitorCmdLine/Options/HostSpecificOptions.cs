namespace HamnetMonitorCmdLine
{
    using System.Collections.Generic;
    using CommandLine;

    /// <summary>
    /// Base class for global options (which are applicable to all verbs).
    /// </summary>
    public abstract class HostSpecificOptions : GlobalOptions
    {
        /// <summary>
        /// Construct taking all the parameters.
        /// </summary>
        /// <param name="hostsOrAddresses">List of host names or IP addresses to query.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use for the queries.</param>
        /// <param name="printStats">A value indicating whether to print PDU stats before terminating.</param>
        public HostSpecificOptions(IEnumerable<string> hostsOrAddresses, int? snmpVersion, bool printStats)
            : base(snmpVersion, printStats)
        {
            this.HostsOrAddresses = hostsOrAddresses;
        }

        /// <summary>
        /// Gets a list of host names or IP addresses to query.
        /// </summary>
        [Option('h', "host", Min = 1, Required = true, HelpText = "List of host names or IP addresses that shall be queried.")]
        public IEnumerable<string> HostsOrAddresses { get; }
    }
}
