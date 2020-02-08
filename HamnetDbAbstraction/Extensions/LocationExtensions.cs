using System;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Extension methods for the <see cref="ILocation" /> and derivates.
    /// </summary>
    public static class LocationExtensions
    {
        /// <summary>
        /// The circumference of earth in meters.
        /// </summary>
        public const double CircumferenceOfEarth = 40075000.0;

        /// <summary>
        /// The circumference of earth in meters per second.
        /// </summary>
        public const double SpeedOfLight = 299792458.0;

        /// <summary>
        /// Gets the direction from this location to another location.
        /// </summary>
        /// <param name="from">This starting point location.</param>
        /// <param name="to">The destination location.</param>
        /// <returns>The distance vector between the two locations.</returns>
        public static IDirectionVector DirectionTo(this ILocation from, ILocation to)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from), "The from location is null");
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to), "The to location is null");
            }

            // computing distance only once and passing it to elevation to optimize performance
            var distance = from.HaversineDistanceTo(to);

            return new DirectionVector(
                from,
                to,
                distance,
                from.GreatCircleBearingTo(to),
                from.ElevationTo(to, distance));
        }

        /// <summary>
        /// Gets the Haversine distance between this location and the another location (in meters).
        /// </summary>
        /// <param name="from">This starting point location.</param>
        /// <param name="to">The destination location.</param>
        /// <returns>The distance between the two locations in meters.</returns>
        public static double HaversineDistanceTo(this ILocation from, ILocation to)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from), "The from location is null");
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to), "The to location is null");
            }
            
            var fromLatRad = from.Latitude.ToRadian();
            var fromLonRad = from.Longitude.ToRadian();

            var toLatRad = to.Latitude.ToRadian();
            var toLonRad = to.Longitude.ToRadian();

            var theta = fromLonRad - toLonRad;

            var distance = Math.Acos((Math.Sin(fromLatRad) * Math.Sin(toLatRad)) + (Math.Cos(fromLatRad) * Math.Cos(toLatRad) * Math.Cos(theta))).ToDegrees() * (CircumferenceOfEarth / 360.0);

            // Add the diagonal component using pythagoras (even if diff minimal in most of our cases)
            var altitudeDifference = Math.Abs(from.Altitude - to.Altitude);

            distance = Math.Sqrt((altitudeDifference * altitudeDifference) + (distance * distance));

	        return distance;
        }

        /// <summary>
        /// Gets the great circle bearing (Rhumb line) from this location to another location in degress (0/360 = north).
        /// </summary>
        /// <param name="from">This starting point location.</param>
        /// <param name="to">The destination location.</param>
        /// <returns>The bearing between the two locations.</returns>
        public static double GreatCircleBearingTo(this ILocation from, ILocation to)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from), "The from location is null");
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to), "The to location is null");
            }

            var fromLatRad = from.Latitude.ToRadian();
            var fromLonRad = from.Longitude.ToRadian();

            var toLatRad = to.Latitude.ToRadian();
            var toLonRad = to.Longitude.ToRadian();

        	// difference in longitudinal coordinates
            var lonDiff = toLonRad - fromLonRad;

	        // difference in the phi of latitudinal coordinates
            var latPhiDiff = Math.Log(
                Math.Tan((toLatRad / 2.0) + (Math.PI / 4.0)) / Math.Tan((fromLatRad / 2.0) + (Math.PI / 4.0)));
 
        	// we need to recalculate lonDiff if it is greater than pi
            if( Math.Abs(lonDiff) > Math.PI )
            {
                if(lonDiff > 0)
                {
                    lonDiff = ((2.0 * Math.PI) - lonDiff) * -1.0;
                }
                else
                {
                    lonDiff = (2.0 * Math.PI) + lonDiff;
                }
            }

            // return the angle, normalized
            return ( Math.Atan2(lonDiff, latPhiDiff).ToDegrees() + 360.0 ) % 360.0;
        }

        /// <summary>
        /// Gets the elevation from this location to another location (positive = up).
        /// </summary>
        /// <param name="from">This starting point location.</param>
        /// <param name="to">The destination location.</param>
        /// <returns>The distance vector between the two locations.</returns>
        public static double ElevationTo(this ILocation from, ILocation to)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from), "The from location is null");
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to), "The to location is null");
            }

          	return from.ElevationTo(to, from.HaversineDistanceTo(to));
        }

        /// <summary>
        /// Gets the elevation from this location to another location (positive = up).
        /// </summary>
        /// <param name="from">This starting point location.</param>
        /// <param name="to">The destination location.</param>
        /// <param name="distance">The distance between the two locations.</param>
        /// <returns>The distance vector between the two locations.</returns>
        public static double ElevationTo(this ILocation from, ILocation to, double distance)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from), "The from location is null");
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to), "The to location is null");
            }

          	return Math.Atan2(to.Altitude - from.Altitude, distance).ToDegrees();
        }

        /// <summary>
        /// Gets the free space pathloss between this location and another location on the given frequency.
        /// </summary>
        /// <param name="from">This starting point location.</param>
        /// <param name="to">The destination location.</param>
        /// <param name="frequency">The frequency to calculate the pathloss for in Hertz.</param>
        /// <param name="distance">The distance between the two locations.</param>
        /// <returns>The distance vector between the two locations.</returns>
        public static double FreeSpacePathloss(this ILocation from, ILocation to, double frequency, double distance)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from), "The from location is null");
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to), "The to location is null");
            }

          	return 20.0 * Math.Log10(((4.0 * Math.PI) / SpeedOfLight) * distance * frequency);
        }

        /// <summary>
        /// Gets the free space pathloss between this location and another location on the given frequency.
        /// </summary>
        /// <param name="from">This starting point location.</param>
        /// <param name="to">The destination location.</param>
        /// <param name="frequency">The frequency to calculate the pathloss for in Hertz.</param>
        /// <returns>The distance vector between the two locations.</returns>
        public static double FreeSpacePathloss(this ILocation from, ILocation to, double frequency)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from), "The from location is null");
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to), "The to location is null");
            }

          	return from.FreeSpacePathloss(to, frequency, from.HaversineDistanceTo(to));
        }

        /// <summary>
        /// Container for a direction vector.
        /// </summary>
        private class DirectionVector : IDirectionVector
        {
            public DirectionVector(ILocation from, ILocation to, double distance, double bearing, double elevation)
            {
                this.From = from;
                this.To = to;
                this.Distance = distance;
                this.Bearing = bearing;
                this.Elevation = elevation;
            }

            /// <inheritdoc />
            public ILocation From { get; }

            /// <inheritdoc />
            public ILocation To { get; }

            /// <inheritdoc />
            public double Bearing { get; }

            /// <inheritdoc />
            public double Elevation { get; }

            /// <inheritdoc />
            public double Distance { get; }
        }
    }
}