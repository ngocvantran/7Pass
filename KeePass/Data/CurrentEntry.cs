using System;
using KeePass.IO.Data;

namespace KeePass.Data
{
    internal static class CurrentEntry
    {
        /// <summary>
        /// Gets or sets the entry.
        /// </summary>
        /// <value>
        /// The entry.
        /// </value>
        public static Entry Entry { get; set; }

        /// <summary>
        /// Gets or sets a value indicating
        /// whether <see cref="Entry"/> has changes.
        /// </summary>
        /// <value><c>true</c> if <see cref="Entry"/>
        /// has changes; otherwise, <c>false</c>.
        /// </value>
        public static bool HasChanges { get; set; }

        /// <summary>
        /// Determines whether the specified entry is new.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>
        ///   <c>true</c> if the specified entry is new; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNew(this Entry entry)
        {
            return string.IsNullOrEmpty(entry.ID);
        }
    }
}