namespace HamnetMonitorCmdLine
{
    using System.Collections.Generic;
    using CommandLine;
    using CommandLine.Text;

    /// <summary>
    /// Definition of the FetchLinkDetails verb and its verb-specific options.
    /// </summary>
    [Verb("FetchLinkDetails", HelpText = "Query link details to the listed remote devices.")]
    public class FetchLinkDetailsOptions : GlobalOptions
    {
        /// <summary>
        /// Construct taking all the parameters.
        /// </summary>
        /// <param name="hostsOrAddresses">List of Host names or IP addresses to query.</param>
        public FetchLinkDetailsOptions(
            IEnumerable<string> hostsOrAddresses)
            : base(hostsOrAddresses)
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
                    new Example("Fetch link details", new FetchLinkDetailsOptions(new string[] { "my-test-host.example.org", "my-remote-host.example.org", "1.2.3.4" }))
                };
            }
        }
    }
}
