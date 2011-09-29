using System;

namespace KeePass.IO.Data
{
    public class DatabaseConfiguration
    {
        /// <summary>
        /// Gets or sets the default name of the user.
        /// </summary>
        /// <value>
        /// The default name of the user.
        /// </value>
        public string DefaultUserName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating
        /// whether password field should be protected.
        /// </summary>
        /// <value><c>true</c> if whether password field
        /// should be protected; otherwise, <c>false</c>.
        /// </value>
        public bool ProtectPassword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating
        /// whether title field should be protected.
        /// </summary>
        /// <value><c>true</c> if whether title field
        /// should be protected; otherwise, <c>false</c>.
        /// </value>
        public bool ProtectTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating
        /// whether username field should be protected.
        /// </summary>
        /// <value><c>true</c> if whether username field
        /// should be protected; otherwise, <c>false</c>.
        /// </value>
        public bool ProtectUserName { get; set; }
    }
}