using System;
using System.Globalization;
using DropNet.Models;
using KeePass.Data;
using KeePass.Utils;

namespace KeePass.Sources.DropBox
{
    internal class MetaListItemInfo : ListItemInfo
    {
        private readonly bool _idDir;
        private readonly string _modified;
        private readonly string _path;

        public bool IsDir
        {
            get { return _idDir; }
        }

        public string Modified
        {
            get { return _modified; }
        }

        public string Path
        {
            get { return _path; }
        }

        public MetaListItemInfo(MetaData data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            _path = data.Path;
            _idDir = data.Is_Dir;
            _modified = data.Modified;

            Title = data.Name;
            Notes = GetRelativeTime(data);
            Icon = ThemeData.GetImage(
                data.Is_Dir ? "folder" : "entry");
        }

        private static string GetRelativeTime(MetaData data)
        {
            DateTime date;
            var parsed = DateTime.TryParseExact(
                data.Modified,
                "ddd, dd MMM yyyy HH:mm:ss +ffff",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out date);

            return parsed
                ? date.ToRelative()
                : string.Empty;
        }
    }
}