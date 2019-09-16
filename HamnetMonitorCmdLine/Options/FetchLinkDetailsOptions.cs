namespace HamnetMonitorCmdLine
{
    using System.Collections.Generic;
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    /// Definition of the LinkDetails verb and its verb-specific options.
    /// </summary>
    [Verb("LinkDetails", HelpText = "Query link details between the listed remote devices.")]
    public class LinkDetailsOptions : HostSpecificOptions
    {
        /// <summary>
        /// Construct taking all the parameters.
        /// </summary>
        /// <param name="hostsOrAddresses">List of Host names or IP addresses to query.</param>
        /// <param name="snmpVersion">The SNMP protocol version to use for the queries.</param>
        /// <param name="printStats">A value indicating whether to print PDU stats before terminating.</param>
        public LinkDetailsOptions(
            IEnumerable<string> hostsOrAddresses
            , int? snmpVersion
            , bool printStats)
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
                    new Example("Fetch link details of the link between my-test-host.example.org and my-other-host.example.org and between my-test-host.example.org and IP 1.2.3.4", new LinkDetailsOptions(new string[] { "my-test-host.example.org", "my-other-host.example.org", "1.2.3.4" }, 2, false))
                };
            }
        }
    }
}
