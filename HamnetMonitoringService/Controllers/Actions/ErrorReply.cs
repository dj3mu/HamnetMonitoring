using System;
using System.Collections.Generic;
using System.Linq;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Class for a general error reply.
    /// </summary>
    internal class ErrorReply : IStatusReply
    {
        List<string> errorDetailsBacking = new List<string>();

        /// <summary>
        /// Construct from an exception.
        /// </summary>
        /// <param name="ex">The exception to create an error reply for.</param>
        public ErrorReply(Exception ex)
        {
            var currentException = ex;
            while(currentException != null)
            {
                this.errorDetailsBacking.Add($"{currentException.GetType().Name}: {currentException.Message}");

                currentException = currentException.InnerException;
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> ErrorDetails => this.errorDetailsBacking;
    }
}