using System;
using KeePass.Data;
using KeePass.Sources.WebDav.Api;

namespace KeePass.Sources.WebDav
{
    internal class MetaListItemInfo : ListItemInfo
    {
        private readonly ItemInfo _item;

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
            get { return _item.Path; }
        }

        public MetaListItemInfo(ItemInfo item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            _item = item;
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