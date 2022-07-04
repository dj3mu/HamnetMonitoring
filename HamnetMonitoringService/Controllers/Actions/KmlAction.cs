using System;
using System.Linq;
using System.Threading.Tasks;
using HamnetDbAbstraction;
using HamnetMonitoringService;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Action for
    /// </summary>
    internal class KmlAction
    {
        private readonly string fromCall;

        private readonly ToolController.ToLocationFromQuery toLocation;

        private readonly ToolController.FromLocationFromQuery fromLocation;

        private readonly string toCall;

#pragma warning disable IDE0052 // API parameter
        private readonly FromUrlQueryQuerierOptions fromUrlQueryQuerierOptions;
#pragma warning restore

        private readonly IHamnetDbAccess hamnetDbAccess;

        /// <summary>
        /// Initializes a new instance of the KmlAction
        /// </summary>
        /// <param name="fromCall">The from site callsign.</param>
        /// <param name="toCall">The to site callsign.</param>
        /// <param name="fromUrlQueryQuerierOptions">The querier options.</param>
        /// <param name="hamnetDbAccess">The accessor to HamnetDB (needed to get coordinates for callsigns).</param>
        public KmlAction(string fromCall, string toCall, FromUrlQueryQuerierOptions fromUrlQueryQuerierOptions, IHamnetDbAccess hamnetDbAccess)
        {
            this.hamnetDbAccess = hamnetDbAccess;
            this.fromCall = fromCall;
            this.toCall = toCall;
            this.fromUrlQueryQuerierOptions = fromUrlQueryQuerierOptions;
        }

        /// <summary>
        /// Initializes a new instance of the KmlAction
        /// </summary>
        /// <param name="fromCall">The from site callsign.</param>
        /// <param name="toLocation">The to site location.</param>
        /// <param name="fromUrlQueryQuerierOptions">The querier options.</param>
        /// <param name="hamnetDbAccess">The accessor to HamnetDB (needed to get coordinates for callsigns).</param>
        public KmlAction(string fromCall, ToolController.ToLocationFromQuery toLocation, FromUrlQueryQuerierOptions fromUrlQueryQuerierOptions, IHamnetDbAccess hamnetDbAccess)
        {
            this.hamnetDbAccess = hamnetDbAccess;
            this.fromCall = fromCall;
            this.toLocation = toLocation;
            this.fromUrlQueryQuerierOptions = fromUrlQueryQuerierOptions;
        }

        /// <summary>
        /// Initializes a new instance of the KmlAction
        /// </summary>
        /// <param name="fromLocation">The from site location.</param>
        /// <param name="toLocation">The to site location.</param>
        /// <param name="fromUrlQueryQuerierOptions">The querier options.</param>
        /// <param name="hamnetDbAccess">The accessor to HamnetDB (needed to get coordinates for callsigns).</param>
        public KmlAction(ToolController.FromLocationFromQuery fromLocation, ToolController.ToLocationFromQuery toLocation, FromUrlQueryQuerierOptions fromUrlQueryQuerierOptions, IHamnetDbAccess hamnetDbAccess)
        {
            this.fromLocation = fromLocation;
            this.toLocation = toLocation;
            this.fromUrlQueryQuerierOptions = fromUrlQueryQuerierOptions;
            this.hamnetDbAccess = hamnetDbAccess;
        }

        /// <summary>
        /// Executes the action.
        /// </summary>
        public Task<string> Execute()
        {
            var task = new Task<string>(this.ExecuteInternal);

            task.Start();

            return task;
        }

        private string ExecuteInternal()
        {
            IHamnetDbSite fromSite = string.IsNullOrWhiteSpace(this.fromCall)
                ? new RawSiteFromFromQuery(this.fromLocation)
                : this.GetSiteForCall(this.fromCall);

            IHamnetDbSite toSite = string.IsNullOrWhiteSpace(this.toCall)
                ? new RawSiteFromToQuery(this.toLocation)
                : this.GetSiteForCall(this.toCall);

            var kmlGenerator = new SingleLinkViewKmlGenerator(fromSite, toSite);

            var kmlString = kmlGenerator.GenerateString();

            return kmlString;
        }

        private IHamnetDbSite GetSiteForCall(string call)
        {
            var upperInvariantCall = call.ToUpperInvariant();
            var site = this.hamnetDbAccess.QuerySites()
                .FirstOrDefault(s => s.Callsign.ToUpperInvariant() == upperInvariantCall);

            if (site == null)
            {
                throw new InvalidOperationException($"Cannot find callsign '{call}' in HamnetDB");
            }

            return site;
        }
    }

    /// <summary>
    /// Container for the site data generated of a TO-location from query URL.
    /// </summary>
    /// <remarks>
    /// This code duplication from from and to looks odd. But the alternative would be an
    /// awfully more complicated parsing of query URL (the container property names are the query URL parameter names).<br/>
    /// Perhaps I find a better approach some day.
    /// </remarks>
    internal class RawSiteFromToQuery : IHamnetDbSite
    {
        private readonly ToolController.ToLocationFromQuery locationFromQuery;

        /// <inheritdoc />
        public RawSiteFromToQuery(ToolController.ToLocationFromQuery toLocation)
        {
            this.locationFromQuery = toLocation;
        }

        /// <inheritdoc />
        public string Name => this.locationFromQuery.ToName;

        /// <inheritdoc />
        public string Callsign => this.locationFromQuery.ToName;

        /// <inheritdoc />
        public string Comment { get; } = string.Empty;

        /// <inheritdoc />
        public bool Inactive { get; } = false;

        /// <inheritdoc />
        public DateTime Edited { get; } = DateTime.Now;

        /// <inheritdoc />
        public string Editor { get; } = string.Empty;

        /// <inheritdoc />
        public string Maintainer { get; } = string.Empty;

        /// <inheritdoc />
        public int Version { get; } = 0;

        /// <inheritdoc />
        public bool MaintainerEditableOnly { get; } = false;

        /// <inheritdoc />
        public bool Deleted { get; } = false;

        /// <inheritdoc />
        public int Id { get; } = 0;

        /// <inheritdoc />
        public bool NoCheck { get; } = false;

        /// <inheritdoc />
        public double Latitude => this.locationFromQuery.ToLatitude;

        /// <inheritdoc />
        public double Longitude => this.locationFromQuery.ToLongitude;

        /// <inheritdoc />
        public double GroundAboveSeaLevel => this.locationFromQuery.ToGroundAboveSeaLevel;

        /// <inheritdoc />
        public double Elevation => this.locationFromQuery.ToElevation;

        /// <inheritdoc />
        public double Altitude => this.locationFromQuery.ToGroundAboveSeaLevel + this.locationFromQuery.ToElevation;
    }

    /// <summary>
    /// Container for the site data generated of a TO-location from query URL.
    /// </summary>
    /// <remarks>
    /// This code duplication from from and to looks odd. But the alternative would be an
    /// awfully more complicated parsing of query URL (the container property names are the query URL parameter names).<br/>
    /// Perhaps I find a better approach some day.
    /// </remarks>
    internal class RawSiteFromFromQuery : IHamnetDbSite
    {
        private readonly ToolController.FromLocationFromQuery locationFromQuery;

        /// <inheritdoc />
        public RawSiteFromFromQuery(ToolController.FromLocationFromQuery fromLocation)
        {
            this.locationFromQuery = fromLocation;
        }

        /// <inheritdoc />
        public string Name => this.locationFromQuery.FromName;

        /// <inheritdoc />
        public string Callsign => this.locationFromQuery.FromName;

        /// <inheritdoc />
        public string Comment { get; } = string.Empty;

        /// <inheritdoc />
        public bool Inactive { get; } = false;

        /// <inheritdoc />
        public DateTime Edited { get; } = DateTime.Now;

        /// <inheritdoc />
        public string Editor { get; } = string.Empty;

        /// <inheritdoc />
        public string Maintainer { get; } = string.Empty;

        /// <inheritdoc />
        public int Version { get; } = 0;

        /// <inheritdoc />
        public bool MaintainerEditableOnly { get; } = false;

        /// <inheritdoc />
        public bool Deleted { get; } = false;

        /// <inheritdoc />
        public int Id { get; } = 0;

        /// <inheritdoc />
        public bool NoCheck { get; } = false;

        /// <inheritdoc />
        public double Latitude => this.locationFromQuery.FromLatitude;

        /// <inheritdoc />
        public double Longitude => this.locationFromQuery.FromLongitude;

        /// <inheritdoc />
        public double GroundAboveSeaLevel => this.locationFromQuery.FromGroundAboveSeaLevel;

        /// <inheritdoc />
        public double Elevation => this.locationFromQuery.FromElevation;

        /// <inheritdoc />
        public double Altitude => this.locationFromQuery.FromGroundAboveSeaLevel + this.locationFromQuery.FromElevation;
    }
}
