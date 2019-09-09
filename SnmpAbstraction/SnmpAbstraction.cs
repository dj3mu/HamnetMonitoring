using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using log4net;
using SnmpSharpNet;

[assembly: InternalsVisibleTo("SnmpAbstractionTests")]
namespace SnmpAbstraction
{
    /// <summary>
    /// Information and entry-point class to the SnmpAbstraction.
    /// </summary>
    public static class SnmpAbstraction
    {
        private static readonly string Log4netConfigurationFile = "Config/log4net.config";

        /// <summary>
        /// Regex for indenting (capturing start of line).
        /// </summary>
        private static readonly Regex IndentationRegex = new Regex(@"^", RegexOptions.Compiled | RegexOptions.Multiline);

        private static Version libraryVersionBacking = null;

        private static string libraryIdBacking = null;

        private static ILog log = null;
        
        private static string libraryInformationalVersionBacking = null;
        
        private static PerformanceCounter performanceCounterBacking = new PerformanceCounter();

        /// <summary>
        /// Gets the performance counter.
        /// </summary>
        public static IPerformanceCounter PerformanceCounter => performanceCounterBacking;

        /// <summary>
        /// Records an SNMP in performance counter.
        /// </summary>
        /// <param name="destinationAddress">The destination IP address that this request has been sent to.</param>
        /// <param name="request">The request.</param>
        /// <param name="result">The result.</param>
        internal static void RecordSnmpRequest(IpAddress destinationAddress, Pdu request, SnmpPacket result)
        {
            performanceCounterBacking.Record(destinationAddress, request, result);
        }

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
            var assembly = Assembly.GetAssembly(typeof(SnmpAbstraction));

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