namespace HamnetMonitorCmdLine
{
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    /// Definition of the HamnetDbLinks verb and its verb-specific options.
    /// </summary>
    [Verb("HamnetDbLinks", HelpText = "Query link details for some or all entries of HamnetDB.")]
    public class HamnetDbLinksOptions : GlobalOptions
    {
        /// <summary>
        /// Construct default instance.
        /// </summary>
        public HamnetDbLinksOptions()
            : this(2, false)
        {
        }

        /// <summary>
        /// Construct taking all the parameters.
        /// </summary>
        /// <param name="snmpVersion">The SNMP protocol version to use for the queries.</param>
        /// <param name="printStats">A value indicating whether to print PDU stats before terminating.</param>
        public HamnetDbLinksOptions(
            int? snmpVersion
            , bool printStats)
            : base(snmpVersion, printStats)
        {
        }

        /// <summary>
        /// Gets the host name or IP or the MySQL database server to query for the hosts and subnets (i.e. _the_ HamnetDB).
        /// </summary>
        [Option('f', "connectionStringFile", Required = true, HelpText = "Sets the path and name of a file containing the connections string to access the HamnetDB. Attention: This string contains plain-text password. The file shall therefore be protected by proper ACL setting.")]
        public string ConnectionStringFile { get; set; } = null;

        /// <summary>
        /// Gets the number of the entries to start querying at.
        /// </summary>
        [Option('o', "offset", Required = false, HelpText = "Sets the number of the entries to start querying at. Together with -c the queries can be sliced as needed.")]
        public int StartOffset { get; set; } = 0;

        /// <summary>
        /// Gets the number of entries returned from Database that shall be queried.
        /// </summary>
        [Option('c', "count", Required = false, HelpText = "Sets the number of entries to query at most. Together with -o the queries can be sliced as needed.")]
        public int NumberOfEntries { get; set; } = int.MaxValue;

        /// <summary>
        /// Gets a network in CIDR or IP/Netmask notation for which the links shall be queried.
        /// </summary>
        [Option('n', "network", Required = false, HelpText = "Sets a network in CIDR or IP/Netmask notation for which the links shall be queried.")]
        public string Network { get; set; } = null;

        ///// <summary>
        ///// CommandLine framework specific way to provide usage examples.
        ///// </summary>
        ///// <value></value>
        //[Usage]
        //public static IEnumerable<Example> Examples
        //{
        //    get
        //    {
        //        return new List<Example>()
        //        {
        //            new Example("Fetch link details of the link between my-test-host.example.org and my-other-host.example.org and between my-test-host.example.org and IP 1.2.3.4", new LinkDetailsOptions(new string[] { "my-test-host.example.org", "my-other-host.example.org", "1.2.3.4" }, 2, false))
        //        };
        //    }
        //}
    }
}
