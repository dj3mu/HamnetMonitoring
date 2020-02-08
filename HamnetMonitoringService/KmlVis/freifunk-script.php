<?php

// We're re-using the algorithms and styles with friendly permission of Rob Gonggrijp of the Freifunk project.
// This script can be found at <see href="https://rop.nl/freifunk/line-of-sight.php.txt" />
// and a detailed description at <see href="https://wiki.freifunk.net/Berlin:Line-of-Sight_visualiser" />.

define ('SPEED_OF_LIGHT', 299792458);   	// in m/s
define ('CIRCUMFERENCE_OF_EARTH', 40075000);	// in m 

$cachefile = 'cached_locations';

// If the cache file exists and it is younger than 12 hrs and 'flush' get var is is not set ...
if (file_exists($cachefile)) {
	if (time() - filectime($cachefile) < 12 * 3600 && !isset($_GET['flush'])) {
		// ... then read cache file and populate the $locations array with it
		$locations = unserialize(file_get_contents($cachefile));
	}
}

if ( !isset($locations) ) { 
	// Otherwise scrape the information from the freifunk wiki, getting all info about the berlin nodes
	$index = file_get_contents('https://wiki.freifunk.net/Kategorie:Standorte_Berlin');
	preg_match_all("|<a href=\"/Berlin:Standorte:(.*?)\" title=\"Berlin:Standorte:(.*?)\"|", $index, $standorte, PREG_SET_ORDER);

//	echo "<pre>";
//	print_r ($standorte);
//	exit();

	foreach ($standorte as $standort) {
		unset($location);
		$location['url'] = "https://wiki.freifunk.net/Berlin:Standorte:" . $standort[1];
		$location['name'] = $standort[2];

		$standort_page_src = file_get_contents($location['url'] . '?action=raw');
		preg_match("/{{Infobox Freifunk-Standort(.*?)\n}}/s", $standort_page_src, $matches);
		$infobox = $matches[1];

		foreach( explode( "| ", $infobox) as $pair) {
			list($name, $value) = explode(" = ", $pair);
			if ( strlen(trim($name)) && strlen(trim($value)) ) {
				$value = trim($value);
				$value = preg_replace('/\[\[(.*?)\|(.*?)\]\]/', '<a href="https://wiki.freifunk.net/$1">$2</a>', $value);
				$value = preg_replace('/\[\[(.*?)\]\]/', '<a href="https://wiki.freifunk.net/$1">$1</a>', $value);
				$value = preg_replace('/\{\{Knoten (.*?)\}\},?\s?/', '$1 (<a href="http://monitor.berlin.freifunk.net/host.php?h=$1">Monitor</a>, <a href="http://openwifimap.net//index.html#detail?node=$1.olsr">OWM</a>)<br>', $value);
				$value = preg_replace('/\[(.*?) (.*?)\]/', '<a href="$1">$2</a>', $value);
				$value = preg_replace('/([A-Za-z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})/', '<a href="mailto:$1">$1</a>', $value);
				$location[$name]=$value;
			}	
		}

		if (trim($location['Koordinaten'])) {
			list($location['lat'], $location['lon']) = explode(",", $location['Koordinaten']);
			unset($location['Koordinaten']);
		}

		if (strlen(trim($location['ueberNN'])) == 0) {
			$location['alt'] = 0;
		} else {
			$location['alt'] = $location['ueberNN'];
		}
		unset($location['ueberNN']);

		$locations[] = $location;
	}

	// And write the array to the cache file
	file_put_contents( $cachefile, serialize($locations) );
}

// Parse the variables Google Earth passes with each refresh as instructed by the first KML file.
list ($cameraLon, $cameraLat, $cameraAlt) = explode(",", $_GET['VARS']);

