namespace HamnetDbAbstraction
{
    /// <summary>
    /// Interface to the Hamnet DB data of a single Hamnet site.
    /// </summary>
    public interface IHamnetDbSite : IHamnetDbBaseData, ILocation
    {
        /// <summary>
        /// Gets or sets the verbose name of the site (location description - not callsign).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the callsign of the site.
        /// </summary>
        string Callsign { get; }

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
