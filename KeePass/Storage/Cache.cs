using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using KeePass.IO.Data;

namespace KeePass.Storage
{
    internal static class Cache
    {
        private const string KEY_DATABASE = "Database";

        private static readonly IsolatedStorageSettings _appSettings;
        private static readonly object _lckStandards;
        private static readonly Dictionary<int, ImageSource> _standards;
        private static DatabaseInfo _info;

        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <value>
        /// The database.
        /// </value>
        public static Database Database { get; private set; }

        /// <summary>
        /// Gets the detailed information for <see cref="Database"/>.
        /// </summary>
        public static DatabaseInfo DbInfo
        {
            get { return _info; }
        }

        static Cache()
        {
            _lckStandards = new object();
            _appSettings = IsolatedStorageSettings
                .ApplicationSettings;
            _standards = new Dictionary<int, ImageSource>();
        }

        /// <summary>
        /// Adds the specified entry id to the recently viewed entries list.
        /// </summary>
        /// <param name="entryId">The entry id.</param>
        public static void AddRecent(string entryId)
        {
            var recents = _info.Details.Recents;

            recents.Remove(entryId);
            recents.Insert(0, entryId);

            if (recents.Count == 10)
                recents.RemoveAt(9);

            _info.SaveDetails();
        }

        public static void CacheDb(DatabaseInfo info,
            string name, Database database)
        {
            _info = info;
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
        /// Clears the recently viewed entries list.
        /// </summary>
        public static void ClearRecents()
        {
            _info.Details.Recents.Clear();
            _info.SaveDetails();
        }

        /// <summary>
        /// Gets the overlay icon.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="icon">The icon information.</param>
        /// <returns>
        /// The overlay icon.
        /// </returns>
        public static ImageSource GetOverlay(
            Dispatcher dispatcher, IconData icon)
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
                lock (_lckStandards)
                {
                    if (!_standards.TryGetValue(id, out source))
                    {
                        var wait = new ManualResetEvent(false);

                        dispatcher.BeginInvoke(() =>
                        {
                            var uri = string.Format(
                                "/Images/KeePass/{0:00}.png", id);
                            source = new BitmapImage(new Uri(
                                uri, UriKind.Relative));
                            _standards.Add(id, source);

                            wait.Set();
                        });

                        wait.WaitOne();
                    }
                }
            }

            return source;
        }

        /// <summary>
        /// Gets the recently viewed entries.
        /// </summary>
        /// <returns></returns>
        public static string[] GetRecents()
        {
            return _info.Details
                .Recents.ToArray();
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

        /// <summary>
        /// Updates the recents.
        /// </summary>
        public static void UpdateRecents()
        {
            var database = Database;
            var recents = _info.Details.Recents;

            var removed = recents
                .Where(x => database.GetEntry(x) == null)
                .ToArray();

            foreach (var entry in removed)
                recents.Remove(entry);

            _info.SaveDetails();
        }
    }
}