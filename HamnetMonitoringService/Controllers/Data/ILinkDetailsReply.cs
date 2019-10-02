using System.Collections.Generic;

namespace HamnetDbRest.Controllers
{
    /// <summary>
    /// Interface to a web reply with details of the links between devices.
    /// </summary>
    public interface ILinkDetailsReply : IStatusReply
    {
        /// <summary>
        /// Gets the list of link details.
        /// </summary>
        IReadOnlyList<ILinkDetailReply> Details { get; }
    }
}
