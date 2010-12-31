using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace KeePass.IO
{
    public class Database
    {
        private readonly IDictionary<Guid, ImageSource> _customIcons;
        private readonly IDictionary<Guid, Entry> _entries;
        private readonly IDictionary<Guid, Group> _groups;

        private readonly Group _root;

        /// <summary>
        /// Gets all entries.
        /// </summary>
        public IEnumerable<Entry> Entries
        {
            get { return _entries.Values; }
        }

        /// <summary>
        /// Gets the flattened list of groups.
        /// </summary>
        public IEnumerable<Group> Groups
        {
            get { return _groups.Values; }
        }

        /// <summary>
        /// Gets the custom icons.
        /// </summary>
        public IDictionary<Guid, ImageSource> Icons
        {
            get { return _customIcons; }
        }

        /// <summary>
        /// Gets the root group.
        /// </summary>
        public Group Root
        {
            get { return _root; }
        }

        public Database(Group root,
            IDictionary<Guid, ImageSource> customIcons)
        {
            if (root == null) throw new ArgumentNullException("root");
            if (customIcons == null) throw new ArgumentNullException("customIcons");

            _root = root;
            _customIcons = customIcons;
            _groups = new Dictionary<Guid, Group>();
            _entries = new Dictionary<Guid, Entry>();

            Index(root);
        }

        /// <summary>
        /// Gets the specified entry.
        /// </summary>
        /// <param name="id">The entry id.</param>
        /// <returns>Entry with the specified id, or <c>null</c> if not found.</returns>
        public Entry GetEntry(Guid id)
        {
            Entry entry;
            return _entries.TryGetValue(id, out entry)
                ? entry : null;
        }

        /// <summary>
        /// Gets the specified group.
        /// </summary>
        /// <param name="id">The group id.</param>
        /// <returns>Group with the specified id, or <c>null</c> if not found.</returns>
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