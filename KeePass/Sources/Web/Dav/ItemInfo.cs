using System;

namespace KeePass.Sources.Web.Dav
{
    internal class ItemInfo
    {
        /// <summary>
        /// Gets or sets a value indicating
        /// whether this item is a directory.
        /// </summary>
        /// <value><c>true</c> if this item
        /// is a directory; otherwise, <c>false</c>.
        /// </value>
        public bool IsDir { get; set; }

        /// <summary>
        /// Gets or sets the modified time.
        /// </summary>
        /// <value>
        /// The modified time.
        /// </value>
        public string Modified { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; set; }
    }
}