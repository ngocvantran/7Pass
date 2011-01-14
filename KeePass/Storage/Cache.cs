using System;
using KeePass.IO;

namespace KeePass.Storage
{
    internal static class Cache
    {
        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        public static Database Database { get; set; }
    }
}