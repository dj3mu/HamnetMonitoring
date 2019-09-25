using System.Collections.Generic;

namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to the combination of a host name and OID to query.
    /// </summary>
    public interface ICachableOids
    {
        /// <summary>
        /// Gets the cachable OID lookup.
        /// </summary>
        IReadOnlyDictionary<CachableValueMeanings, ICachableOid> Oids { get; }
    }
}
