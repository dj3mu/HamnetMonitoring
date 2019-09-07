namespace HamnetMonitorCmdLine
{
    using System.Collections.Generic;
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    /// Definition of the WirelessPeers verb and its verb-specific options.
    /// </summary>
    [Verb("WirelessPeers", HelpText = "Query wireless peer information (MAC, etc.).")]
    public class WirelessPeersOptions : GlobalOptions
    {
        /// <summary>
        /// Construct taking all the parameters.
        /// </summary>
        /// <param name="hostsOrAddresses">List of Host names or IP addresses to query.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use for the queries.</param>
        public WirelessPeersOptions(
            IEnumerable<string> hostsOrAddresses, int? snmpVersion)
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
                    new Example("Query wireless peer data", new WirelessPeersOptions(new string[] { "my-test-host.example.org" }, 2))
                };
            }
        }
    }
}
