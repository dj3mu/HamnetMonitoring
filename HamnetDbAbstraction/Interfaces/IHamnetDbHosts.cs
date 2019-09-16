using System.Collections.Generic;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Interface to access the hosts of the HamnetDB.
    /// </summary>
    public interface IHamnetDbHosts : IReadOnlyCollection<IHamnetDbHost>
    {
    }
}
