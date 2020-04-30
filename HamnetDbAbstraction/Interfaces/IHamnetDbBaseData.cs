using System;

namespace HamnetDbAbstraction
{
    /// <summary>
    /// Interface to the base data of HamnetDB entries.
    /// </summary>
    public interface IHamnetDbBaseData
    {
        /// <summary>
        /// Gets or sets the date and time when the entry has last been edited.
        /// </summary>
        DateTime Edited { get; }

        /// <summary>
        /// Gets or sets the last editor of this subnet.
        /// </summary>
        string Editor { get; }

        /// <summary>
        /// Gets or sets the maintainer of this subnet.
        /// </summary>
        string Maintainer { get; }

        /// <summary>
        /// Gets or sets the version of this data set.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this subnet is editable by maintainer only.
        /// </summary>
        bool MaintainerEditableOnly { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this subnet entry has been deleted and shall not be considered any more.
        /// </summary>
        /// <value>
        /// <c>1</c> means true, <c>0</c> false.
        /// </value>
        bool Deleted { get; }

        /// <summary>
        /// Gets or sets the ID of this subnet.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to check this subnet.
        /// </summary>
        bool NoCheck { get; }
    }
}