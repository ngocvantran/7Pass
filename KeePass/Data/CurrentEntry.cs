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
        public static EntryBinding Entry { get; set; }

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