namespace HamnetMonitorCmdLine
{
    using System.Collections.Generic;
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    /// Definition of the InterfaceData verb and its verb-specific options.
    /// </summary>
    [Verb("InterfaceData", HelpText = "Query interface data (type, MAC, IP, etc.).")]
    public class InterfaceDataOptions : GlobalOptions
    {
        /// <summary>
        /// Construct taking all the parameters.
        /// </summary>
        /// <param name="hostsOrAddresses">List of Host names or IP addresses to query.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use for the queries.</param>
        public InterfaceDataOptions(
            IEnumerable<string> hostsOrAddresses,
            int? snmpVersion)
            : base(hostsOrAddresses, snmpVersion)
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
                    new Example("Query Interface Data", new InterfaceDataOptions(new string[] { "my-test-host.example.org" }, 2))
                };
            }
        }
    }
}
