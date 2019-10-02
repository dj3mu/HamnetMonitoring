using System.Collections.Generic;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Container for some configuration info.
    /// </summary>
    internal class ConfigurationInfo : Dictionary<string, string>, IConfigurationInfo
    {
        private readonly Dictionary<string, string> stats = new Dictionary<string, string>();

        /// <summary>
        /// Constructs for a given configuration name.
        /// </summary>
        public ConfigurationInfo()
        {
        }

        /// <summary>
        /// Copy-construct from the given collection.
        /// </summary>
        public ConfigurationInfo(IEnumerable<KeyValuePair<string, string>> collection) : base(collection)
        {
        }
    }
}
