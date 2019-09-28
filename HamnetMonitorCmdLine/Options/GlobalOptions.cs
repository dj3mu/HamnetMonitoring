namespace HamnetMonitorCmdLine
{
    using CommandLine;

    /// <summary>
    /// Base class for global options (which are applicable to all verbs).
    /// </summary>
    public abstract class GlobalOptions
    {
        /// <summary>
        /// Construct taking all the parameters.
        /// </summary>
        /// <param name="snmpVersion">The SNMP protocol version to use for the queries.</param>
        /// <param name="printStats">A value indicating whether to print PDU stats before terminating.</param>
        public GlobalOptions(int? snmpVersion, bool printStats)
        {
            this.SnmpVersion = snmpVersion;
            this.PrintStats = printStats;
        }

        /// <summary>
        /// Gets the SNMP protocol version to use.
        /// </summary>
        [Option('v', "snmpversion", Required = false, HelpText = "The version of the SNMP protocol to use. Defaults to version 2.")]
        public int? SnmpVersion { get; set; } = null;

        /// <summary>
        /// Gets flag
        /// </summary>
        [Option('c', "caching", Required = false, HelpText = "Choose whether to use the cache database or not.")]
        public bool UseCaching { get; set; } = true;

        /// <summary>
        /// Gets a value indicating whether to print PDU stats before terminating.
        /// </summary>
        [Option("stats", Required = false, HelpText = "Display request and response SNMP PDU stats before terminating.")]
        public bool PrintStats { get; set; } = false;
    }
}
