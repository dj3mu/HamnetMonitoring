using System;
using System.Collections.Generic;
using System.Net;
using MySqlConnector;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Implementation of <see cref="IHamnetDbAccess" /> for MySQL using raw SQL queries.
    /// </summary>
    internal class MySqlHamnetDbAccessor : IHamnetDbAccess
    {
        private static readonly log4net.ILog log = HamnetDbAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// To detect redundant calls.
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// Backing field for the lazy-initialized connection.
        /// </summary>
        private MySqlConnection connection;

        /// <summary>
        /// If applicable, an additional Disposer that will be called when this obejects gets disposed off.
        /// </summary>
        private readonly IDisposable additionalDisposer;

        /// <summary>
        /// Instantiate from connection string and an additional Disposer.
        /// </summary>
        /// <param name="connectionString">The connection string to use for talking to the database backend.</param>
        /// <param name="additionalDisposer">An additional Disposer that will be called (if not null) when this obejects gets disposed off.</param>
        public MySqlHamnetDbAccessor(string connectionString, IDisposable additionalDisposer)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "The connection string is null, empty or white-space-only");
            }

            this.ConnectionString = connectionString;
            this.additionalDisposer = additionalDisposer;
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MySqlHamnetDbAccessor()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// Gets the connection string used by this accessor.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// If needed instantiates and returns the <see cref="MySqlConnection" />.
        /// </summary>
        /// <returns>The MySQL connection handle.</returns>
        public MySqlConnection GetConnection()
        {
            if (this.connection != null)
            {
                return this.connection;
            }

            this.connection = new MySqlConnection(this.ConnectionString);

            this.connection.Open();

            return this.connection;
        }

        /// <inheritdoc />
        public IHamnetDbHosts QueryBgpRouters()
        {
            var connection = this.GetConnection();
            List<IHamnetDbHost> hosts = this.ReadHostsForSqlCommand("SELECT ip,site,name,typ FROM hamnet_host WHERE routing=1");

            return new HamnetDbHosts(hosts);
        }

        /// <inheritdoc />
        public IHamnetDbHosts QueryMonitoredHosts()
        {
            var connection = this.GetConnection();
            List<IHamnetDbHost> hosts = this.ReadHostsForSqlCommand("SELECT ip,site,name,typ FROM hamnet_host WHERE monitor=1");

            return new HamnetDbHosts(hosts);
        }

        /// <inheritdoc />
        public IHamnetDbSites QuerySites()
        {
            throw new NotImplementedException("Querying sites from HamnetDB via MySQL is not yet implemented (was not needed up to now)");
        }

        /// <inheritdoc />
        public IHamnetDbSubnets QuerySubnets()
        {
            var connection = this.GetConnection();
            List<IHamnetDbSubnet> subnets = new List<IHamnetDbSubnet>();
            using(MySqlCommand cmd = new MySqlCommand("SELECT ip FROM hamnet_subnet;", this.connection))
            {
                using MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var networkCidr = reader.GetString("ip");
                    if (!IPNetwork.TryParse(networkCidr, out IPNetwork ipNet))
                    {
                        log.Error($"Cannot convert retrieved string '{networkCidr}' to a valid IP network. This entry will be skipped.");
                        continue;
                    }

                    subnets.Add(new HamnetDbSubnet(ipNet));
                }
            }

            return new HamnetDbSubnets(subnets);
        }

        /// <summary>
        /// Correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes off this instance.
        /// </summary>
        /// <param name="disposing"><c>true</c> when called by <see cref="Dispose()" />.
        /// <c>false</c> when called by finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (this.connection != null)
                    {
                        this.connection.Dispose();
                        this.connection = null;
                    }

                    if (this.additionalDisposer != null)
                    {
                        this.additionalDisposer.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                this.disposedValue = true;
            }
        }

        private List<IHamnetDbHost> ReadHostsForSqlCommand(string hostSelectCommand)
        {
            List<IHamnetDbHost> hosts = new List<IHamnetDbHost>();

            using (MySqlCommand cmd = new MySqlCommand(hostSelectCommand, this.connection))
            {
                using MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var addressString = reader.GetString("ip");
                    if (!IPAddress.TryParse(addressString, out IPAddress address))
                    {
                        log.Error($"Cannot convert retrieved string '{addressString}' to a valid IP address. This entry will be skipped.");
                        continue;
                    }

                    hosts.Add(new HamnetDbHost(address, reader.GetString("site"), reader.GetString("name"), reader.GetString("typ")));
                }
            }

            return hosts;
        }
    }
}