using System;
using KeePass.Data;
using KeePass.Sources.WebDav.Api;

namespace KeePass.Sources.WebDav
{
    internal class MetaListItemInfo : ListItemInfo
    {
        private readonly ItemInfo _item;
        private readonly string _path;

        public bool IsDir
        {
            get { return _item.IsDir; }
        }

        public string Modified
        {
            get { return _item.Modified; }
        }

        public string Path
        {
            get { return _path; }
        }

        public MetaListItemInfo(string basePath, ItemInfo item)
        {
            if (basePath == null) throw new ArgumentNullException("basePath");
            if (item == null) throw new ArgumentNullException("item");

            _item = item;
            _path = new Uri(new Uri(basePath), item.Path).ToString();

            var path = item.Path;

            try
            {
                Title = System.IO.Path.GetFileName(path);
            }
            catch (ArgumentException)
            {
                Title = path;
            }
        }
    }
}