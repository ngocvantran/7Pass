using System;
using KeePass.Sources.DropBox.Api;
using KeePass.Utils;

namespace KeePass.Sources.DropBox
{
    internal class MetaListItemInfo : ListItemInfo
    {
        private readonly bool _idDir;
        private readonly string _path;

        public bool IsDir
        {
            get { return _idDir; }
        }

        public string Path
        {
            get { return _path; }
        }

        public MetaListItemInfo(MetaData data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            Title = data.Name;
            _path = data.Path;
            _idDir = data.IsDir;

            Icon = ThemeData.GetImage(
                data.IsDir ? "folder" : "entry");
        }
    }
}