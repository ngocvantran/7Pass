using System;
using System.Collections.Generic;

namespace KeePass.Storage
{
    public class DatabaseDetails
    {
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