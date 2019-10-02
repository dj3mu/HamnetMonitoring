namespace SnmpAbstraction
{
    /// <summary>
    /// Enumeration of supported data types.
    /// </summary>
    internal enum DataTypesEnum
    {
        /// <summary>
        /// A string value
        /// </summary>
        String = 1,

        /// <summary>
        /// An OID
        /// </summary>
        Oid = 2,

        /// <summary>
        /// TimeTicks / TimeSpan
        /// </summary>
        TimeTicks = 3,

        /// <summary>
        /// Integer
        /// </summary>
        Integer = 4,

        /// <summary>
        /// A string with hex value (chars 0-9, A-F)
        /// </summary>
        HexString = 5,

        /// <summary>
        /// A 32 bit gauge
        /// </summary>
        Gauge32 = 6,

        /// <summary>
        /// A 64 bit counter
        /// </summary>
        Counter64 = 7,

        /// <summary>
        /// An IP Address
        /// </summary>
        IpAddress = 8,

        /// <summary>
        /// A 32 bit counter
        /// </summary>
        Counter32 = 9,

        /// <summary>
        /// A collection of values of arbitrary type (e.g. used by OID that are only root for walk operations).
        /// </summary>
        Collection = 10
    }
}
