using System;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Extension methods for all number types (int, float, double, decimal, ...)
    /// </summary>
    public static class NumericExtensions
    {
        /// <summary>
        /// Interprets the given double value as degrees and converts it to the corresponding radian value.
        /// </summary>
        /// <param name="degrees">The value in degrees.</param>
        /// <returns>The value in radian.</returns>
        public static double ToRadian(this double degrees)
        {
            double radian = Math.PI * degrees / 180.0;

            return radian;
        }

        /// <summary>
        /// Interprets the given double value as radian and converts it to the corresponding degree value.
        /// </summary>
        /// <param name="radian">The value in radian.</param>
        /// <returns>The value in radian.</returns>
        public static double ToDegrees(this double radian)
        {
            double degrees = radian * 180.0 / Math.PI;

            return degrees;
        }
    }
}