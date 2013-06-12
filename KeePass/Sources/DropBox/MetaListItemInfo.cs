using System;
using System.Globalization;
using DropNet.Models;
using KeePass.Data;
using KeePass.Utils;

namespace KeePass.Sources.DropBox
{
    internal class MetaListItemInfo : ListItemInfo, IListItem
    {
        private readonly bool _isDir;
        private readonly string _modified;
        private readonly string _path;
        private readonly long _size;

        public bool IsDir
        {
            get { return _isDir; }
        }

        public string Modified
        {
            get { return _modified; }
        }

        public string Path
        {
            get { return _path; }
        }

        public long Size
        {
            get { return _size; }
        }

        public MetaListItemInfo(MetaData data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            _path = data.Path;
            _size = data.Bytes;
            _isDir = data.Is_Dir;
            _modified = data.Modified;

            Title = data.Name;
            Notes = GetRelativeTime(data);
            Icon = ThemeData.GetImage(
                data.Is_Dir ? "folder" : "entry");
        }

        public MetaListItemInfo(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            _path = path;
            _isDir = true;
            _modified = string.Empty;

            Title = "Parent Folder";
            Icon = ThemeData.GetImage("parent");
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