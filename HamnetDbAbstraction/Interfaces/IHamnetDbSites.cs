using System.Collections.Generic;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Interface to access the sites of the HamnetDB.
    /// </summary>
    public interface IHamnetDbSites : IReadOnlyCollection<IHamnetDbSite>
    {
    }
}
