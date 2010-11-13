using System;

namespace KeePass
{
    public static class KeyCache
    {
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
        /// Clears password cache..
        /// </summary>
        public static void Clear()
        {
            Password = null;
        }
    }
}