using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
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
        /// The location of the database.
        /// </summary>
        private string databaseLocation;

        /// <summary>
        /// Prevent construction from outside the singleton getter.
        /// </summary>
        private DatabaseProvider()
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.databaseLocation = Path.Combine(assemblyFolder, DeviceDatabasePathAndFile);
        }

        /// <summary>
        /// Gets the singleton instance of the database provider class.
        /// </summary>
        public static DatabaseProvider Instance { get; } = new DatabaseProvider();

        /// <summary>
        /// Gets a device database context.
        /// </summary>
        /// <remarks>The context must be disposed off by the caller.</remarks>
        public QueryResultDatabaseContext CreateContext()
        {
            var context = new QueryResultDatabaseContext(this.databaseLocation);
            
            context.Database.Migrate();

            return context;
        }
    }

    /// <summary>
    /// Factory class for the purpose of Entitiy Framework design-time.
    /// </summary>
    internal class QueryResultDatabaseContextFactory : IDesignTimeDbContextFactory<QueryResultDatabaseContext>
    {
        /// <inheritdoc />
        public QueryResultDatabaseContext CreateDbContext(string[] args)
        {
            return DatabaseProvider.Instance.CreateContext();
        }
    }
}