// Create links if the eye altitude is below 250 meters
if (isset ($cameraAlt) && $cameraAlt < 250) {
	$survey_location['lat'] = $cameraLat;
	$survey_location['lon'] = $cameraLon;
	$survey_location['alt'] = $cameraAlt - 6;
	$survey_location['name'] = "Survey Location";

	foreach($locations as $location) {
		if ( $location['alt'] > 0 ) {
			unset($link);
			$link['name'] = $location['name'];
			$link['lat'] = $location['lat'];
			$link['lon'] = $location['lon'];
			$link['alt'] = $location['alt'];
			$link['distance'] = distance($survey_location, $link) . " m";
			$link['azimuth to'] = bearing($survey_location, $link) . "&deg;";
			$link['elevation to'] = elevation($survey_location, $link) . "&deg;";
			$link['azimuth from'] = bearing($link, $survey_location) . "&deg;";
			$link['fspl_2.4'] = fspl( $link['distance'], 2400000000);
			$link['fspl_5'] = fspl( $link['distance'], 5000000000);
			$links[] = $link;
		}
	}

	// Sort the $links array by direction to $survey_location, starting north.
	if (isset($links)) {
		foreach ($links as $key => $link) {
			$direction[$key] = $link['azimuth to'];
		}
		array_multisort($direction, SORT_ASC, SORT_NUMERIC, $links);
	}

	$locations[] = $survey_location;
}

// This creates $kml which is what's output in the end
$kml = headerKML();

// First all the nodes in a non-expanding folder with a placemark icon.
$kml .= '<Folder>';
$kml .= '<name>Locations</name>';
$kml .= '<Style><ListStyle><listItemType>checkHideChildren</listItemType><ItemIcon><href>http://maps.google.com/mapfiles/kml/shapes/placemark_circle.png</href></ItemIcon></ListStyle></Style>';
foreach ($locations as $location) {
	if ($location['lat'] > 0){
		$kml .= '<Placemark>';
		$kml .= '<name>' . $location['name'] . '</name>';
		$kml .= '<description><![CDATA[' . balloonCSS();
		$kml .= '<h2><a href="' . $location['url'] . '">' . $location['name'] . '</a></h2>';
		$kml .= '<table>';
		foreach ($location as $name => $val) {
			if ($name !== 'name' && $name !== 'url') {
				$kml .= '<tr><td class="left">' . $name . ': </td><td>' . $val . '</td></tr>';
			}
		}
		$kml .= '</table>]]></description>';

		$kml .= '<styleUrl>#msn_placemark_circle</styleUrl>';
		$kml .= '<Point>';
		$kml .= '<altitudeMode>absolute</altitudeMode>';
		$kml .= '<coordinates>' . $location['lon'] . ',' . $location['lat'] . ',' . $location['alt'] . '</coordinates>';
		$kml .= '</Point>';
		$kml .= '</Placemark>' . "\n\n";
	}
}
$kml .= '</Folder>';


