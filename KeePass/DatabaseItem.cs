using System;
using KeePass.Storage;

namespace KeePass
{
    public class DatabaseItem
    {
        private readonly DatabaseInfo _info;

        public object Info
        {
            get { return _info; }
        }

        public string Name
        {
            get { return _info.Details.Name; }
        }

        internal DatabaseItem(DatabaseInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            _info = info;

            if (info.Details == null)
                info.LoadDetails();
        }
    }
}