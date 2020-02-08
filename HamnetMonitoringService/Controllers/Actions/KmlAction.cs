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
        private string fromCall;

        private string toCall;

        private FromUrlQueryQuerierOptions fromUrlQueryQuerierOptions;

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
            IHamnetDbSite fromSite = this.GetSiteForCall(this.fromCall);
            IHamnetDbSite toSite = this.GetSiteForCall(this.toCall);

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
}
