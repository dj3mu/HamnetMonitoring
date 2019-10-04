using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SnmpAbstraction
{
    /// <summary>
    /// Exception class for exceptions related to HamnetSnmp library.
    /// </summary>
    [Serializable]
    public class HamnetSnmpException : Exception
    {
        /// <summary>
        /// Gets the list of host(s) affected by this error.
        /// </summary>
        public IReadOnlyCollection<string> AffectedHosts { get; }

        /// <summary>
        /// Default c'tor. No more details.
        /// </summary>
        public HamnetSnmpException()
        {
            this.AffectedHosts = new string[0];
        }

        /// <summary>
        /// Construct with an informative message.
        /// </summary>
        /// <param name="message">The message containing details of the exception.</param>
        public HamnetSnmpException(string message) : base(message)
        {
            this.AffectedHosts = new string[0];
        }

        /// <summary>
        /// Construct with an informative message.
        /// </summary>
        /// <param name="message">The message containing details of the exception.</param>
        /// <param name="affectedHosts">The addresses of the host(s) impacted by this exception.</param>
        public HamnetSnmpException(string message, params string[] affectedHosts) : base(message)
        {
            this.AffectedHosts = affectedHosts?.Where(a => !string.IsNullOrWhiteSpace(a))?.ToArray() ?? new string[0];
        }

        /// <summary>
        /// Construct with an informative message and nest another exception.
        /// </summary>
        /// <param name="message">The message containing details of the exception.</param>
        /// <param name="innerException">The exception to be nested.</param>
        public HamnetSnmpException(string message, Exception innerException) : base(message, innerException)
        {
            this.AffectedHosts = new string[0];
        }

        /// <summary>
        /// Construct with an informative message and nest another exception.
        /// </summary>
        /// <param name="message">The message containing details of the exception.</param>
        /// <param name="innerException">The exception to be nested.</param>
        /// <param name="affectedHosts">The addresses of the host(s) impacted by this exception.</param>
        public HamnetSnmpException(string message, Exception innerException, params string[] affectedHosts) : base(message, innerException)
        {
            this.AffectedHosts = affectedHosts?.Where(a => !string.IsNullOrWhiteSpace(a))?.ToArray() ?? new string[0];
        }

        /// <summary>
        /// Deserialization c'tor.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        protected HamnetSnmpException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}