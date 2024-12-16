using System;
using System.Collections.Generic;
using HamnetDbAbstraction;
using SharpKml.Base;
using SharpKml.Dom;

namespace HamnetMonitoringService
{
    /// <summary>
    /// Extension methods for the types imported from HamnetDbAbstraction library.
    /// </summary>
    public static class HamnetDbTypeExtensions
    {
        /// <summary>
        /// Creates a KML placemark of the given site.
        /// </summary>
        /// <param name="site">The site to convert.</param>
        /// <param name="styleUrl">The URI of the shared style to use. Null for not to add any style URL.</param>
        /// <returns>The placemark object reflecting the site.</returns>
        public static Placemark ToKmlPlacemark(this IHamnetDbSite site, Uri styleUrl = null)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site), "site is null");
            }

            var pm = new Placemark
            {
                Name = site.Callsign?.ToUpperInvariant(),
                Id = site.Callsign?.ToUpperInvariant(), // we use callsign for unique ID - must see if that works out
                Description = new Description
                {
                    Text = $@"<h2>{site.Callsign?.ToUpperInvariant()}:</h2>
<table>
<tr><td class=""left"">latitude: </td><td>{site.Latitude}&#176;</td></tr>
<tr><td class=""left"">longitude: </td><td>{site.Longitude}&#176;</td></tr>
<tr><td class=""left"">ground asl: </td><td>{site.GroundAboveSeaLevel} m</td></tr>
<tr><td class=""left"">elevation: </td><td>{site.Elevation} m</td></tr>
<tr><td class=""left"">abs. altitude: </td><td>{site.Altitude} m</td></tr>
</table>
<p>Comment:</p>
<p>{site.Comment.Replace("\n", "<br/>")}</p>
"
                },
                Geometry = site.ToKmlPoint()
            };

            if (styleUrl != null)
            {
                pm.StyleUrl = styleUrl;
            }

            return pm;
        }

        /// <summary>
        /// Creates a KML Point of the given site.
        /// </summary>
        /// <param name="location">The location to convert.</param>
        /// <returns>The point object reflecting the site.</returns>
        public static Point ToKmlPoint(this ILocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location), "location is null");
            }

            return new Point
            {
                Coordinate = location.ToKmlVector(),
                AltitudeMode = AltitudeMode.Absolute
            };
        }

        /// <summary>
        /// Creates a KML Vector of the given site.
        /// </summary>
        /// <param name="location">The location to convert.</param>
        /// <returns>The vector object reflecting the location.</returns>
        public static Vector ToKmlVector(this ILocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location), "location is null");
            }

            return new SharpKml.Base.Vector(location.Latitude, location.Longitude, location.Altitude);
        }

        /// <summary>
        /// Creates a KML folder with the visualization settings of the link.
        /// </summary>
        /// <param name="directionVector">The direction vector to convert.</param>
        /// <returns>The KML elements for visualizing the link.</returns>
        public static Folder ToKmlLinkFolder(this IDirectionVector directionVector)
        {
            if (directionVector == null)
            {
                throw new ArgumentNullException(nameof(directionVector), "directionVector is null");
            }

            var fromAsHamnetSite = directionVector.From as IHamnetDbSite;
            var toAsHamnetSite = directionVector.To as IHamnetDbSite;
            var linkFolder = new Folder
            {
                Name = (fromAsHamnetSite != null) && (toAsHamnetSite != null)
                    ? $"Link between {fromAsHamnetSite.Callsign?.ToUpperInvariant()} and {toAsHamnetSite.Callsign?.ToUpperInvariant()}"
                    : $"Link between {directionVector.From.Latitude}/{directionVector.From.Longitude} and {directionVector.To.Latitude}/{directionVector.To.Longitude}",
                Visibility = true
            };

            var listStyle = new ListStyle
            {
                ItemType = ListItemType.CheckHideChildren,
            };

            listStyle.AddItemIcon(new ItemIcon
            {
                Href = new Uri("empty_icon.png", UriKind.Relative)
            });

            linkFolder.AddStyle(new Style
            {
                List = listStyle
            });

            linkFolder.AddFeature(CreateLinePlacemark(directionVector.From, directionVector.To, "Line with screen to ground", new Uri("#line", UriKind.Relative), true, string.Empty));

            // description will be added to the larger (outer) zone only - hence the 2.4 GHz zone
            string description = KmlCommonElements.BalloonCssString;
            description += (fromAsHamnetSite != null) && (toAsHamnetSite != null)
                    ? $"<h2>Link between {fromAsHamnetSite.Callsign?.ToUpperInvariant()} and {toAsHamnetSite.Callsign?.ToUpperInvariant()}</h2>"
                    : $"<h2>Link between {directionVector.From.Latitude}/{directionVector.From.Longitude} and {directionVector.To.Latitude}/{directionVector.To.Longitude}</h2>";

            description += $@"<table>
<tr><td class=""left"">distance: </td><td>{directionVector.Distance / 1000.0:F1} km</td></tr>
<tr><td class=""left"">azimuth to: </td><td>{directionVector.Bearing:F1}&#176;</td></tr>
<tr><td class=""left"">elevation to: </td><td>{directionVector.Elevation:F1}&#176;</td></tr>
<tr><td class=""left"">azimuth from: </td><td>{(directionVector.Bearing + 180.0) % 360.0:F3}&#176;</td></tr>
<tr><td class=""left""><a href=""https://en.wikipedia.org/wiki/Free-space_path_loss"">FSPL</a>: </td><td>{directionVector.Distance.FreeSpacePathloss(2.4e9):F1} dB @ 2.4 GHz<br>{directionVector.Distance.FreeSpacePathloss(5.8e9):F1} dB @ 5.8 GHz</td></tr>
</table>";
            linkFolder.AddFeature(CreateFresnelPlacemark(directionVector, "2.4 GHz fresnel zone", 2.4e9, KmlCommonElements.PolygonStyleTransparentReferenceUri, description));
            linkFolder.AddFeature(CreateFresnelPlacemark(directionVector, "5.8 GHz fresnel zone", 5.8e9, KmlCommonElements.PolygonStyleReferenceUri, string.Empty));

            return linkFolder;
        }

        private static Placemark CreateFresnelPlacemark(IDirectionVector directionVector, string name, double frequency, Uri styleReferenceUri, string description)
        {
            var fresnelCreator = new FresnelPolygonsCreator(directionVector);

            IReadOnlyList<IReadOnlyList<ILocation>> fresnelRingLocations = fresnelCreator.CalculateRingLocations(frequency, 20);
            var linearRings = CreateKmlRings(fresnelRingLocations);

            var multiGeometry = new MultipleGeometry();
            foreach(var linearRing in linearRings)
            {
                multiGeometry.AddGeometry(new Polygon
                {
                    AltitudeMode = AltitudeMode.Absolute,
                    OuterBoundary = new OuterBoundary
                    {
                        LinearRing = linearRing
                    }
                });
            }

            var linePlacemark = new Placemark
            {
                Name = name,
                StyleUrl = styleReferenceUri,
                Geometry = multiGeometry,
                Snippet = new Snippet(),
                Description = new Description
                {
                    Text = description
                }
            };

            return linePlacemark;
        }

        /// <summary>
        /// Create the KML LinearRing objects for the given list of locations.
        /// </summary>
        /// <remarks>
        /// <p>This algorithm is used with friendly permission of Rop Gonggrijp of the Freifunk project.</p>
        /// <p>The original PHP version can be found at <see href="https://rop.nl/freifunk/line-of-sight.php.txt" />
        /// and a detailed description at <see href="https://wiki.freifunk.net/Berlin:Line-of-Sight_visualiser" />.</p>
        /// <p>The original PHP code is included in comments.</p>
        /// </remarks>
        private static IEnumerable<LinearRing> CreateKmlRings(IReadOnlyList<IReadOnlyList<ILocation>> rings)
        {
            var polygons = new List<List<ILocation>>(rings.Count);

            // since polygons connect this ring with next, skip last one.
            // for ($ring_nr = 0; $ring_nr < count($rings) - 1; $ring_nr++) {
            for (int ring_nr = 0; ring_nr < rings.Count - 1; ++ring_nr)
            {
                // $next_ring_nr = $ring_nr + 1;
                var next_ring_nr = ring_nr + 1;

                var currentRing = rings[ring_nr];

                // for ($point_nr = 0; $point_nr < $steps_in_circles; $point_nr++) {
                for (var point_nr = 0; point_nr < currentRing.Count; ++point_nr)
                {
                    // $next_point_nr = $point_nr + 1;
                    var next_point_nr = point_nr + 1;

                    // if ($point_nr == $steps_in_circles - 1)
                    if (point_nr == currentRing.Count - 1)
                    {
                        next_point_nr = 0;
                    }

                    var currentPolygon = new List<ILocation>
                    {
                        // $polygon[] = $rings[$ring_nr][$point_nr];
                        rings[ring_nr][point_nr],

                        // $polygon[] = $rings[$next_ring_nr][$point_nr];
                        rings[next_ring_nr][point_nr],

                        // $polygon[] = $rings[$next_ring_nr][$next_point_nr];
                        rings[next_ring_nr][next_point_nr],

                        // $polygon[] = $rings[$ring_nr][$next_point_nr];
                        rings[ring_nr][next_point_nr]
                    };

                    // $polygons[] = $polygon;
                    polygons.Add(currentPolygon);
                }
            }

            // foreach ($polygons as $polygon) {
            foreach (var polygon in polygons)
            {
                var cc = new CoordinateCollection();
                foreach(var point in polygon)
                {
                    cc.Add(point.ToKmlVector());
                }

                yield return new LinearRing
                {
                    Coordinates = cc
                };
            }
        }

        /// <summary>
        /// Create the KML Placemark containing a LineString object between the given locations.
        /// </summary>
        private static Placemark CreateLinePlacemark(ILocation from, ILocation to, string name, Uri styleUrl, bool extrude, string description)
        {
            var lineStringCoordinates = new CoordinateCollection
            {
                to.ToKmlVector(),
                from.ToKmlVector()
            };

            var lineString = new LineString
            {
                Extrude = extrude,
                AltitudeMode = AltitudeMode.Absolute,
                Coordinates = lineStringCoordinates
            };

            var linePlacemark = new Placemark
            {
                Name = name,
                StyleUrl = styleUrl,
                Geometry = lineString,
                Description = new Description
                {
                    Text = description ?? string.Empty
                }
            };

            return linePlacemark;
        }
    }
}