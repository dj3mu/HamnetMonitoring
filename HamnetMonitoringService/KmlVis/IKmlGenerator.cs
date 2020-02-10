using SharpKml.Dom;

namespace HamnetMonitoringService
{
    /// <summary>
    /// Interface to all kinds of KML generators.
    /// </summary>
    public interface IKmlGenerator
    {
        /// <summary>
        /// Actually generates the KML data.
        /// </summary>
        /// <returns>The KML data as <see cref="SharpKml.Dom.Document" /> instance.</returns>
        Kml GenerateKmlObject();

        /// <summary>
        /// Actually generates the KML data.
        /// </summary>
        /// <returns>The KML data as a single string containing the XML data.</returns>
        string GenerateString();
    }
}