// And then all the links if we have a survey location (i.e. we're under 250 m altitude)
if (isset($links)) {
	$kml .= "<Folder><name>Links (clockwise from north)</name><open>1</open><visibility>1</visibility>\n\n";
	foreach ($links as $link) {
		$kml .= '<Folder>';
		$kml .= '<name>Link to ' . $link['name'] . ' (' . number_format($link['distance'] / 1000, 1) . ' km)</name><visibility>1</visibility>';
		$kml .= '<Style><ListStyle><listItemType>checkHideChildren</listItemType><ItemIcon><href>empty_icon.png</href></ItemIcon></ListStyle></Style>';

		$kml .= '<Placemark>';
		$kml .= '<name>Line with screen to ground</name>';
		$kml .= '<styleUrl>#line</styleUrl>';
		$kml .= '<LineString>';
		$kml .= '<extrude>1</extrude>';
		$kml .= '<altitudeMode>absolute</altitudeMode>';
		$kml .= '<coordinates>' . $link['lon'] . ',' . $link['lat'] . ',' . $link['alt'] . ',' . $survey_location['lon'] . ',' . $survey_location['lat'] . ',' . ( $survey_location['alt'] ) . '</coordinates>';
		$kml .= '</LineString>';
		$kml .= '</Placemark>' . "\n";

		$kml .= '<Placemark>';
		$kml .= '<name>2.4 GHz fresnel zone</name>';
		$kml .= '<snippet></snippet>';
		$kml .= '<description><![CDATA[' . balloonCSS();
		$kml .= '<h2>Link to ' . $link['name'] . ' (' . number_format($link['distance'] / 1000, 1) . ' km)</h2>';
		$kml .= '<table>';
		$kml .= '<tr><td class="left">distance: </td><td>' . $link['distance'] . '</td></tr>';
		$kml .= '<tr><td class="left">azimuth to: </td><td>' . $link['azimuth to'] . '</td></tr>';
		$kml .= '<tr><td class="left">elevation to: </td><td>' . $link['elevation to'] . '</td></tr>';
		$kml .= '<tr><td class="left">azimuth from: </td><td>' . $link['azimuth from'] . '</td></tr>';
		$kml .= '<tr><td class="left"><a href="https://en.wikipedia.org/wiki/Free-space_path_loss">FSPL</a>: </td><td>' . $link['fspl_2.4'] . ' dB @ 2.4 GHz<br>' . $link['fspl_5'] . ' dB @ 5 GHz</td></tr>';
		$kml .= '</table>]]></description>';
		$kml .= '<styleUrl>#polygon-transparent</styleUrl>';
		$kml .= '<MultiGeometry>' . "\n\n";
		$kml .= makeFresnelPolygons( $survey_location, $link, 2400000000, 20);
		$kml .= '</MultiGeometry>' . "\n\n";
		$kml .= '</Placemark>';

		$kml .= '<Placemark>';
		$kml .= '<name>5 GHz fresnel zone</name>';
		$kml .= '<styleUrl>#polygon</styleUrl>';
		$kml .= '<MultiGeometry>' . "\n\n";
		$kml .= makeFresnelPolygons( $survey_location, $link, 5000000000,20);
		$kml .= '</MultiGeometry>' . "\n\n";
		$kml .= '</Placemark>';

		$kml .= '</Folder>' . "\n\n";
	}
	$kml .= '</Folder>';

}

$kml .= '</Document></kml>' . "\n";

echo $kml;


// The arguments to distance, bearing and elevation functions are arrays that expected to have keys
// named 'lat', 'lon' and (except for bearing) 'alt'. 

function distance($from, $to) {

	$lat1 = deg2rad($from['lat']);
	$lon1 = deg2rad($from['lon']);
	$lat2 = deg2rad($to['lat']);
	$lon2 = deg2rad($to['lon']);

	$theta = $lon1 - $lon2;
	$dist = rad2deg( acos(     sin($lat1) * sin($lat2)  +  cos($lat1) * cos($lat2) * cos($theta)     ) )  *  ( CIRCUMFERENCE_OF_EARTH / 360 );

	// Add the diagonal component using pythagoras
	// (even if diff minimal in most of our cases)
	$alt_diff = abs($from['alt'] - $to['alt']);
	$dist = sqrt( ($alt_diff * $alt_diff) + ($dist * $dist) );

	return intval ($dist);

}

function bearing($from, $to) {

	$lat1 = deg2rad($from['lat']);
	$lon1 = deg2rad($from['lon']);
	$lat2 = deg2rad($to['lat']);
	$lon2 = deg2rad($to['lon']);

	//difference in longitudinal coordinates
	$dLon = $lon2 - $lon1;

	//difference in the phi of latitudinal coordinates
	$dPhi = log(tan($lat2 / 2 + M_PI / 4) / tan($lat1 / 2 + M_PI / 4));
 
	//we need to recalculate $dLon if it is greater than pi
	if( abs($dLon) > M_PI ) {
		if($dLon > 0) {
			$dLon = (2 * M_PI - $dLon) * -1;
		} else {
			$dLon = 2 * M_PI + $dLon;
		}
	}

	//return the angle, normalized
	return ( rad2deg(atan2($dLon, $dPhi)) + 360 ) % 360;
}

