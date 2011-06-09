using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Gets or sets a value indicating whether
        /// Recycle Bin is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool RecycleBinEnabled { get; set; }

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
        /// Adds the new entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="groupId">The group id.</param>
        public void AddNew(Entry entry, string groupId)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            var group = !string.IsNullOrEmpty(groupId)
                ? _groups[groupId] : _root;

            entry.ID = Uuid.NewUuid();
            entry.Group = group;

            group.Entries.Add(entry);
            _entries.Add(entry.ID, entry);
        }

        /// <summary>
        /// Adds the new group.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="name">The name.</param>
        public Group AddNew(Group parent, string name)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            var group = new Group
            {
                Name = name,
            };

            parent.Add(group);
            group.ID = Uuid.NewUuid();

            _groups.Add(group.ID, group);

            return group;
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

        /// <summary>
        /// Removes the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void Remove(Entry entry)
        {
            entry.Remove();
            _entries.Remove(entry.ID);
        }

        /// <summary>
        /// Removes the specified entry.
        /// </summary>
        /// <param name="group">The group.</param>
        public void Remove(Group group)
        {
            foreach (var entry in group.Entries.ToList())
                Remove(entry);

            foreach (var child in group.Groups.ToList())
                Remove(child);

            group.Remove();
            _groups.Remove(group.ID);
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