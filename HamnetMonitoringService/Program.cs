using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HamnetDbRest
{
    /// <summary>
    /// The REST service program entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The section key for the RSSI Aquisition service configuration.
        /// </summary>
        public static readonly string RssiAquisitionServiceSectionKey = "RssiAquisitionService";

        /// <summary>
        /// The section key for the RSSI Aquisition service configuration.
        /// </summary>
        public static readonly string BgpAquisitionServiceSectionKey = "BgpAquisitionService";

        /// <summary>
        /// The section key for the Influx database configuration.
        /// </summary>
        public static readonly string InfluxSectionKey = "Influx";
    
        /// <summary>
        /// The base time for calculation of Unix Time Stamp (by subtracting this time stamp).
        /// </summary>
        public static readonly DateTime UnixTimeStampBase = new DateTime(1970, 1, 1);

        private static readonly string Log4netConfigurationFile = "Config/log4net.config";

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
        /// Gets a constant string to identify a process-wide monitor (e.g. lock during maitenance).
        /// </summary>
        public static string ProgramWideMutexName { get; } = "HamnetMonitoringService-ProcessWideMonitor";

        /// <summary>
        /// The entry method.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            var configFile = (args != null) && (args.Length > 0) && !string.IsNullOrWhiteSpace(args[0])
                ? args[0] // first command line options is the config file to use
                : "/etc/HamnetMonitoringService/appsettings.json"; // this is the fallback config file - only good for Linux

            configFile = configFile.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            culture.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
            culture.DateTimeFormat.FirstDayOfWeek = DayOfWeek.Monday;
            culture.DateTimeFormat.FullDateTimePattern = "yyyy-MM-ddTHH\\:mm\\:sszzz";
            culture.DateTimeFormat.LongTimePattern = "HH\\:mm\\:sszzz";
            culture.DateTimeFormat.ShortTimePattern = "HH:mm:ssK";
            Thread.CurrentThread.CurrentCulture = culture;

            var configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFile, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var host = new WebHostBuilder()
                .UseConfiguration(configuration)
                .UseKestrel()
                .UseStartup<Startup>()
                .ConfigureLogging((hostingContext, logging) =>
                    {
                        // The ILoggingBuilder minimum level determines the
                        // the lowest possible level for logging. The log4net
                        // level then sets the level that we actually log at.
                        logging.AddLog4Net(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Config", "log4net.config"));
#if DEBUG
                        logging.SetMinimumLevel(LogLevel.Debug);
#else
                        logging.SetMinimumLevel(LogLevel.Information);
#endif
                    })
                .Build();

            host.Run();
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
        /// Initializes the backing fields for the version-specific properties.
        /// </summary>
        private static void InitBackings()
        {
            var assembly = Assembly.GetAssembly(typeof(Program));

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
