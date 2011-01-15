using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using KeePass.IO;

namespace KeePass.Storage
{
    internal static class Cache
    {
        private static readonly Dictionary<int, ImageSource> _standards;

        private static Database _database;

        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        public static Database Database
        {
            get { return _database; }
            set
            {
                _database = value;
                _standards.Clear();
            }
        }

        static Cache()
        {
            _standards = new Dictionary<int, ImageSource>();
        }

        /// <summary>
        /// Gets the overlay icon.
        /// </summary>
        /// <param name="icon">The icon information.</param>
        /// <returns>The overlay icon.</returns>
        public static ImageSource GetOverlay(IconData icon)
        {
            if (icon == null)
                throw new ArgumentNullException("icon");

            ImageSource source;
            if (!string.IsNullOrEmpty(icon.Custom) &&
                _database.Icons.TryGetValue(icon.Custom, out source))
                return source;

            var id = icon.Standard;
            if (!_standards.TryGetValue(id, out source))
            {
                var uri = string.Format(
                    "/Images/KeePass/{0:00}.png", id);
                source = new BitmapImage(new Uri(
                    uri, UriKind.Relative));
                _standards.Add(id, source);
            }

            return source;
        }
    }
}