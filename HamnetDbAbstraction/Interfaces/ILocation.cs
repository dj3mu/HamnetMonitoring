namespace HamnetDbAbstraction
{
    /// <summary>
    /// Interface to the data for a single location of latitude, longitude and altitude.
    /// </summary>
    public interface ILocation
    {
        /// <summary>
        /// Gets the latitude of the location.
        /// </summary>
        double Latitude { get; }

        /// <summary>
        /// Gets the longitude of the location.
        /// </summary>
        double Longitude { get; }

        /// <summary>
        /// Gets the height above sea level of the ground of the site in meters.
        /// </summary>
        double GroundAboveSeaLevel { get; }

        /// <summary>
        /// Gets the height of the antenna of the site in meters, relative to <see cref="GroundAboveSeaLevel" />.
        /// </summary>
        double Elevation { get; }

        /// <summary>
        /// Gets the absolute altitude of the location in meters relative to medium sea level.
        /// </summary>
        /// <value>
        /// <p>This value will be the sum of <see cref="GroundAboveSeaLevel"/> + <see cref="Elevation"/>.</p>
        /// <p>If <see cref="Elevation"/> is NaN it will be same as <see cref="GroundAboveSeaLevel"/>.</p>
        /// <p>If <see cref="GroundAboveSeaLevel"/> is NaN or &lt;= 0 it will be NaN.</p>
        /// </value>
        double Altitude { get; }
    }
}