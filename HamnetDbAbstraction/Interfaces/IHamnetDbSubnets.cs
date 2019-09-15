using System.Collections.Generic;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Interface to access the Subnet info of HamnetDB.
    /// </summary>
    public interface IHamnetDbSubnets : IReadOnlyCollection<IHamnetDbSubnet>
    {
    }
}