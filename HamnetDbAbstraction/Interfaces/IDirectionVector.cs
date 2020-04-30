namespace HamnetDbAbstraction
{
    /// <summary>
    /// Interface to the data of a direction vector (e.g. between two locations)
    /// with bearing, elevation and distance.
    /// </summary>
    public interface IDirectionVector
    {
        /// <summary>
        /// Gets the point where the direction starts from.
        /// </summary>
        ILocation From { get; }

        /// <summary>
        /// Gets the point where the direction points to.
        /// </summary>
        ILocation To { get; }

        /// <summary>
        /// Gets the bearing from <see cref="From" /> to <see cref="To" /> in degrees (0/360 = north).
        /// </summary>
        double Bearing { get; }

        /// <summary>
        /// Gets the elevation from <see cref="From" /> to <see cref="To" /> in degrees (positive = up, negative = down).
        /// </summary>
        double Elevation { get; }

        /// <summary>
        /// Gets the distance between <see cref="From" /> and <see cref="To" /> in meters.
        /// </summary>
        double Distance { get; }
    }
}