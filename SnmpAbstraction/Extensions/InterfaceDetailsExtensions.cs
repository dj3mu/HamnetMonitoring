namespace SnmpAbstraction
{
    /// <summary>
    /// Extensions to the <see cref="IInterfaceDetails" /> type.
    /// </summary>
    internal static class InterfaceDetailsExtensions
    {
        /// <summary>
        /// Handle to the logger.
        /// </summary>
#pragma warning disable IDE0052 // for future use
        private static readonly log4net.ILog log = SnmpAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#pragma warning restore
    }
}