using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Implementation of <see cref="IHamnetDbAccess" /> retrieving data via REST / JSON interface of HamnetDB.
    /// </summary>
    internal class JsonHamnetDbAccessor : IHamnetDbAccess
    {
        private static readonly log4net.ILog log = HamnetDbAbstraction.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Culture used by HamnetDB.
        /// </summary>
        private static CultureInfo HamnetDbJsonCultureInfo = CultureInfo.CreateSpecificCulture("de-DE");

        /// <summary>
        /// HamnetDB is using German formattings.
        /// </summary>
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Converters = { new HamnetDbDoubleConverter() }
        };

        /// <summary>
        /// To detect redundant calls.
        /// </summary>
        private bool disposedValue = false;

        /// <summary>
        /// If applicable, an additional Disposer that will be called when this obejects gets disposed off.
        /// </summary>
        private readonly IDisposable additionalDisposer;

        /// <summary>
        /// Instantiate from connection string and an additional Disposer.
        /// </summary>
        /// <param name="hostsApiUrl">The URL to access the hosts of HamnetDB.</param>
        /// <param name="subnetsApiUrl">The URL to access the subnets of HamnetDB.</param>
        /// <param name="sitesApiUrl">The URL to access the sites of HamnetDB.</param>
        /// <param name="additionalDisposer">An additional Disposer that will be called (if not null) when this obejects gets disposed off.</param>
        public JsonHamnetDbAccessor(string hostsApiUrl, string subnetsApiUrl, string sitesApiUrl, IDisposable additionalDisposer)
        {
            if (string.IsNullOrWhiteSpace(hostsApiUrl))
            {
                throw new ArgumentNullException(nameof(hostsApiUrl), "The hosts API URL is null, empty or white-space-only");
            }

            if (string.IsNullOrWhiteSpace(subnetsApiUrl))
            {
                throw new ArgumentNullException(nameof(subnetsApiUrl), "The subnets API URL is null, empty or white-space-only");
            }

            if (string.IsNullOrWhiteSpace(sitesApiUrl))
            {
                throw new ArgumentNullException(nameof(sitesApiUrl), "The sitesApiUrl API URL is null, empty or white-space-only");
            }

            this.HostApiUrl = hostsApiUrl;
            this.SubnetsApiUrl = subnetsApiUrl;
            this.SitesApiUrl = sitesApiUrl;
            this.additionalDisposer = additionalDisposer;
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~JsonHamnetDbAccessor()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>
        /// Gets the URL to access the hosts of HamnetDB.
        /// </summary>
        public string HostApiUrl { get; }

        /// <summary>
        /// Gets the URL to access the subnets of HamnetDB.
        /// </summary>
        public string SubnetsApiUrl { get; }

        /// <summary>
        /// Gets the URL to access the sites of HamnetDB.
        /// </summary>
        public string SitesApiUrl { get; }

        /// <inheritdoc />
        public IHamnetDbHosts QueryBgpRouters()
        {
            var responseString = this.SendHttpRequest(new Uri(this.HostApiUrl, UriKind.Absolute));

            var responseData = JsonConvert.DeserializeObject<IEnumerable<JsonHostDataSet>>(responseString);
            List<IHamnetDbHost> hosts = new List<IHamnetDbHost>();
            foreach (var responseDataSet in responseData.Where(r => !r.Deleted && r.Routing))
            {
                IPAddress address;
                if (!IPAddress.TryParse(responseDataSet.Address, out address))
                {
                    log.Error($"Cannot convert retrieved string '{responseDataSet.Address}' to a valid IP address. This entry will be skipped.");
                    continue;
                }

                hosts.Add(new HamnetDbHost(address, responseDataSet.Site, responseDataSet.Name, responseDataSet.Typ));
            }

            return new HamnetDbHosts(hosts);
        }

        /// <inheritdoc />
        public IHamnetDbHosts QueryMonitoredHosts()
        {
            var responseString = this.SendHttpRequest(new Uri(this.HostApiUrl, UriKind.Absolute));

            var responseData = JsonConvert.DeserializeObject<IEnumerable<JsonHostDataSet>>(responseString);
            List<IHamnetDbHost> hosts = new List<IHamnetDbHost>();
            foreach (var responseDataSet in responseData.Where(r => !r.Deleted && r.Monitor))
            {
                IPAddress address;
                if (!IPAddress.TryParse(responseDataSet.Address, out address))
                {
                    log.Error($"Cannot convert retrieved string '{responseDataSet.Address}' to a valid IP address. This entry will be skipped.");
                    continue;
                }

                hosts.Add(new HamnetDbHost(address, responseDataSet.Site, responseDataSet.Name, responseDataSet.Typ));
            }

            return new HamnetDbHosts(hosts);
        }

        /// <inheritdoc />
        public IHamnetDbSubnets QuerySubnets()
        {
            var responseString = this.SendHttpRequest(new Uri(this.SubnetsApiUrl, UriKind.Absolute));

            var responseData = JsonConvert.DeserializeObject<IEnumerable<JsonSubnetDataSet>>(responseString);
            List<IHamnetDbSubnet> hosts = new List<IHamnetDbSubnet>();
            foreach (var responseDataSet in responseData.Where(r => !r.Deleted))
            {
                IPNetwork network;
                if (!IPNetwork.TryParse(responseDataSet.Subnet, out network))
                {
                    log.Error($"Cannot convert retrieved string '{responseDataSet.Subnet}' to a valid IP subnet. This entry will be skipped.");
                    continue;
                }

                hosts.Add(new HamnetDbSubnet(network));
            }

            return new HamnetDbSubnets(hosts);
        }

        /// <inheritdoc />
        public IHamnetDbSites QuerySites()
        {
            var responseString = this.SendHttpRequest(new Uri(this.SitesApiUrl, UriKind.Absolute));

            var responseData = JsonConvert.DeserializeObject<IEnumerable<JsonSiteDataSet>>(responseString, SerializerSettings);

            return new HamnetDbSites(responseData);
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

        private string SendHttpRequest(Uri uri)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = uri;

                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // List data response.
                HttpResponseMessage response = client.GetAsync(string.Empty).Result;
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    throw new HttpRequestException($"Request for '{uri}' failed: Status {response.StatusCode}, Reason '{response.ReasonPhrase}");
                }
            }
        }

        /// <summary>
        /// JsonConverter for reading the double values with German formatting as used by HamnetDB REST API.
        /// </summary>
        private class HamnetDbDoubleConverter : JsonConverter<double>
        {
            /// <inheritdoc />
            public override double ReadJson(JsonReader reader, Type objectType, double existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                string s = (string)reader.Value?.ToString();

                if (!double.TryParse(s, NumberStyles.Any, HamnetDbJsonCultureInfo, out double parsedDouble))
                {
                    return double.NaN;
                }

                return parsedDouble;
            }

            /// <inheritdoc />
            public override void WriteJson(JsonWriter writer, double value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString(HamnetDbJsonCultureInfo));
            }
        }
    }
}