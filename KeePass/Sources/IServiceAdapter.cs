using System;
using KeePass.Storage;

namespace KeePass.Sources
{
    internal interface IServiceAdapter
    {
        event SynchronizeErrorEventHandler Error;

        void Conflict(ListItem item,
            Action<ListItem, string> uploaded);

        void Download(ListItem item,
            Action<ListItem, byte[]> downloaded);

        SyncInfo Initialize(DatabaseInfo info);

        void List(Action<ListItem> ready);

        void Upload(ListItem item,
            Action<ListItem> uploaded);
    }
}