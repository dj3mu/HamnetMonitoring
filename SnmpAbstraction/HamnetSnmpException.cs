using System;
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
        /// Default c'tor. No more details.
        /// </summary>
        public HamnetSnmpException()
        {
        }

        /// <summary>
        /// Construct with an informative message.
        /// </summary>
        /// <param name="message">The message containing details of the exception.</param>
        public HamnetSnmpException(string message) : base(message)
        {
        }

        /// <summary>
        /// Construct with an informative message and nest another exception.
        /// </summary>
        /// <param name="message">The message containing details of the exception.</param>
        /// <param name="innerException">The exception to be nested.</param>
        public HamnetSnmpException(string message, Exception innerException) : base(message, innerException)
        {
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