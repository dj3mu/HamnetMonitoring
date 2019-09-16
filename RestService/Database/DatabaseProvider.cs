using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;

namespace RestService.Database
{
    /// <summary>
    /// Provider class for databases.
    /// </summary>
    internal class DatabaseProvider
    {
        /// <summary>
        /// The device database path and file name.
        /// </summary>
        private static readonly string DeviceDatabasePathAndFile = Path.Combine("Config", "ResultDatabase.sqlite");

        /// <summary>
        /// Prevent construction from outside the singleton getter.
        /// </summary>
        private DatabaseProvider()
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.ResultDatabase = new QueryResultDatabaseContext(Path.Combine(assemblyFolder, DeviceDatabasePathAndFile));
            this.ResultDatabase.Database.EnsureCreated();
        }

        /// <summary>
        /// Gets the singleton instance of the database provider class.
        /// </summary>
        public static DatabaseProvider Instance { get; } = new DatabaseProvider();

        /// <summary>
        /// Gets the device database handle.
        /// </summary>
        public QueryResultDatabaseContext ResultDatabase { get; }
    }

    /// <summary>
    /// Factory class for the purpose of Entitiy Framework design-time.
    /// </summary>
    internal class QueryResultDatabaseContextFactory : IDesignTimeDbContextFactory<QueryResultDatabaseContext>
    {
        /// <inheritdoc />
        public QueryResultDatabaseContext CreateDbContext(string[] args)
        {
            return DatabaseProvider.Instance.ResultDatabase;
        }
    }
}
