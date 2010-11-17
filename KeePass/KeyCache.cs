using System;
using KeePass.IO;

namespace KeePass
{
    public static class KeyCache
    {
        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        /// <value>The database.</value>
        public static Group Database { get; set; }

        /// <summary>
        /// Gets a value indicating whether user has provide the password..
        /// </summary>
        /// <value>
        /// 	<c>true</c> if user has provide the password; otherwise, <c>false</c>.
        /// </value>
        public static bool HasPassword
        {
            get { return !string.IsNullOrEmpty(Password); }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public static string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the password should be stored..
        /// </summary>
        /// <value><c>true</c> if the password should be stored; otherwise, <c>false</c>.</value>
        public static bool StorePassword { get; set; }

        /// <summary>
        /// Clears password cache..
        /// </summary>
        public static void Clear()
        {
            Password = null;
        }
    }
}