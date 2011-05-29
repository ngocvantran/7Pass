using System;

namespace KeePass.IO.Data
{
    public class IconData
    {
        /// <summary>
        /// Gets or sets the custom icon id.
        /// </summary>
        /// <value>
        /// The custom icon id.
        /// </value>
        public string Custom { get; set; }

        /// <summary>
        /// Gets or sets the standard icon index.
        /// This value is used when <see cref="Custom"/> is <c>null</c>..
        /// </summary>
        /// <value>
        /// The standard icon index.
        /// </value>
        public int Standard { get; set; }
    }
}