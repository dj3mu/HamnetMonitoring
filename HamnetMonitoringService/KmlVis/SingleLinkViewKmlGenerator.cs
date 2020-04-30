using System;
using HamnetDbAbstraction;
using SharpKml.Base;
using SharpKml.Dom;

namespace HamnetMonitoringService
{
    /// <summary>
    /// Generator class for creating a KML file for visualization of a single link.
    /// </summary>
    /// <remarks>
    /// <p>The styles and algorithms in this file are used with friendly permission of Rop Gonggrijp of the Freifunk project.</p>
    /// <p>The original PHP version can be found at <see href="https://rop.nl/freifunk/line-of-sight.php.txt" />
    /// and a detailed description at <see href="https://wiki.freifunk.net/Berlin:Line-of-Sight_visualiser" />.</p>
    /// <p>The original PHP code is included in comments.</p>
    /// </remarks>
    internal class SingleLinkViewKmlGenerator : KmlGeneratorBase
    {
        /// <summary>
        /// Initializes a new instance for a link between the two given locations.
        /// </summary>
        /// <param name="from">
        /// The from site for the link.<br/>
        /// This site will also become the initial camera position.
        /// </param>
        /// <param name="to">
        /// The to site for the link.<br/>
        /// The heading to this site will be the initial camera view direction.
        /// </param>
        public SingleLinkViewKmlGenerator(IHamnetDbSite from, IHamnetDbSite to)
        {
            // a lot of work to make sure we get at least half-way consistent data
            this.To = to ?? throw new ArgumentNullException(nameof(to), "to site is null");
            this.From = from ?? throw new ArgumentNullException(nameof(from), "from site is null");

            if (double.IsNaN(this.From.Altitude) || !double.IsFinite(this.From.Altitude))
            {
                throw new ArgumentOutOfRangeException(nameof(from), "The Altitude property of the From location is NaN or inifite. Make sure both, ground above sea level _and_ elevation are set properly");
            }

            if (double.IsNaN(this.From.Latitude) || !double.IsFinite(this.From.Latitude))
            {
                throw new ArgumentOutOfRangeException(nameof(from), "The Latitude property of the From location is NaN or inifite");
            }

            if (double.IsNaN(this.From.Longitude) || !double.IsFinite(this.From.Longitude))
            {
                throw new ArgumentOutOfRangeException(nameof(from), "The Longitude property of the From location is NaN or inifite");
            }

            if (double.IsNaN(this.To.Altitude) || !double.IsFinite(this.To.Altitude))
            {
                throw new ArgumentOutOfRangeException(nameof(to), "The Altitude property of the To location is NaN or inifite. Make sure both, ground above sea level _and_ elevation are set properly");
            }

            if (double.IsNaN(this.To.Latitude) || !double.IsFinite(this.To.Latitude))
            {
                throw new ArgumentOutOfRangeException(nameof(to), "The Latitude property of the To location is NaN or inifite");
            }

            if (double.IsNaN(this.To.Longitude) || !double.IsFinite(this.To.Longitude))
            {
                throw new ArgumentOutOfRangeException(nameof(to), "The Longitude property of the To location is NaN or inifite");
            }
        }

        /// <summary>
        /// Gets the from site for the link.<br/>
        /// This site will also become the initial camera position.
        /// </summary>
        public IHamnetDbSite From { get; }

        /// <summary>
        /// Gets the to site for the link.<br/>
        /// The heading to this site will be the initial camera view direction.
        /// </summary>
        public IHamnetDbSite To { get; }

        /// <inheritdoc />
        public override Kml GenerateKmlObject()
        {
            Document document = this.StartDocument(out Kml rootElement);

            this.AddSharedStyles(document);
            this.AddLocationsFolder(document);
            this.AddLinksFolder(document);

            return rootElement;
        }

        private void AddLinksFolder(Document document)
        {
            IDirectionVector vector = this.From.DirectionTo(this.To);

            var linkFolder = new Folder
            {
                Name = "Links",
                Open = true,
                Visibility = true
            };

            document.AddFeature(linkFolder);

            linkFolder.AddFeature(vector.ToKmlLinkFolder());
        }

        private void AddLocationsFolder(Document document)
        {
            var locationFolder = new Folder
            {
                Name = "Locations"
            };

            document.AddFeature(locationFolder);

            var listStyle = new ListStyle
            {
                ItemType = ListItemType.CheckHideChildren,
            };

            listStyle.AddItemIcon(new ItemIcon
            {
                Href = new Uri("http://maps.google.com/mapfiles/kml/shapes/placemark_circle.png", UriKind.Absolute)
            });

            locationFolder.AddStyle(new Style
            {
                List = listStyle
            });

            locationFolder.AddFeature(this.From.ToKmlPlacemark(KmlCommonElements.PlacementCircleStyleMapReferenceUri));
            locationFolder.AddFeature(this.To.ToKmlPlacemark(KmlCommonElements.PlacementCircleStyleMapReferenceUri));
        }

        private Document StartDocument(out Kml rootElement)
        {
            var kml = new Kml();

            kml.AddNamespacePrefix(KmlNamespaces.GX22Prefix, KmlNamespaces.GX22Namespace);
            kml.AddNamespacePrefix(KmlNamespaces.AtomPrefix, KmlNamespaces.AtomNamespace);
            kml.AddNamespacePrefix(KmlNamespaces.Kml22Prefix, KmlNamespaces.Kml22Namespace);

            var document = new Document
            {
                Open = true,
                Name = $"Link between {this.From.Callsign?.ToUpperInvariant()} and {this.To.Callsign?.ToUpperInvariant()}"
            };

            kml.Feature = document;
            rootElement = kml;

           return document;
        }

        private void AddSharedStyles(Document document)
        {
            document.AddStyle(new Style
            {
                Id = "sh_placemark_circle_highlight",
                Icon = new IconStyle
                {
                    Scale = 1.7,
                    Icon = new IconStyle.IconLink(new Uri("http://maps.google.com/mapfiles/kml/shapes/placemark_circle_highlight.png", UriKind.Absolute))
                },
                Balloon = KmlCommonElements.StandardBallonStyle
            });

            document.AddStyle(new Style
            {
                Id = "sn_placemark_circle",
                Icon = new IconStyle
                {
                    Scale = 1.3,
                    Icon = new IconStyle.IconLink(new Uri("http://maps.google.com/mapfiles/kml/shapes/placemark_circle.png", UriKind.Absolute))
                },
                Balloon = KmlCommonElements.StandardBallonStyle
            });

            document.AddStyle(KmlCommonElements.PlacementCircleStyleMap);
            document.AddStyle(KmlCommonElements.LineStyle);
            document.AddStyle(KmlCommonElements.PolygonStyle);
            document.AddStyle(KmlCommonElements.PolygonStyleTransparent);
        }
    }
}