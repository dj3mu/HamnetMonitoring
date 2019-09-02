namespace SnmpAbstraction
{
    /// <summary>
    /// Enumeration of supported data types.
    /// </summary>
    internal enum DataTypesEnum
    {
        // A string value
        String = 1,

        // An OID
        Oid = 2,

        // TimeTicks / TimeSpan
        TimeTicks = 3,

        // Integer
        Integer = 4,

        // A string with hex value (chars 0-9, A-F)
        HexString = 5,

        // A 32 bit gauge
        Gauge32 = 6,

        // A 64 bit counter
        Counter64 = 7,

        // An IP Address
        IpAddress = 8,

        // A 32 bit counter
        Counter32 = 9
    }
}
