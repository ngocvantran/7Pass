using System;
using System.Collections.Generic;

namespace KeePass.IO
{
    public class Database
    {
        private readonly IDictionary<Guid, Entry> _entries;
        private readonly IDictionary<Guid, Group> _groups;

        private readonly Group _root;

        public IEnumerable<Entry> Entries
        {
            get { return _entries.Values; }
        }

        public IEnumerable<Group> Groups
        {
            get { return _groups.Values; }
        }

        public Group Root
        {
            get { return _root; }
        }

        public Database(Group root)
        {
            if (root == null)
                throw new ArgumentNullException("root");

            _root = root;
            _groups = new Dictionary<Guid, Group>();
            _entries = new Dictionary<Guid, Entry>();

            Index(root);
        }

        public Entry GetEntry(Guid id)
        {
            Entry entry;
            return _entries.TryGetValue(id, out entry)
                ? entry : null;
        }

        public Group GetGroup(Guid id)
        {
            Group group;
            return _groups.TryGetValue(id, out group)
                ? group : null;
        }

        private void Index(Group group)
        {
            _groups.Add(group.ID, group);

            foreach (var entry in group.Entries)
                _entries.Add(entry.ID, entry);

            foreach (var child in group.Groups)
                Index(child);
        }
    }
}