using System;
using System.Collections.Generic;
using System.Linq;
using HamnetDbAbstraction;

namespace HamnetMonitoringService
{
    /// <summary>
    /// Class for computing the ILocations of the rings of the fresnel zone for the given IDirectionVector.
    /// </summary>
    /// <remarks>
    /// <p>This algorithm is used with friendly permission of Rob Gonggrijp of the Freifunk project.</p>
    /// <p>The original PHP version can be found at <see href="https://rop.nl/freifunk/line-of-sight.php.txt" />
    /// and a detailed description at <see href="https://wiki.freifunk.net/Berlin:Line-of-Sight_visualiser" />.</p>
    /// <p>The original PHP code is included in comments.</p>
    /// </remarks>
    internal class FresnelPolygonsCreator
    {
        private IDirectionVector directionVector;

        /// <summary>
        /// Initialize for a specific IDirectionVector.
        /// </summary>
        /// <param name="directionVector">The vector to plot the rings for.</param>
        public FresnelPolygonsCreator(IDirectionVector directionVector)
        {
            this.directionVector = directionVector ?? throw new ArgumentNullException(nameof(directionVector), "directionVector is null");
        }

        internal IReadOnlyList<IReadOnlyList<ILocation>> CreateRingLocations(double frequency, int stepsInCircles)
        {
            // How many degrees is a meter?
            // $lat_meter = 1 / ( CIRCUMFERENCE_OF_EARTH / 360 );
            var latitudeMeterPerDegree = 1.0 / (LocationExtensions.CircumferenceOfEarth / 360.0);

            // $lon_meter = (1 / cos(deg2rad($from['lat']))) * $lat_meter;
            var longitudeMeterPerDegree = (1.0 / Math.Cos(directionVector.From.Latitude.ToRadian())) * latitudeMeterPerDegree;

            // $distance = distance($from, $to);
            // $bearing = bearing($from, $to);
	        // $wavelen = SPEED_OF_LIGHT / $freq;  // Speed of light
            var wavelength = LocationExtensions.SpeedOfLight / frequency; // $wavelen

            // $steps_in_path is an array of values between 0 (at $from) and 1 (at $to)
            // These are the distances where new polygons are started to show elipse

            // First we do that at some fixed fractions of path
        	// $steps_in_path = array(0,0.25,0.4);
            var stepsInPath = new List<double> { 0, 0.25, 0.4 }; // $steps_in_path

            // Then we add some steps set in meters because that looks better at the ends of the beam
	        // foreach (array(0.3,1,2,4,7,10,20,40,70,100) as $meters) {
            foreach (var meters in new[] { 0.3, 1.0, 2.0, 4.0, 7.0, 10.0, 20.0, 40.0, 70.0, 100.0 })
            {
                // calculate fraction of path
                // $steps_in_path[] = $meters / $distance;
                stepsInPath.Add(meters / directionVector.Distance);
            }

            // Add the reverse of these steps on other side of beam
	        // $temp = $steps_in_path;
            var temp = new List<double>(stepsInPath);
        	
            // foreach ($temp as $step) {
            foreach (var step in temp)
            {
        		// $steps_in_path[] = 1 - $step;
                stepsInPath.Add(1 - step);
            }

            // Sort and remove duplicates
            var distinctSteps = stepsInPath.OrderBy(p => p).Distinct();

            List<List<LocationVector>> rings = new List<List<LocationVector>>();
            // Fill array $rings with arrays that each hold a ring of points surrounding the beam
            foreach (var step in distinctSteps)
            {
                // $centerpoint['lat'] = $from['lat'] + ( ($to['lat'] - $from['lat']) * $step );
                // $centerpoint['lon'] = $from['lon'] + ( ($to['lon'] - $from['lon']) * $step );
                // $centerpoint['alt'] = $from['alt'] + ( ($to['alt'] - $from['alt']) * $step );
                ILocation centerpoint = new LocationVector(
                    directionVector.From.Latitude + ((directionVector.To.Latitude - directionVector.From.Latitude) * step ),
                    directionVector.From.Longitude + ((directionVector.To.Longitude - directionVector.From.Longitude) * step ),
                    directionVector.From.Altitude + ((directionVector.To.Altitude - directionVector.From.Altitude) * step ));

                // Fresnel radius calculation
                // $d1 = $distance * $step;
                var d1 = directionVector.Distance * step;

                // $d2 = $distance - $d1;
                var d2 = directionVector.Distance - d1;

                // $radius = sqrt(($wavelen * $d1 * $d2) / $distance );
                var radius = Math.Sqrt((wavelength * d1 * d2) / directionVector.Distance);

                // Bearing of line perpendicular to bearing of line of sight.
                // $ring_bearing = $bearing + 90 % 360;
                var ringBearing = (directionVector.Bearing + 90.0) % 360.0;
                var cosRingBearing = Math.Cos(ringBearing.ToRadian());
                var sinRingBearing = Math.Sin(ringBearing.ToRadian());

                List<LocationVector> ring = new List<LocationVector>(stepsInCircles);
                //for ($n = 0; $n < $steps_in_circles; $n++) {
                for (int n = 0; n < stepsInCircles; ++n)
                {
                    //$angle = $n * (360 / $steps_in_circles );
                    var angle = n * (360.0 / stepsInCircles);
                    var radianAngle = angle.ToRadian();
                    
                    // $vertical_factor = cos(deg2rad($angle));
                    var verticalFactor = Math.Cos(radianAngle);

                    // $horizontal_factor = sin(deg2rad($angle));
                    var horizontalFactor = Math.Sin(radianAngle);
                    
                    // $lat_factor = cos(deg2rad($ring_bearing)) * $horizontal_factor;
                    var latitudeFactor = cosRingBearing * horizontalFactor;

                    // $lon_factor = sin(deg2rad($ring_bearing)) * $horizontal_factor;
                    var longitudeFactor = sinRingBearing * horizontalFactor;

                    // $new_point['lat'] = $centerpoint['lat'] + ($lat_factor * $lat_meter * $radius);
                    // $new_point['lon'] = $centerpoint['lon'] + ($lon_factor * $lon_meter * $radius);
                    // $new_point['alt'] = $centerpoint['alt'] + ($vertical_factor * $radius);
                    var newPoint = new LocationVector(
                        centerpoint.Latitude + (latitudeFactor * latitudeMeterPerDegree * radius),
                        centerpoint.Longitude + (longitudeFactor * longitudeMeterPerDegree * radius),
                        centerpoint.Altitude + (verticalFactor * radius));

                    // $ring[] = $new_point;
                    ring.Add(newPoint);
                }

                // $rings[] = $ring;
                rings.Add(ring);
            }

            return rings;
        }

        /// <summary>
        /// Container for a location vector.
        /// </summary>
        private class LocationVector : ILocation
        {
            public LocationVector(double latitude, double longitude, double altitude)
            {
                this.Altitude = altitude;
                this.Longitude = longitude;
                this.Latitude = latitude;
                this.GroundAboveSeaLevel = altitude;
                this.Elevation = double.NaN;
            }

            public LocationVector(double latitude, double longitude, double groundAboveSeaLevel, double elevation)
            {
                this.Altitude = groundAboveSeaLevel + (double.IsNaN(elevation) ? 0.0 : elevation);
                this.Longitude = longitude;
                this.Latitude = latitude;
                this.GroundAboveSeaLevel = groundAboveSeaLevel;
                this.Elevation = elevation;
            }

            /// <inheritdoc />
            public double Latitude { get; }

            /// <inheritdoc />
            public double Longitude { get; }

            /// <inheritdoc />
            public double Altitude { get; }

            /// <inheritdoc />
            public double GroundAboveSeaLevel { get; }

            /// <inheritdoc />
            public double Elevation { get; }
        }
    }
}