using System;
using System.Windows.Media;

namespace KeePass.Data
{
    public class DatabaseItem
    {
        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        /// <value>The notes.</value>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the overlay icon.
        /// </summary>
        /// <value>
        /// The overlay icon.
        /// </value>
        public ImageSource Overlay { get; set; }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>The source.</value>
        public object Source { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Title { get; set; }
    }
}