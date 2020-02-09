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
    internal static class KmlCommonElements
    {
        private const string PlacementCircleStyleMapReferenceName = "msn_placemark_circle";

        private const string PlacementCircleStyleReferenceName = "sn_placemark_circle";

        private const string PlacementCircleHighlightStyleReferenceName = "sh_placemark_circle_highlight";

        private const string PolygonStyleReferenceName = "polygon";

        private const string PolygonTransparentStyleReferenceName = "polygon-transparent";

        private const string LineStyleReferenceName = "line";

        /// <summary>
        /// Gets an URI referencing the PlacementCircleStyleMap.
        /// </summary>
        public static readonly Uri PlacementCircleStyleMapReferenceUri = new Uri($"#{PlacementCircleStyleMapReferenceName}", UriKind.Relative);

        /// <summary>
        /// The URI to reference the style of a placement circle.
        /// </summary>
        public static readonly Uri PlacementCircleStyleReferenceUri = new Uri($"#{PlacementCircleStyleReferenceName}", UriKind.Relative);
        
        /// <summary>
        /// The URI to reference the style of a highlighted placement circle.
        /// </summary>
        public static readonly Uri PlacementCircleHighlightStyleReferenceUri = new Uri($"#{PlacementCircleHighlightStyleReferenceName}", UriKind.Relative);

        /// <summary>
        /// The URI to reference the standard polygon style.
        /// </summary>
        public static readonly Uri PolygonStyleReferenceUri = new Uri($"#{PolygonStyleReferenceName}", UriKind.Relative);

        /// <summary>
        /// The URI to reference the standard polygon style.
        /// </summary>
        public static readonly Uri PolygonStyleTransparentReferenceUri = new Uri($"#{PolygonTransparentStyleReferenceName}", UriKind.Relative);

        /// <summary>
        /// The URI to reference the line style.
        /// </summary>
        public static readonly Uri LineStyleReferenceUri = new Uri($"#{LineStyleReferenceName}", UriKind.Relative);
        
        /// <summary>
        /// String with the text of the CSS for a ballon popup.
        /// </summary>
        /// <remarks>
        /// There is no global stylesheet in Google Earth, so this needs to be appended to each balloon to make it display nicely.
        /// </remarks>
        internal static readonly string BalloonCssString;

        /// <summary>
        /// Gets a new placement cirle style map.
        /// </summary>
        public static StyleMapCollection PlacementCircleStyleMap
        {
            get
            {
                var placementCircleStyleMap = new StyleMapCollection
                {
                    new Pair
                    {
                        State = StyleState.Normal,
                        StyleUrl = PlacementCircleStyleReferenceUri
                    },
                    new Pair
                    {
                        State = StyleState.Highlight,
                        StyleUrl = PlacementCircleHighlightStyleReferenceUri
                    }          
                };

                placementCircleStyleMap.Id = PlacementCircleStyleMapReferenceName;

                return placementCircleStyleMap;
            }
        }

        /// <summary>
        /// Gets a new polygon standard style.
        /// </summary>
        public static Style PolygonStyle
        {
            get
            {
                return new Style
                {
                    Id = PolygonStyleReferenceName,
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
            }
        }

        /// <summary>
        /// Gets a new polygon transparent style.
        /// </summary>
        public static Style PolygonStyleTransparent
        {
            get
            {
                return new Style
                {
                    Id = PolygonTransparentStyleReferenceName,
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
            }
        }

        /// <summary>
        /// Gets a new line style.
        /// </summary>
        public static Style LineStyle
        {
            get
            {
                return new Style
                {
                    Id = LineStyleReferenceName,
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
            }
        }

        /// <summary>
        /// Gets a new standard balloon style.
        /// </summary>
        public static BalloonStyle StandardBallonStyle
        {
            get
            {
                return new BalloonStyle
                {
                    Id = "balloon",
                    BackgroundColor = Color32.Parse("ff6c3adf"),
                    TextColor = new Color32(0xff, 0x00, 0x00, 0x00),
                    Text = "$[description]",
                    DisplayMode = DisplayMode.Default
                };
            }
        }

        /// <summary>
        /// Initialize the static members of the class.
        /// </summary>
        static KmlCommonElements()
        {
            BalloonCssString = @"
<style type=""text/css"">
    a:link {text-decoration: none;}
    td.left {text-align: right; vertical-align: top; margin-bottom: 5px;}
    td, h2 {white-space: nowrap; font-family: verdana;}
</style>";
        }
    }
}