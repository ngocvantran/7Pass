using System;
using System.Windows.Threading;
using KeePass.IO;
using KeePass.Storage;
using KeePass.Utils;

namespace KeePass.Data
{
    public class GroupItem : ListItemInfo
    {
        private readonly Uri _targetUri;

        public Uri TargetUri
        {
            get { return _targetUri; }
        }

        public GroupItem(Group group, Dispatcher dispatcher)
        {
            if (group == null) throw new ArgumentNullException("group");
            if (dispatcher == null) throw new ArgumentNullException("dispatcher");

            Title = group.Name;
            Icon = ThemeData.GetImage("folder");
            Overlay = Cache.GetOverlay(dispatcher, group.Icon);

            _targetUri = Navigation.GetPathTo
                <GroupDetails>("id={0}", group.ID);
        }

        public GroupItem(Entry entry, Dispatcher dispatcher)
        {
            if (entry == null) throw new ArgumentNullException("entry");
            if (dispatcher == null) throw new ArgumentNullException("dispatcher");

            Title = entry.Title;
            Notes = entry.Notes;
            Icon = ThemeData.GetImage("entry");
            Overlay = Cache.GetOverlay(dispatcher, entry.Icon);

            _targetUri = Navigation.GetPathTo
                <EntryDetails>("id={0}", entry.ID);
        }
    }
}