using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SnmpAbstraction
{
    /// <summary>
    /// Extensions to the <see cref="IEnumerable" /> type.
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
#pragma warning disable IDE0052 // for future use
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#pragma warning restore

        /// <summary>
        /// Logarithmically sums the element of the given enumeration.
        /// </summary>
        /// <param name="singleValues">The enumeration of the single values.</param>
        /// <returns>A byte array with the hex values of the string.</returns>
        public static double DecibelLogSum(this IEnumerable<double> singleValues)
        {
            if (singleValues == null)
            {
                throw new ArgumentNullException(nameof(singleValues), "The single values list to logarithmically sum up is null");
            }

            var linearSum = singleValues.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).Aggregate(0.0, (aggregated, current) =>
            {
                return aggregated += Math.Pow(10.0, current / 10.0);
            });

            return Math.Log10(linearSum) * 10.0;
        }

        /// <summary>
        /// Logarithmically sums the element of the given enumeration.
        /// </summary>
        /// <param name="singleValues">The enumeration of the single values.</param>
        /// <returns>A byte array with the hex values of the string.</returns>
        public static double DecibelLogSum(this IEnumerable<int> singleValues)
        {
            if (singleValues == null)
            {
                throw new ArgumentNullException(nameof(singleValues), "The single values list to logarithmically sum up is null");
            }

            return singleValues.Select(v => Convert.ToDouble(v)).DecibelLogSum();
        }
    }
}
