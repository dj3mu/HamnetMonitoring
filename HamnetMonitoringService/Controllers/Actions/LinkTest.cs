using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SnmpAbstraction;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Class doing a link test between two hosts specified by IP or host name.
    /// </summary>
    internal class LinkTest
    {
        private string host1;
        
        private string host2;

        private IQuerierOptions querierOptions;

        /// <summary>
        /// Construct for a specific host.
        /// </summary>
        /// <param name="host1">The first host or IP of the link.</param>
        /// <param name="host2">The second host or IP of the link.</param>
        /// <param name="querierOptions">The options to the Hamnet querier.</param>
        public LinkTest(string host1, string host2, IQuerierOptions querierOptions)
        {
            if (string.IsNullOrWhiteSpace(host1))
            {
                throw new ArgumentNullException(nameof(host1), "Host #1 is null, empty or white-space-only");
            }

            if (string.IsNullOrWhiteSpace(host2))
            {
                throw new ArgumentNullException(nameof(host2), "Host #2 is null, empty or white-space-only");
            }

            this.host1 = host1;
            this.host2 = host2;
            this.querierOptions = querierOptions ?? new FromUrlQueryQuerierOptions();
        }

        /// <summary>
        /// Asynchronuously executes the ping task.
        /// </summary>
        /// <returns></returns>
        internal async Task<ActionResult<IStatusReply>> Execute()
        {
            return await Task.Run(this.DoLinkTest);
        }

        private ActionResult<IStatusReply> DoLinkTest()
        {
            try
            {
                using(var querier = SnmpQuerierFactory.Instance.Create(this.host1, this.querierOptions))
                {
                    var linkDetails = querier.FetchLinkDetails(this.host2);

                    return new LinkDetailsReply(linkDetails);
                }
            }
            catch(Exception ex)
            {
                return new ErrorReply(ex);
            }
        }
    }
}
