using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace KeePass.IO.Data
{
    public class Database
    {
        private readonly IDictionary<string, ImageSource> _customIcons;
        private readonly IDictionary<string, Entry> _entries;
        private readonly IDictionary<string, Group> _groups;

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
        public IDictionary<string, ImageSource> Icons
        {
            get { return _customIcons; }
        }

        /// <summary>
        /// Gets or sets the recycle bin.
        /// </summary>
        /// <value>
        /// The recycle bin.
        /// </value>
        public Group RecycleBin { get; set; }

        /// <summary>
        /// Gets the root group.
        /// </summary>
        public Group Root
        {
            get { return _root; }
        }

        public Database(Group root,
            IDictionary<string, ImageSource> customIcons)
        {
            if (root == null) throw new ArgumentNullException("root");
            if (customIcons == null) throw new ArgumentNullException("customIcons");

            _root = root;
            _customIcons = customIcons;
            _groups = new Dictionary<string, Group>();
            _entries = new Dictionary<string, Entry>();

            Index(root);
        }

        /// <summary>
        /// Gets the specified entry.
        /// </summary>
        /// <param name="id">The entry id.</param>
        /// <returns>Entry with the specified id, or <c>null</c> if not found.</returns>
        public Entry GetEntry(string id)
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
        public Group GetGroup(string id)
        {
            Group group;
            return _groups.TryGetValue(id, out group)
                ? group : null;
        }

        private void Index(Group group)
        {
            _groups.AddOrSet(group.ID, group);

            foreach (var entry in group.Entries)
                _entries.AddOrSet(entry.ID, entry);

            foreach (var child in group.Groups)
                Index(child);
        }
    }
}