namespace SnmpAbstraction
{
    /// <summary>
    /// Enumeration of supported data types.
    /// </summary>
    public enum DataTypesEnum
    {
        // A string value
        String = 1,

        // An OID
        Oid = 2,

        // TimeTicks / TimeSpan
        TimeTicks,

        // Integer
        Integer,

        // A string with hex value (chars 0-9, A-F)
        HexString,

        // A 32 bit gauge
        Gauge32,

        // A 64 bit counter
        Counter64,

        // An IP Address
        IpAddress,

        // A 32 bit counter
        Counter32
    }
}
