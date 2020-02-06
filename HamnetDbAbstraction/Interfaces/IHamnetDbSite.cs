namespace HamnetDbAbstraction
{
    /// <summary>
    /// Interface to the Hamnet DB data of a single Hamnet site.
    /// </summary>
    public interface IHamnetDbSite : IHamnetDbBaseData
    {
        /// <summary>
        /// Gets the latitude of the site.
        /// </summary>
        double Latitude { get; }

        /// <summary>
        /// Gets or sets the longitude of the site.
        /// </summary>
        double Longitude { get; }

        /// <summary>
        /// Gets or sets the height above sea level of the ground of the site.
        /// </summary>
        double GroundAboveSeaLevel { get; }

        /// <summary>
        /// Gets or sets the height of the antenna of the site, relative to <see cref="GroundAboveSeaLevel" />.
        /// </summary>
        double Elevation { get; }

        /// <summary>
        /// Gets or sets the callsign of the site.
        /// </summary>
        string Callsign { get; }

        /// <summary>
        /// Gets or sets the verbose name of the site (location description - not callsign).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets a comment associated with this site.
        /// </summary>
        string Comment { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this site is inactive.
        /// </summary>
        bool Inactive { get; }
    }
}
