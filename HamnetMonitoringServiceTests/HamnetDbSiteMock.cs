using System;
using HamnetDbAbstraction;

namespace HamnetMonitoringServiceTests
{
    internal class HamnetDbSiteMock : IHamnetDbSite
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double Altitude { get; set; }

        public double GroundAboveSeaLevel { get; set; }

        public double Elevation { get; set; }

        public string Callsign { get; set; }

        public string Comment { get; set; }

        public bool Inactive { get; set; }

        public DateTime Edited { get; set; }

        public string Editor { get; set; }

        public string Maintainer { get; set; }

        public int Version { get; set; }

        public bool MaintainerEditableOnly { get; set; }

        public bool Deleted { get; set; }

        public int Id { get; set; }

        public bool NoCheck { get; set; }

        public string Name { get; set; }
    }
}