using System;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Interface to access the HamnetDB engine.
    /// </summary>
    public interface IHamnetDbAccess : IDisposable
    {
        /// <summary>
        /// Queries the hosts to be monitored (i.e. for which the &quot;monitor&quot; field is set to <c>true</c>).
        /// </summary>
        /// <returns>The list of hosts to be monitored</returns>
        IHamnetDbHosts QueryMonitoredHosts();

        /// <summary>
        /// Queries the subnets recorded in the HamnetDB.
        /// </summary>
        /// <returns>The list of subnets defined in the HamnetDB.</returns>
        IHamnetDbSubnets QuerySubnets();
        
        /// <summary>
        /// Queries the hosts that are considered to be BGP routers (i.e. for which the &quot;...&quot; field is set to <c>true</c>).
        /// </summary>
        /// <returns>The list of hosts which are BGP routers.</returns>
        IHamnetDbHosts QueryBgpRouters();
    }
}
