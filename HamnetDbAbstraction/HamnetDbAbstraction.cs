using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using log4net;

[assembly: InternalsVisibleTo("HamnetDbAbstractionTests")]
namespace HamnetDbAbstraction
{
    /// <summary>
    /// Information and entry-point class to the SnmpAbstraction.
    /// </summary>
    public static class HamnetDbAbstraction
    {
        /// <summary>
        /// Regex for indenting (capturing start of line).
        /// </summary>
        private static readonly Regex IndentationRegex = new Regex(@"^", RegexOptions.Compiled | RegexOptions.Multiline);

        private static string Log4netConfigurationFile = "Config/log4net.config";

        private static Version libraryVersionBacking = null;

        private static string libraryIdBacking = null;

        private static ILog log = null;
        
        private static string libraryInformationalVersionBacking = null;
        
        /// <summary>
        /// Gets the version number of the InstallationServiceLib.
        /// </summary>
        public static Version LibraryVersion
        {
            get
            {
                if (libraryVersionBacking != null)
                {
                    return libraryVersionBacking;
                }

                InitBackings();

                return libraryVersionBacking;
            }
        }

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
        /// Gets the library ID of the InstallationServiceLib.
        /// </summary>
        public static string LibraryIdVersion
        {
            get
            {
                if (!String.IsNullOrEmpty(libraryIdBacking))
                {
                    return libraryIdBacking;
                }

                InitBackings();

                return libraryIdBacking;
            }
        }

        /// <summary>
        /// Sets the configuration file for the log4net logger.
        /// </summary>
        /// <param name="configFilePath">The new configuration file.</param>
        public static void SetLoggerConfig(string configFilePath)
        {
            if (!File.Exists(configFilePath))
            {
                log.Error($"Cannot find log4net configuration file '{configFilePath}': Going ahead with previous logging configuration");
            }

            Log4netConfigurationFile = configFilePath;
            log = null; // force re-init of the logger
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
                log.Info($"{LibraryInformationalVersion} initializing");
            }

            return LogManager.GetLogger(type);
        }

        /// <summary>
        /// Performs indentation of every line of the input string.
        /// </summary>
        /// <param name="input">The string of which to indent every line.</param>
        /// <returns>String with the lines indented.</returns>
        internal static string IndentLines(string input)
        {
            return IndentationRegex.Replace(input, "  ");
        }

        /// <summary>
        /// Initializes the backing fields for the version-specific properties.
        /// </summary>
        private static void InitBackings()
        {
            var assembly = Assembly.GetAssembly(typeof(HamnetDbAbstraction));

            libraryVersionBacking = assembly.GetName().Version;

            libraryInformationalVersionBacking = (assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false).FirstOrDefault() as AssemblyInformationalVersionAttribute)
                ?.InformationalVersion;

            var product = (assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false).FirstOrDefault() as AssemblyProductAttribute)
                ?.Product;

            libraryIdBacking = $"{product} v {LibraryVersion} ({libraryInformationalVersionBacking})";

            GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).Info($"Determined Library Information Version: '{libraryIdBacking}'");
        }
    }
}