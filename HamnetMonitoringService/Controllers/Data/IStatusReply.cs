using System.Collections.Generic;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Interface to a web reply status info.
    /// </summary>
    public interface IStatusReply
    {
        /// <summary>
        /// Gets the error details or empty string if no error.
        /// </summary>
        IEnumerable<string> ErrorDetails { get;}
    }
}
