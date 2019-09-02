using SemVersion;

namespace SnmpAbstraction
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Parses a string as <see cref="SemanticVersion" /> and returns the result or the fallback version if the string is not parsable.
        /// </summary>
        /// <param name="versionString">The version string to parse. It must at least contain two dots (i.e. three sections).</param>
        /// <param name="fallBackVersion">The fallback version to use in case the version is not parseable.</param>
        /// <returns></returns>
        public static SemanticVersion ParseVersion(this string versionString, SemanticVersion fallBackVersion)
        {
            if (string.IsNullOrWhiteSpace(versionString))
            {
                return fallBackVersion;
            }

            SemanticVersion version;
            if (!SemanticVersion.TryParse(versionString, out version))
            {
                log.Warn($"ParseVersion: Non-parseable version string '{versionString}' found. Returning version 'null'");
                return fallBackVersion;
            }

            return version;
        }
    }
}