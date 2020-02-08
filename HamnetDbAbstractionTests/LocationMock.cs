using HamnetDbAbstraction;

namespace HamnetDbAbstractionTests
{
    internal class LocationMock : ILocation
    {
        public LocationMock(double latitude, double longitude, double altitude)
        {
            this.Altitude = altitude;
            this.Longitude = longitude;
            this.Latitude = latitude;
            this.GroundAboveSeaLevel = altitude;
            this.Elevation = double.NaN;
        }

        public LocationMock(double latitude, double longitude, double groundAboveSeaLevel, double elevation)
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