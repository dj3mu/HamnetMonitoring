namespace HamnetMonitorCmdLine
{
    using System.Collections.Generic;
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    /// Definition of the SystemData verb and its verb-specific options.
    /// </summary>
    [Verb("SystemData", HelpText = "Query basic system data (description, admin, root OID, etc.).")]
    public class SystemDataOptions : HostSpecificOptions
    {
        /// <summary>
        /// Construct taking all the parameters.
        /// </summary>
        /// <param name="hostsOrAddresses">List of Host names or IP addresses to query.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use for the queries.</param>
        /// <param name="printStats">A value indicating whether to print PDU stats before terminating.</param>
        public SystemDataOptions(
            IEnumerable<string> hostsOrAddresses,
            int? snmpVersion,
            bool printStats)
            : base(hostsOrAddresses, snmpVersion, printStats)
        {
        }

        /// <summary>
        /// CommandLine framework specific way to provide usage examples.
        /// </summary>
        /// <value></value>
        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example("Query System Data", new SystemDataOptions(new string[] { "my-test-host.example.org" }, 2, false))
                };
            }
        }
    }
}