function elevation($from, $to) {
	
	return intval(rad2deg(atan2( $to['alt'] - $from['alt'], distance($from, $to) )));

}

function fspl($dist, $freq) {

	return intval(20 * log10(((4 * M_PI) / SPEED_OF_LIGHT) * $dist * $freq));

}

function makeFresnelPolygons($from, $to, $freq, $steps_in_circles) {

	// How many degrees is a meter?
	$lat_meter = 1 / ( CIRCUMFERENCE_OF_EARTH / 360 );
	$lon_meter = (1 / cos(deg2rad($from['lat']))) * $lat_meter;


	$distance = distance($from, $to);
	$bearing = bearing($from, $to);
	$wavelen = SPEED_OF_LIGHT / $freq;  // Speed of light


	// $steps_in_path is an array of values between 0 (at $from) and 1 (at $to)
	// These are the distances where new polygons are started to show elipse

	// First we do that at some fixed fractions of path
	$steps_in_path = array(0,0.25,0.4);

	// Then we add some steps set in meters because that looks better at
	// the ends of the beam
	foreach (array(0.3,1,2,4,7,10,20,40,70,100) as $meters) {
		// calculate fraction of path
		$steps_in_path[] = $meters / $distance;
	}

	// Add the reverse of these steps on other side of beam
	$temp = $steps_in_path;
	foreach ($temp as $step) {
		$steps_in_path[] = 1 - $step;
	}
	
	// Sort and remove duplicates
	sort($steps_in_path, SORT_NUMERIC);
	$steps_in_path = array_unique($steps_in_path);

	// Fill array $rings with arrays that each hold a ring of points surrounding the beam
	foreach ($steps_in_path as $step) {

		$centerpoint['lat'] = $from['lat'] + ( ($to['lat'] - $from['lat']) * $step );
		$centerpoint['lon'] = $from['lon'] + ( ($to['lon'] - $from['lon']) * $step );
		$centerpoint['alt'] = $from['alt'] + ( ($to['alt'] - $from['alt']) * $step );

		// Fresnel radius calculation
		$d1 = $distance * $step;
		$d2 = $distance - $d1;
		$radius = sqrt( ($wavelen * $d1 * $d2) / $distance );

		// Bearing of line perpendicular to bearing of line of sight.
		$ring_bearing = $bearing + 90 % 360;

		unset ($ring);
		for ($n=0; $n<$steps_in_circles; $n++) {

			$angle = $n * ( 360 / $steps_in_circles );
			$vertical_factor = cos(deg2rad($angle));
			$horizontal_factor = sin(deg2rad($angle));
			$lat_factor = cos(deg2rad($ring_bearing)) * $horizontal_factor;
			$lon_factor = sin(deg2rad($ring_bearing)) * $horizontal_factor;

			$new_point['lat'] = $centerpoint['lat'] + ($lat_factor * $lat_meter * $radius);
			$new_point['lon'] = $centerpoint['lon'] + ($lon_factor * $lon_meter * $radius);
			$new_point['alt'] = $centerpoint['alt'] + ($vertical_factor * $radius);

			$ring[] = $new_point;
		}
		$rings[] = $ring;

	}

	// Make the polygons

	// since polygons connect this ring with next, skip last one.
	for ($ring_nr = 0; $ring_nr < count($rings) - 1; $ring_nr++) {

		$next_ring_nr = $ring_nr + 1;

		for ($point_nr = 0; $point_nr < $steps_in_circles; $point_nr++) {

			$next_point_nr = $point_nr + 1;
			if ($point_nr == $steps_in_circles - 1) {
				$next_point_nr = 0;
			}

			unset ($polygon);
			$polygon[] = $rings[$ring_nr][$point_nr];
			$polygon[] = $rings[$next_ring_nr][$point_nr];
			$polygon[] = $rings[$next_ring_nr][$next_point_nr];
			$polygon[] = $rings[$ring_nr][$next_point_nr];

			$polygons[] = $polygon;


		}		
	}

	$ret = '';

	foreach ($polygons as $polygon) {
		$ret .= '<Polygon><altitudeMode>absolute</altitudeMode><outerBoundaryIs><LinearRing><coordinates>';

		foreach ($polygon as $point) {
			$ret .= $point['lon'] . ',' . $point['lat'] . ',' . $point['alt'] . " ";
		}
	
		$ret .= '</coordinates></LinearRing></outerBoundaryIs></Polygon>';
	}

	return $ret;

}



