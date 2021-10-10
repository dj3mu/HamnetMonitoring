namespace HamnetMonitorCmdLine
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Xml;
    using CommandLine;
    using HamnetDbAbstraction;
    using log4net;
    using SnmpAbstraction;
    using SnmpSharpNet;

    /// <summary>
    /// Main entry class
    /// </summary>
    class Program
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static ILog log = null;

        private static readonly string Log4netConfigurationFile = "Config/log4net.config";

        private static string libraryInformationalVersionBacking = null;

        /// <summary>
        /// Gets the informational version ID of the InstallationServiceLib.
        /// </summary>
        public static string LibraryInformationalVersion
        {
            get
            {
                if (!String.IsNullOrEmpty(libraryInformationalVersionBacking))
                {
                    return libraryInformationalVersionBacking;
                }

                InitBackings();

                return libraryInformationalVersionBacking;
            }
        }

        /// <summary>
        /// Initializes and returns the handle to log4net.
        /// </summary>
        /// <param name="type">The &quot;calling&quot; type (included with the trace output).</param>
        /// <returns>The handle to log4net.</returns>
        internal static ILog GetLogger(Type type)
        {
            if (log == null)
            {
                var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                XmlDocument log4netConfig = new XmlDocument();
                log4netConfig.Load(File.OpenRead(Path.Combine(assemblyFolder, Log4netConfigurationFile)));
                var repo = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
                log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);

                log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            }

            return LogManager.GetLogger(type);
        }

        /// <summary>
        /// Main entry method.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private static int Main(string[] args)
        {
            InitBackings();

            try
            {
                return CommandLine.Parser.Default.ParseArguments<SystemDataOptions, InterfaceDataOptions, WirelessPeersOptions, LinkDetailsOptions, HamnetDbLinksOptions>(args)
                    .MapResult(
                        (SystemDataOptions opts) => RunAddAndReturnExitCode(opts),
                        (InterfaceDataOptions opts) => RunAddAndReturnExitCode(opts),
                        (WirelessPeersOptions opts) => RunAddAndReturnExitCode(opts),
                        (LinkDetailsOptions opts) => RunAddAndReturnExitCode(opts),
                        (HamnetDbLinksOptions opts) => RunAddAndReturnExitCode(opts),
                        errs => (int)ExitCodes.InvalidCommandLine);
            }
            catch(HamnetSnmpException hamnetEx)
            {
                Console.Error.Write($"ERROR: HNEX: {FormatExceptionForUser(hamnetEx)}");
                return (int)ExitCodes.HamnetException;
            }
            catch(SnmpException snmpEx)
            {
                Console.Error.Write($"ERROR: SNEX: {FormatExceptionForUser(snmpEx)}");
                return (int)ExitCodes.SnmpException;
            }
            catch(Exception ex)
            {
                Console.Error.Write($"ERROR: EX: {FormatExceptionForUser(ex)}");
                return (int)ExitCodes.Exception;
            }
        }

        /// <summary>
        /// Formats the exception for user output.
        /// </summary>
        /// <param name="ex">The exception to format.</param>
        /// <returns>Formatted exception depending on </returns>
        private static object FormatExceptionForUser(Exception ex)
        {
#if DEBUG
            return ex.ToString();
#else
            return ex.Message;
#endif
        }

        /// <summary>
        /// Execution of HamnetDB retrieved monitoring
        /// </summary>
        /// <param name="opts">The options defining the data to retrieve and query.</param>
        /// <returns>The exit code to return.</returns>
        private static int RunAddAndReturnExitCode(HamnetDbLinksOptions opts)
        {
            log.Info("Running HamnetDB-retrieved monitoring");

            var querierOptions = CreateQuerierOptions(opts);

            var accessor = HamnetDbProvider.Instance.GetHamnetDb(opts.ConnectionStringFile);

            Console.WriteLine();
            Console.WriteLine($"Getting unique host pairs to be monitored from HamnetDB. Please stand by ...");

            var uniquePairs = accessor.UniqueMonitoredHostPairsInSameSubnet();

            Console.WriteLine($"... found {uniquePairs.Count} unique pairs");

            var pairsSlicedForOptions = uniquePairs.Skip(opts.StartOffset).Take(opts.NumberOfEntries);

            Console.Write("SNMP querying ");
            if (opts.NumberOfEntries == int.MaxValue)
            {
                Console.Write("all");
            }
            else
            {
                Console.Write(opts.NumberOfEntries);
            }

            Console.Write($" entries, starting at entry index {opts.StartOffset}");

            IPNetwork onlyNetwork = null;
            if (!string.IsNullOrWhiteSpace(opts.Network) && IPNetwork.TryParse(opts.Network, out onlyNetwork))
            {
                Console.Write($" and being inside network {onlyNetwork}");
            }
            else
            {
                onlyNetwork = null;
            }

            Console.WriteLine(":");

            foreach (var pair in pairsSlicedForOptions)
            {
                if ((onlyNetwork != null) && !onlyNetwork.Contains(pair.Value.First().Address))
                {
                    //Console.WriteLine();
                    //Console.WriteLine($"Skipping link details for pair {pair.Value.First().Address} <-> {pair.Value.Last().Address}: Not inside network {onlyNetwork}");
                    continue;
                }

                var title = $"Querying link details for pair {pair.Value.First().Address} <-> {pair.Value.Last().Address} of subnet {pair.Key.Subnet}";
                Console.WriteLine();
                Console.WriteLine(new string('=', title.Length));
                Console.WriteLine(title);
                Console.WriteLine(new string('-', title.Length));

                try
                {
                    using var querier = SnmpQuerierFactory.Instance.Create(pair.Value.First().Address, querierOptions);

                    var linkDetails = querier.FetchLinkDetails(pair.Value.Last().Address.ToString());

                    OutputResult(opts, linkDetails);
                }
                catch (SnmpException ex)
                {
                    Console.Error.WriteLine($"Cannot get link details for pair {pair.Value.First().Address} <-> {pair.Value.Last().Address} (subnet {pair.Key.Subnet}): SNMP Error: {ex.Message}");
                }
                catch (HamnetSnmpException ex)
                {
                    Console.Error.WriteLine($"Cannot get link details for pair {pair.Value.First().Address} <-> {pair.Value.Last().Address} (subnet {pair.Key.Subnet}): Error: {ex.Message}");
                }
            }

            return (int)ExitCodes.Ok;
        }

        /// <summary>
        /// Execution of link details query
        /// </summary>
        /// <param name="opts">The options defining the queries.</param>
        /// <returns>The exit code to return.</returns>
        private static int RunAddAndReturnExitCode(LinkDetailsOptions opts)
        {
            log.Info("Running link details query");

            var querierOptions = CreateQuerierOptions(opts);

            using var querier = SnmpQuerierFactory.Instance.Create(opts.HostsOrAddresses.First(), querierOptions);

            var wirelessPeerInfos = querier.FetchLinkDetails(opts.HostsOrAddresses.Skip(1).ToArray());

            OutputResult(opts, wirelessPeerInfos);

            return (int)ExitCodes.Ok;
        }

        /// <summary>
        /// Execution of wireless peers query
        /// </summary>
        /// <param name="opts">The options defining the queries.</param>
        /// <returns>The exit code to return.</returns>
        private static int RunAddAndReturnExitCode(WirelessPeersOptions opts)
        {
            log.Info("Running wireless peers query");

            var querierOptions = CreateQuerierOptions(opts);

            foreach (string address in opts.HostsOrAddresses)
            {
                using var querier = SnmpQuerierFactory.Instance.Create(address, querierOptions);

                var wirelessPeerInfos = querier.WirelessPeerInfos;

                OutputResult(opts, wirelessPeerInfos);
            }

            return (int)ExitCodes.Ok;
        }

        /// <summary>
        /// Create the querier options from the command line options.
        /// </summary>
        /// <param name="opts">The command line options.</param>
        /// <returns>The querier options according to the command line options.</returns>
        private static IQuerierOptions CreateQuerierOptions(GlobalOptions opts)
        {
            QuerierOptions returnOptions = QuerierOptions.Default;

            if (opts.SnmpVersion.HasValue)
            {
                returnOptions = returnOptions.WithProtocolVersion(opts.SnmpVersion.Value.ToSnmpVersion()).WithCaching(opts.UseCaching);
            }

            return returnOptions;
        }

        /// <summary>
        /// Execution of system data query
        /// </summary>
        /// <param name="opts">The options defining the queries.</param>
        /// <returns>The exit code to return.</returns>
        private static int RunAddAndReturnExitCode(SystemDataOptions opts)
        {
            log.Info("Running SystemData query");

            var querierOptions = CreateQuerierOptions(opts);

            foreach (string address in opts.HostsOrAddresses)
            {
                using var querier = SnmpQuerierFactory.Instance.Create(address, querierOptions);

                var systemData = querier.SystemData;

                OutputResult(opts, systemData);
            }

            return (int)ExitCodes.Ok;
        }

        /// <summary>
        /// Execution of interface data query
        /// </summary>
        /// <param name="opts">The options defining the queries.</param>
        /// <returns>The exit code to return.</returns>
        private static int RunAddAndReturnExitCode(InterfaceDataOptions opts)
        {
            log.Info("Running SystemData query");

            var querierOptions = CreateQuerierOptions(opts);

            foreach (string address in opts.HostsOrAddresses)
            {
                using var querier = SnmpQuerierFactory.Instance.Create(address, querierOptions);

                var interfaceData = querier.NetworkInterfaceDetails;

                OutputResult(opts, interfaceData);
            }

            return (int)ExitCodes.Ok;
        }

        /// <summary>
        /// Outputs the result in the selected format.
        /// </summary>
        /// <param name="options">The global options (which finally decide about the output format).</param>
        /// <param name="result">The result to print.</param>
        private static void OutputResult(GlobalOptions options, IHamnetSnmpQuerierResult result)
        {
            ILazyEvaluated resultAsLazyEval = result as ILazyEvaluated;
            if (resultAsLazyEval != null)
            {
                resultAsLazyEval.ForceEvaluateAll();
            }

            Console.Out.WriteLine(new BlockTextFormatter().Format(result));

            if (options.PrintStats)
            {
                Console.Out.WriteLine();
                Console.Out.WriteLine(SnmpAbstraction.PerformanceCounter.ToString());
            }
        }

        /// <summary>
        /// Initializes the backing fields for the version-specific properties.
        /// </summary>
        private static void InitBackings()
        {
            var assembly = Assembly.GetAssembly(typeof(Program));

            libraryInformationalVersionBacking = (assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).FirstOrDefault() as AssemblyInformationalVersionAttribute)
                ?.InformationalVersion;

            var product = (assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false).FirstOrDefault() as AssemblyProductAttribute)
                ?.Product;

            GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).Info($"{product} v '{libraryInformationalVersionBacking}'");
        }
    }
}
