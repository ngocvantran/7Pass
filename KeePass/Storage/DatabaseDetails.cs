using System;
using System.Collections.Generic;

namespace KeePass.Storage
{
    public class DatabaseDetails
    {
        /// <summary>
        /// Gets or sets a value indicating whether
        /// the database has local changes.
        /// </summary>
        /// <value><c>true</c> if the database has
        /// local changes; otherwise, <c>false</c>.
        /// </value>
        public bool HasLocalChanges { get; set; }

        /// <summary>
        /// Gets or sets the modified value.
        /// </summary>
        /// <value>
        /// The modified value.
        /// </value>
        public string Modified { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the recently viewed entries.
        /// </summary>
        /// <value>
        /// The recently viewed entries.
        /// </value>
        public List<string> Recents { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url { get; set; }

        public DatabaseDetails()
        {
            Recents = new List<string>();
        }
    }
}