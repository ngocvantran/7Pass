using System;
using System.Net;

namespace KeePass.Sources.Web
{
    internal static class WebLinks
    {
        /// <summary>
        /// Gets or sets the credentials for web authentication.
        /// </summary>
        public static ICredentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets the folder.
        /// </summary>
        /// <value>
        /// The folder.
        /// </value>
        public static string Folder { get; set; }

        /// <summary>
        /// Gets a value indicating whether there is any link to display.
        /// </summary>
        /// <value>
        ///   <c>true</c> if there is any link to display; otherwise, <c>false</c>.
        /// </value>
        public static bool HasData
        {
            get { return Links != null; }
        }

        /// <summary>
        /// Gets or sets the detected links.
        /// </summary>
        public static string[] Links { get; set; }
    }
}