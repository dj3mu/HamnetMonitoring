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
    /// <p>The styles and algorithms are used with friendly permission of Rob Gonggrijp of the Freifunk project.</p>
    /// <p>The original PHP version can be found at <see href="https://rop.nl/freifunk/line-of-sight.php.txt" />
    /// and a detailed description at <see href="https://wiki.freifunk.net/Berlin:Line-of-Sight_visualiser" />.</p>
    /// <p>The original PHP code is included in comments.</p>
    /// </remarks>
    internal class SingleLinkViewKmlGenerator : KmlGeneratorBase
    {
        internal static readonly Uri PlacementCircleStyleMapReferenceUri;

        internal static Uri PolygonStyleReferenceUri;

        internal static readonly Uri LineStyleReferenceUri;
        
        internal static readonly Uri PolygonStyleTransparentReferenceUri;

        private static readonly Style LineStyle;
        private static readonly Style PolygonStyleTransparent;
        private static readonly Style PolygonStyle;
        private static readonly StyleMapCollection PlacementCircleStyleMap;
        private static readonly BalloonStyle StandardBallonStyle;

        /// <summary>
        /// Initialize the static members of the class.
        /// </summary>
        static SingleLinkViewKmlGenerator()
        {
            StandardBallonStyle = new BalloonStyle
            {
                Id = "balloon",
                BackgroundColor = Color32.Parse("ff6c3adf"),
                TextColor = new Color32(0xff, 0x00, 0x00, 0x00),
                Text = "$[description]",
                DisplayMode = DisplayMode.Default
            };        

            PlacementCircleStyleMap = new StyleMapCollection
            {
                new Pair
                {
                    State = StyleState.Normal,
                    StyleUrl = new Uri("#sn_placemark_circle", UriKind.Relative)
                },
                new Pair
                {
                    State = StyleState.Highlight,
                    StyleUrl = new Uri("#sh_placemark_circle_highlight", UriKind.Relative)
                }          
            };

            PlacementCircleStyleMap.Id = "msn_placemark_circle";
            PlacementCircleStyleMapReferenceUri = new Uri($"#{PlacementCircleStyleMap.Id}", UriKind.Relative);

            PolygonStyle = new Style
            {
                Id = "polygon",
                Line = new LineStyle
                {
                    Width = 1.0,
                    Color = new Color32(0xff, 0x00, 0x60, 0x00)
                },
                Polygon = new PolygonStyle
                {
                    Color = new Color32(0xff, 0x50, 0xff, 0x50)
                },
                Balloon = StandardBallonStyle
            };
            
            PolygonStyleReferenceUri = new Uri($"#{PolygonStyle.Id}", UriKind.Relative);

            PolygonStyleTransparent = new Style
            {
                Id = "polygon-transparent",
                Line = new LineStyle
                {
                    Width = 1.0,
                    Color = new Color32(0x40, 0x00, 0xff, 0x00)
                },
                Polygon = new PolygonStyle
                {
                    Color = new Color32(0x80, 0x00, 0xff, 0x00)
                },
                Balloon = StandardBallonStyle
            };

            PolygonStyleTransparentReferenceUri = new Uri($"#{PolygonStyleTransparent.Id}", UriKind.Relative);

            LineStyle = new Style
            {
                Id = "line",
                Line = new LineStyle
                {
                    Width = 1.0,
                    Color = new Color32(0xff, 0x00, 0x00, 0x00)
                },
                Polygon = new PolygonStyle
                {
                    Color = new Color32(0xa0, 0xff, 0xff, 0xff)
                },
                Balloon = StandardBallonStyle
            };

            LineStyleReferenceUri = new Uri($"#{LineStyle.Id}", UriKind.Relative);
        }

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
            this.To = to ?? throw new ArgumentNullException(nameof(to), "to site is null");
            this.From = from ?? throw new ArgumentNullException(nameof(from), "from site is null");

            if (double.IsNaN(this.From.Altitude) || !double.IsFinite(this.From.Altitude))
            {
                throw new ArgumentOutOfRangeException(nameof(to), "The Altitude property of the From location is NaN or inifite");
            }

            if (double.IsNaN(this.From.Latitude) || !double.IsFinite(this.From.Latitude))
            {
                throw new ArgumentOutOfRangeException(nameof(to), "The Latitude property of the From location is NaN or inifite");
            }

            if (double.IsNaN(this.From.Longitude) || !double.IsFinite(this.From.Longitude))
            {
                throw new ArgumentOutOfRangeException(nameof(to), "The Longitude property of the From location is NaN or inifite");
            }

            if (double.IsNaN(this.To.Altitude) || !double.IsFinite(this.To.Altitude))
            {
                throw new ArgumentOutOfRangeException(nameof(to), "The Altitude property of the To location is NaN or inifite");
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

            locationFolder.AddFeature(this.From.ToKmlPlacemark(PlacementCircleStyleMapReferenceUri));
            locationFolder.AddFeature(this.To.ToKmlPlacemark(PlacementCircleStyleMapReferenceUri));
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
                Name = "hamnetdb.monitoring.service"
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
                Balloon = StandardBallonStyle
            });

            document.AddStyle(new Style
            {
                Id = "sn_placemark_circle",
                Icon = new IconStyle
                {
                    Scale = 1.3,
                    Icon = new IconStyle.IconLink(new Uri("http://maps.google.com/mapfiles/kml/shapes/placemark_circle.png", UriKind.Absolute))
                },
                Balloon = StandardBallonStyle
            });

            document.AddStyle(PlacementCircleStyleMap);
            document.AddStyle(LineStyle);
            document.AddStyle(PolygonStyle);
            document.AddStyle(PolygonStyleTransparent);
        }
    }
}