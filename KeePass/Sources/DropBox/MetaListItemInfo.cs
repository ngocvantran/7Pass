using System;
using KeePass.Sources.DropBox.Api;
using KeePass.Utils;

namespace KeePass.Sources.DropBox
{
    internal class MetaListItemInfo : ListItemInfo
    {
        private readonly string _path;

        public string Path
        {
            get { return _path; }
        }

        public MetaListItemInfo(MetaData data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            _path = data.Path;
            Title = data.Name;
            Icon = ThemeData.GetImage(
                data.IsDir ? "folder" : "entry");
        }
    }
}