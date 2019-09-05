namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to query algorithm. That is an algorithm that does more complex queries using
    /// the device handlers provided to it.
    /// </summary>
    public interface IQueryAlgorithm<TReturnType>
        where TReturnType : IHamnetSnmpQuerierResult
    {
        /// <summary>
        /// Actually performs the query.
        /// </summary>
        /// <returns>The query result.</returns>
        TReturnType DoQuery();
    }
}
