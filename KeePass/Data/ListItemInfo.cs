using System;
using System.Windows;

namespace KeePass.Data
{
    public class ListItemInfo
    {
        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>
        /// The icon.
        /// </value>
        public string Icon { get; set; }

        /// <summary>
        /// Gets the state of the icon.
        /// </summary>
        /// <value>
        /// The state of the icon.
        /// </value>
        public Visibility IconState
        {
            get
            {
                return string.IsNullOrEmpty(Icon)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        /// <value>
        /// The notes.
        /// </value>
        public string Notes { get; set; }

        /// <summary>
        /// Gets the state of the notes.
        /// </summary>
        /// <value>
        /// The state of the notes.
        /// </value>
        public Visibility NotesState
        {
            get
            {
                return string.IsNullOrEmpty(Notes)
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        /// <summary>
        /// Gets or sets the overlay icon.
        /// </summary>
        /// <value>
        /// The overlay icon.
        /// </value>
        public string Overlay { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; }
    }
}