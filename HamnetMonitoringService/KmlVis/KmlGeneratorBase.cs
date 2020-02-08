using SharpKml.Dom;

namespace HamnetMonitoringService
{
    /// <summary>
    /// Generator class for creating a KML file for visualization of a single link.
    /// </summary>
    internal abstract class KmlGeneratorBase : IKmlGenerator
    {
        /// <inheritdoc />
        public abstract Kml GenerateKmlObject();

        /// <inheritdoc />
        public string GenerateString()
        {
            var kml = this.GenerateKmlObject();
            
            var serializer = new SharpKml.Base.Serializer();
            serializer.Serialize(kml);

            return serializer.Xml;
        }
    }
}