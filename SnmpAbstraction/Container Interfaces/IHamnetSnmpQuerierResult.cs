using System.IO;
using SnmpSharpNet;

namespace SnmpAbstraction
{
    /// <summary>
    /// Marker interface for all result containers returned by a <see cref="IHamnetSnmpQuerier" />.
    /// </summary>
    public interface IHamnetSnmpQuerierResult
    {
        /// <summary>
        /// Gets the address of the device that produced this result.
        /// </summary>
        IpAddress DeviceAddress { get; }

        /// <summary>
        /// Prints the result to the given <see cref="TextWriter" /> using
        /// human-readable formatting.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        void ToTextWriter(TextWriter writer);
    }
}