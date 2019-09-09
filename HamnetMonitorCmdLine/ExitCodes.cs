namespace HamnetMonitorCmdLine
{
    internal enum ExitCodes
    {
        // Everything is OK - no error
        Ok = 0,

        // Command Line Error (wrong or missing option)
        InvalidCommandLine = 1,

        /// <summary>
        /// A HamnetSnmp-specific exception has been caught.
        /// </summary>
        HamnetException = 2,

        /// <summary>
        /// An SNMP exception has been caught.
        /// </summary>
        SnmpException = 3,

        /// <summary>
        /// A general exception has been caught.
        /// </summary>
        Exception = 4
    }
}