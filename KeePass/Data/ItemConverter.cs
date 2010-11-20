using System;
using System.Collections.Generic;
using System.Linq;
using KeePass.IO;

namespace KeePass.Data
{
    internal class ItemConverter
    {
        private readonly bool _darkTheme;

        public ItemConverter()
        {
            _darkTheme = Theme.IsDarkTheme();
        }

        public IEnumerable<DatabaseItem> Convert(
            IEnumerable<Entry> entries)
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
                    Icon = icon,
                    Title = x.Title,
                    Notes = x.Notes,
                });
        }

        public IEnumerable<DatabaseItem> Convert(
            IEnumerable<Group> groups)
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
                    Icon = icon,
                    Title = x.Name,
                });
        }
    }
}