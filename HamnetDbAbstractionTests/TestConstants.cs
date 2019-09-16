namespace HamnetDbAbstractionTests
{
    /// <summary>
    /// Some constants useful for the unit tests
    /// </summary>
    /// <remarks> The address put here is one of the author's private Hamnet client hardware
    /// Feel free to use it for first tests. But please consider switching to one of your
    /// own devices when doing more extensive testind and/or development.
    /// </remarks>
    public static class TestConstants
    {
        /// <summary>
        /// Path to a file containing the database connection string.
        /// </summary>
        public static readonly string ConnectionStringFilePath = @"~\connectionstring.hamnetdb";
    }
}
