using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using KeePass.IO;

namespace KeePass.Storage
{
    internal static class Cache
    {
        private const string KEY_DATABASE = "Database";

        private static readonly IsolatedStorageSettings _appSettings;
        private static readonly Dictionary<int, ImageSource> _standards;

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        public static Database Database { get; private set; }

        static Cache()
        {
            _appSettings = IsolatedStorageSettings
                .ApplicationSettings;
            _standards = new Dictionary<int, ImageSource>();
        }

        public static void CacheDb(string name, Database database)
        {
            _standards.Clear();
            Database = database;

            _appSettings[KEY_DATABASE] = name;
            _appSettings.Save();
        }

        public static void Clear()
        {
            Database = null;
            _standards.Clear();

            _appSettings.Remove(KEY_DATABASE);
            _appSettings.Save();
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
                Database.Icons.TryGetValue(icon.Custom, out source))
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

        public static void RestoreCache(Dispatcher dispatcher)
        {
            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");

            string name;
            if (!_appSettings.TryGetValue(KEY_DATABASE, out name) ||
                string.IsNullOrEmpty(name))
                return;

            var info = new DatabaseInfo(name);
            if (!info.HasPassword)
                return;

            info.Open(dispatcher);
        }
    }
}