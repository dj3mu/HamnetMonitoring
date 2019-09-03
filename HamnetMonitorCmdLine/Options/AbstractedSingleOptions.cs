namespace HamnetMonitorCmdLine
{
    using System.Collections.Generic;
    using CommandLine;
    using CommandLine.Text;

    [Verb("single", HelpText = "Query abstracted, but single values (without further link matching or similar algorithms).")]
    public class AbstractedSingleOptions : GlobalOptions
    {
        /// <summary>
        /// Construct taking all the parameters.
        /// </summary>
        /// <param name="meanings">List of abstracted meanings that shall be queried.</param>
        public AbstractedSingleOptions(
            IEnumerable<string> hostsOrAddresses,
            IEnumerable<string> meanings)
            : base(hostsOrAddresses)
        {
            this.Meanings = meanings;
        }

        /// <summary>
        /// Gets a list of abstracted meanings that shall be queried.
        /// </summary>
        [Option('m', "meaning", Min = 1, Required = true, HelpText = "List of 'meanings' (i.e. abstracted values) that shall be queried.")]
        public IEnumerable<string> Meanings { get; }
        
        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example("Query System Data", new AbstractedSingleOptions(new string[] { "my-test-host.example.org" }, new string[] { "SystemData" }))
                };
            }
        }
    }
}