function headerKML() {

	$kml = <<<HEREDOC
<?xml version="1.0" encoding="UTF-8"?>
<kml xmlns="http://www.opengis.net/kml/2.2" xmlns:gx="http://www.google.com/kml/ext/2.2" xmlns:kml="http://www.opengis.net/kml/2.2" xmlns:atom="http://www.w3.org/2005/Atom">
<Document>
	<name>line-of-sight.php</name>
	<open>1</open>
	<Style id="sh_placemark_circle_highlight">
		<IconStyle>
			<scale>1.7</scale>
			<Icon>
				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_circle_highlight.png</href>
			</Icon>
		</IconStyle>
		<BalloonStyle id="balloon">
			<bgColor>ff6c3adf</bgColor>
			<textColor>ff000000</textColor>
			<text>$[description]</text>
			<displayMode>default</displayMode>
		</BalloonStyle>
	</Style>
	<Style id="sn_placemark_circle">
		<IconStyle>
			<scale>1.3</scale>
			<Icon>
				<href>http://maps.google.com/mapfiles/kml/shapes/placemark_circle.png</href>
			</Icon>
		</IconStyle>
		<BalloonStyle id="balloon">
			<bgColor>ff6c3adf</bgColor>
			<textColor>ff000000</textColor>
			<text>$[description]</text>
			<displayMode>default</displayMode>
		</BalloonStyle>
	</Style>
	<StyleMap id="msn_placemark_circle">
		<Pair>
			<key>normal</key>
			<styleUrl>#sn_placemark_circle</styleUrl>
		</Pair>
		<Pair>
			<key>highlight</key>
			<styleUrl>#sh_placemark_circle_highlight</styleUrl>
		</Pair>
	</StyleMap>
	<Style id="line">
		<LineStyle>
			<width>1</width>
			<color>ff000000</color>
		</LineStyle>
		<PolyStyle>
			<color>a0ffffff</color>
		</PolyStyle>
		<BalloonStyle id="balloon">
			<bgColor>ff6c3adf</bgColor>
			<textColor>ff000000</textColor>
			<text>$[description]</text>
			<displayMode>default</displayMode>
		</BalloonStyle>
	</Style>
	<Style id="polygon">
		<LineStyle>
			<width>1</width>
			<color>ff006000</color>
		</LineStyle>
		<PolyStyle>
			<color>ff50ff50</color>
		</PolyStyle>
		<BalloonStyle id="balloon">
			<bgColor>ff6c3adf</bgColor>
			<textColor>ff000000</textColor>
			<text>$[description]</text>
			<displayMode>default</displayMode>
		</BalloonStyle>
	</Style>
	<Style id="polygon-transparent">
		<LineStyle>
			<width>1</width>
			<color>4000ff00</color>
		</LineStyle>
		<PolyStyle>
			<color>8000ff00</color>
		</PolyStyle>
		<BalloonStyle id="balloon">
			<bgColor>ff6c3adf</bgColor>
			<textColor>ff000000</textColor>
			<text>$[description]</text>
			<displayMode>default</displayMode>
		</BalloonStyle>
	</Style>
HEREDOC;

	return $kml;
}

function balloonCSS() {

	// There is no global stylesheet in Google Earth, so this needs to be appended to each balloon to make it display nicely.
	$css = <<<HEREDOC
<style type="text/css">
	a:link {text-decoration: none;}
	td.left {text-align: right; vertical-align: top; margin-bottom: 5px;}
	td, h2 {white-space: nowrap; font-family: verdana;}
</style>
HEREDOC;
	
	return $css;
}

?>