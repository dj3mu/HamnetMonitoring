namespace HamnetMonitorCmdLine
{
    using System.Collections.Generic;
    using CommandLine;

    public class GlobalOptions
    {
        /// <summary>
        /// Construct taking all the parameters.
        /// </summary>
        /// <param name="hostsOrAddresses">List of host names or IP addresses to query.</param>
        public GlobalOptions(IEnumerable<string> hostsOrAddresses)
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
