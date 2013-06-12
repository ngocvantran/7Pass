using System;
using System.Globalization;
using System.Xml.Linq;
using KeePass.Data;
using KeePass.Utils;

namespace KeePass.Sources.SkyDrive
{
    internal class MetaListItemInfo : ListItemInfo
    {
        private readonly bool _isDir;
        private readonly string _modified;
        private readonly string _parent;
        private readonly string _path;
        private readonly int _size;

        public bool IsDir
        {
            get { return _isDir; }
        }

        public string Modified
        {
            get { return _modified; }
        }

        public string Parent
        {
            get { return _parent; }
        }

        public string Path
        {
            get { return _path; }
        }

        public int Size
        {
            get { return _size; }
        }

        public MetaListItemInfo(XElement node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            _path = node.GetValue("id");
            _parent = node.GetValue("parent_id");
            _modified = node.GetValue("updated_time");
            _isDir = node.GetValue("type") == "folder";
            int.TryParse(node.GetValue("size"), out _size);

            Title = node.GetValue("name");
            Notes = GetRelativeTime(_modified);
            Icon = ThemeData.GetImage(_isDir
                ? "folder" : "entry");
        }

        public MetaListItemInfo AsParent()
        {
            var clone = (MetaListItemInfo)
                MemberwiseClone();
            clone.Title = "..";

            return clone;
        }

        private static string GetRelativeTime(string time)
        {
            try
            {
                var date = DateTime.ParseExact(time,
                    "yyyy-MM-ddTHH:mm:sszzzz",
                    CultureInfo.InvariantCulture);

                return date.ToRelative();
            }
            catch
            {
                return time;
            }
        }
    }
}