namespace HamnetMonitorCmdLine
{
    internal enum ExitCodes
    {
        // Everything is OK - no error
        Ok = 0,

        // Command Line Error (wrong or missing option)
        InvalidCommandLine = 1
    }
}