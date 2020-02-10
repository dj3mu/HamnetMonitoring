using System;
using HamnetDbAbstraction;

namespace HamnetMonitoringServiceTests
{
    /// <summary>
    /// Tests for all kinds of extension methods (I don't want to scatter the tests too much).
    /// </summary>
    internal class TestDataProvider
    {
        internal static readonly IHamnetDbSite TestSite1 = new HamnetDbSiteMock
            {
                Callsign = "DB0EBE-Mock",
                Comment = "Mock Site configuration: http…wddvdq4j86d9r/DB0EBE.gif",
                Elevation = 30.0,
                GroundAboveSeaLevel = 600.0,
                Id = 4711,
                Latitude = 48.090109,
                Longitude = 11.961273,
                Altitude = 622.0 + 30.0, // intentionally not machting GroundAboveSeaLevel + Elevation to be able to identify which value is getting used
                Deleted = false,
                Edited = DateTime.Now,
                Editor = "Unit Tester",
                Inactive = false,
                Maintainer = "Unit Test Maintainer",
                MaintainerEditableOnly = false,
                Name = "Ebersberg Aussichtsturm Ludwigshöhe Mock",
                NoCheck = false,
                Version = 4712
            };

        internal static readonly IHamnetDbSite TestSiteDl0rus = new HamnetDbSiteMock
            {
                Callsign = "DL0RUS-Mock",
                Comment = "Mock Site configuration: http…wddvdq4j86d9r/DB0EBE.gif",
                Elevation = 30.0,
                GroundAboveSeaLevel = 557.0, // double.NaN, // estimation
                Id = 1998,
                Latitude = 48.128350,
                Longitude = 11.611690,
                Altitude = 540.0 + 17.0, // double.NaN,
                Deleted = false,
                Edited = DateTime.Now,
                Editor = "DG8NGN-Mock",
                Inactive = false,
                Maintainer = "DG8NGN-Mock Maintainer",
                MaintainerEditableOnly = false,
                Name = "R&S",
                NoCheck = false,
                Version = 3
            };

        internal static readonly IHamnetDbSite TestSiteDb0dba = new HamnetDbSiteMock
            {
                Callsign = "DL0DBA-Mock",
                Comment = "Mock Site configuration: https://www.dropbox.com/s/r2i6d6wu30jujly/DB0DBA.gif",
                Elevation = 61.0,
                GroundAboveSeaLevel = 565.0,
                Id = 1998,
                Latitude = 48.08287,
                Longitude = 11.50305,
                Altitude = 565.0 + 61.0,
                Deleted = false,
                Edited = DateTime.Now,
                Editor = "DG8NGN-Mock",
                Inactive = false,
                Maintainer = "DG8NGN-Mock Maintainer",
                MaintainerEditableOnly = false,
                Name = "München DEBA-Hochhaus Mock",
                NoCheck = false,
                Version = 13
            };

        internal static readonly IHamnetDbSite TestSiteZero1 = new HamnetDbSiteMock
            {
                Callsign = "Zero-1",
                Comment = "Zero-1",
                Elevation = 10.0,
                GroundAboveSeaLevel = 600.0,
                Id = 4711,
                Latitude = 48.0,
                Longitude = 12.0,
                Altitude = 610.0,
                Deleted = false,
                Edited = DateTime.Now,
                Editor = "Zero-1",
                Inactive = false,
                Maintainer = "Zero-1",
                MaintainerEditableOnly = false,
                Name = "Zero-1",
                NoCheck = false,
                Version = 4712
            };

        internal static readonly IHamnetDbSite TestSiteZero2 = new HamnetDbSiteMock
            {
                Callsign = "Zero-2",
                Comment = "Zero-2",
                Elevation = 10.0,
                GroundAboveSeaLevel = 600.0,
                Id = 4710,
                Latitude = 48.0,
                Longitude = 12.1,
                Altitude = 610.0,
                Deleted = false,
                Edited = DateTime.Now,
                Editor = "Zero-2",
                Inactive = false,
                Maintainer = "Zero-2",
                MaintainerEditableOnly = false,
                Name = "Zero-2",
                NoCheck = false,
                Version = 4712
            };

        internal static readonly IHamnetDbSite TestSite2 = new HamnetDbSiteMock
            {
                Callsign = "DB0ZM-Mock",
                Comment = "Mock Am Standort ist auch   - 2m FM Relais DB0ZM 145.750  - 70cm FM-Relais DB0NJ 438.775  - 70cm DMR-Relais DB0NJ 439.4375  - 2 Kameras von http://www.foto-webcam.eu    Site configuration: https://www.dropbox.com/s/0sd219kow4lb23f/DB0ZM.gif",
                Elevation = 10.0,
                GroundAboveSeaLevel = 500.0,
                Id = 11,
                Latitude = 48.184086,
                Longitude = 11.611249,
                Altitude = 500.0 + 65.0, // intentionally not machting GroundAboveSeaLevel + Elevation to be able to identify which value is getting used
                Deleted = false,
                Edited = DateTime.Now,
                Editor = "Unit Tester ZM",
                Inactive = false,
                Maintainer = "Unit Test Maintainer ZM",
                MaintainerEditableOnly = false,
                Name = "München-Freimann Studentenstadt Mock",
                NoCheck = false,
                Version = 8
            };
    }
}
