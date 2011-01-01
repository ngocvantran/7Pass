using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using KeePass.IO;

namespace KeePass.Data
{
    internal class ItemConverter
    {
        private readonly bool _darkTheme;
        private readonly Database _database;
        private readonly IDictionary<int, ImageSource> _icons;
        private readonly object _lckIcons;

        public ItemConverter(Database database)
        {
            if (database == null)
                throw new ArgumentNullException("database");

            _database = database;
            _lckIcons = new object();
            _darkTheme = Theme.IsDarkTheme();
            _icons = new Dictionary<int, ImageSource>();
        }

        public IEnumerable<DatabaseItem> Convert(
            IEnumerable<Entry> entries, Dispatcher dispatcher)
        {
            if (entries == null)
                throw new ArgumentNullException("entries");

            var icon = _darkTheme
                ? "/Images/entry.dark.png"
                : "/Images/entry.light.png";

            return entries.Select(x =>
                new DatabaseItem
                {
                    Source = x,
                    Title = x.Title,
                    Notes = x.Notes,
                    Icon = icon,
                    Overlay = GetSource(x.Icon, dispatcher),
                });
        }

        public IEnumerable<DatabaseItem> Convert(
            IEnumerable<Group> groups, Dispatcher dispatcher)
        {
            if (groups == null)
                throw new ArgumentNullException("groups");

            var icon = _darkTheme
                ? "/Images/group.dark.png"
                : "/Images/group.light.png";

            return groups.Select(x =>
                new DatabaseItem
                {
                    Source = x,
                    Title = x.Name,
                    Icon = icon,
                    Overlay = GetSource(x.Icon, dispatcher),
                });
        }

        private ImageSource GetSource(
            IconData data, Dispatcher dispatcher)
        {
            if (data.Custom == Guid.Empty)
            {
                ImageSource source;
                var index = data.Standard;

                if (_icons.TryGetValue(index, out source))
                    return source;

                lock (_lckIcons)
                {
                    if (_icons.TryGetValue(index, out source))
                        return source;

                    var wait = new ManualResetEvent(false);
                    dispatcher.BeginInvoke(() =>
                    {
                        source = new BitmapImage(new Uri(
                            string.Format("/Icons/{0:00}.png", index),
                            UriKind.Relative));
                        wait.Set();
                    });

                    wait.WaitOne();
                    _icons.Add(index, source);

                    return source;
                }
            }

            return _database.Icons[data.Custom];
        }
    }
}