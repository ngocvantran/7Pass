using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KeePass.IO
{
    [DebuggerDisplay("Group {Name}, {Groups.Count} groups, {Entries.Count} entries")]
    public class Group
    {
        private readonly List<Entry> _entries;
        private readonly List<Group> _groups;

        /// <summary>
        /// Gets the entries.
        /// </summary>
        /// <value>The entries.</value>
        public ICollection<Entry> Entries
        {
            get { return _entries; }
        }

        /// <summary>
        /// Gets the child groups.
        /// </summary>
        /// <value>The child groups.</value>
        public ICollection<Group> Groups
        {
            get { return _groups; }
        }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>The ID.</value>
        public Guid ID { get; set; }

        /// <summary>
        /// Gets or sets the icon data.
        /// </summary>
        /// <value>
        /// The icon data.
        /// </value>
        public IconData Icon { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        public Group()
        {
            _groups = new List<Group>();
            _entries = new List<Entry>();
        }

        /// <summary>
        /// Adds the specified group.
        /// </summary>
        /// <param name="group">The group.</param>
        public void Add(Group group)
        {
            _groups.Add(group);
        }

        /// <summary>
        /// Adds the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void Add(Entry entry)
        {
            _entries.Add(entry);
        }

        public void Sort()
        {
            _groups.Sort(new GroupSorter());
            _entries.Sort(new EntrySorter());
        }
    }
}