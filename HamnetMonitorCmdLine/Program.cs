﻿namespace HamnetMonitorCmdLine
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using CommandLine;
    using log4net;
    using SnmpAbstraction;

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
                XmlDocument log4netConfig = new XmlDocument();
                log4netConfig.Load(File.OpenRead(Log4netConfigurationFile));
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

            return CommandLine.Parser.Default.ParseArguments<SystemDataOptions>(args)
                .MapResult(
                    (SystemDataOptions opts) => RunAddAndReturnExitCode(opts),
                    errs => (int)ExitCodes.InvalidCommandLine);
        }

        /// <summary>
        /// Execution of Abstracted Single Queries
        /// </summary>
        /// <param name="opts">The options defining the queries.</param>
        /// <returns>The exit code to return.</returns>
        private static int RunAddAndReturnExitCode(SystemDataOptions opts)
        {
            log.Info("Running SystemData query");

            foreach (string address in opts.HostsOrAddresses)
            {
                var querier = SnmpQuerierFactory.Instance.Create(address);

                var systemData = querier.SystemData;

                OutputResult(opts, systemData);
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

            result.ToTextWriter(Console.Out);
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
