using System;
using KeePass.Data;
using KeePass.Utils;

namespace KeePass.Sources.SkyDrive
{
    internal class ParentItem : ListItemInfo
    {
        private readonly string _path;

        public string Path
        {
            get { return _path; }
        }

        public ParentItem(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            _path = path;
            Title = "..";
            Notes = "Parent Folder";
            Icon = ThemeData.GetImage("folder");
        }
    }
}