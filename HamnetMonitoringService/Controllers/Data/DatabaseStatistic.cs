using System.Collections.Generic;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Container for some database statistic.
    /// </summary>
    internal class DatabaseStatistic : Dictionary<string, string>, IDatabasestatistic
    {
        private readonly Dictionary<string, string> stats = new Dictionary<string, string>();

        /// <summary>
        /// Constructs for a given databse name.
        /// </summary>
        public DatabaseStatistic()
        {
        }

        /// <summary>
        /// Copy-construct from the given collection.
        /// </summary>
        public DatabaseStatistic(IEnumerable<KeyValuePair<string, string>> collection) : base(collection)
        {
        }
    }
}
