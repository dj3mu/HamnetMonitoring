using System.Collections.Generic;
using SnmpAbstraction;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Interface to the data of a BGP peers query.
    /// </summary>
    internal interface ICacheDataResult : IStatusReply, IReadOnlyList<ICacheData>
    {
    }
}
