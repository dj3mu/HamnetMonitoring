namespace SnmpAbstraction
{
    /// <summary>
    /// Interface to containers whose properties are lazy-evaluated (i.e. on first access).
    /// </summary>
    public interface ILazyEvaluated
    {
        /// <summary>
        /// Triggers immediate evaluation of all lazy properties.
        /// </summary>
        void ForceEvaluateAll();
    }
}