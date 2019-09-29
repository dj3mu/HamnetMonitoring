using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Class doing a ping to a host
    /// </summary>
    internal class PingTest
    {
        private string host;

        /// <summary>
        /// Construct for a specific host.
        /// </summary>
        /// <param name="host">The host or IP address to ping.</param>
        public PingTest(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentNullException(nameof(host), "Host is null, empty or white-space-only");
            }

            this.host = host;
        }

        /// <summary>
        /// Asynchronuously executes the ping task.
        /// </summary>
        /// <returns></returns>
        internal async Task<ActionResult<IStatusReply>> Execute()
        {
            return await Task.Run(this.DoPing);
        }

        private ActionResult<IStatusReply> DoPing()
        {
            try
            {
                Ping pingSender = new Ping ();
                PingOptions options = new PingOptions
                {
                    DontFragment = false,
                    Ttl = 128
                };

                // Create a buffer of 32 bytes of data to be transmitted.
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                int timeout = 3000;
                PingReply reply = pingSender.Send (this.host, timeout, buffer, options);

                return new PingTestResult(reply);
            }
            catch(Exception ex)
            {
                return new ErrorReply(ex);
            }
        }

        /// <summary>
        /// Container for a ping test result.
        /// </summary>
        private class PingTestResult : IPingTestResult
        {
            private PingReply reply;

            public PingTestResult(PingReply reply)
            {
                this.reply = reply;
                this.Address = reply.Address.ToString();
                this.Status = reply.Status.ToString();
                this.RoundtripTime = TimeSpan.FromMilliseconds(this.reply.RoundtripTime);
            }

            /// <inheritdoc />
            public string Address { get; }

            /// <inheritdoc />
            public TimeSpan RoundtripTime { get; }

            /// <inheritdoc />
            public int? TimeToLive => this.reply?.Options?.Ttl;

            /// <inheritdoc />
            public bool? DontFragment => this.reply?.Options?.DontFragment;

            /// <inheritdoc />
            public int? BufferSize => this.reply?.Buffer?.Length;

            /// <inheritdoc />
            public string Status { get; }

            /// <inheritdoc />
            public IEnumerable<string> ErrorDetails { get; } = Enumerable.Empty<string>();
        }
    }
}
