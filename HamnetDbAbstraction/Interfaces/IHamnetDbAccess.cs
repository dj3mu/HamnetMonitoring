using System;
using System.Collections.Generic;
using System.Net;

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
        
        /// <summary>
        /// Queries the sites defined in the HamnetDB.
        /// </summary>
        /// <returns>The list of sites defined in HamnetDB.</returns>
        IHamnetDbSites QuerySites();
    }

    /// <summary>
    /// Interface for IHamnetDbAccess that support (more performant) query of interface usually provide by Extension methods in HamnetDbAccessExtensions.
    /// </summary>
    public interface IDirectSupportOfHamnetDbAccessExtensions : IHamnetDbAccess
    {
        /// <summary>
        /// Gets all unique <c>pairs</c> of hosts with monitoring flag set and which share the same subnet.<br/>
        /// Subnets with less than or more than two hosts will be discarded.
        /// </summary>
        /// <param name="subnet">The subnet to return data for.</param>
        /// <returns>The dictionary mapping a subnet to its unique monitored host pair.</returns>
        IReadOnlyDictionary<IHamnetDbSubnet, IHamnetDbHosts> UniqueMonitoredHostPairsInSubnet(IPNetwork subnet);

        /// <summary>
        /// Gets all unique <c>pairs</c> of hosts with monitoring flag set and which share the same subnet.<br/>
        /// Subnets with less than or more than two hosts will be discarded.
        /// </summary>
        /// <returns>The dictionary mapping a subnet to its unique monitored host pair.</returns>
        IReadOnlyDictionary<IHamnetDbSubnet, IHamnetDbHosts> UniqueMonitoredHostPairsInSameSubnet();
    }
}
