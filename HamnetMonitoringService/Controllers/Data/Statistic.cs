﻿using System.Collections.Generic;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Container for some database statistic.
    /// </summary>
    internal class Statistic : Dictionary<string, string>, IStatistic
    {
        /// <summary>
        /// Constructs for a given databse name.
        /// </summary>
        public Statistic()
        {
        }

        /// <summary>
        /// Copy-construct from the given collection.
        /// </summary>
        public Statistic(IEnumerable<KeyValuePair<string, string>> collection) : base(collection)
        {
        }
    }
}
