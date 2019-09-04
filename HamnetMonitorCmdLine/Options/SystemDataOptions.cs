namespace HamnetMonitorCmdLine
{
    using System.Collections.Generic;
    using CommandLine;
    using CommandLine.Text;

    [Verb("SystemData", HelpText = "Query basic system data (description, admin, root OID, etc.).")]
    public class SystemDataOptions : GlobalOptions
    {
        /// <summary>
        /// Construct taking all the parameters.
        /// </summary>
        /// <param name="meanings">List of abstracted meanings that shall be queried.</param>
        public SystemDataOptions(
            IEnumerable<string> hostsOrAddresses)
            : base(hostsOrAddresses)
        {
        }

        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example("Query System Data", new SystemDataOptions(new string[] { "my-test-host.example.org" }))
                };
            }
        }
    }
}
