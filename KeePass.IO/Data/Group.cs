using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace KeePass.IO.Data
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
            get { return _groups.AsReadOnly(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Group"/> is hidden.
        /// </summary>
        /// <value>
        ///   <c>true</c> if hidden; otherwise, <c>false</c>.
        /// </value>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>The ID.</value>
        public string ID { get; set; }

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

        /// <summary>
        /// Gets the parent group.
        /// </summary>
        public Group Parent { get; private set; }

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
            if (group == null)
                throw new ArgumentNullException("group");

            group.Parent = this;
            _groups.Add(group);
        }

        /// <summary>
        /// Adds the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void Add(Entry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            entry.Group = this;
            _entries.Add(entry);
        }

        public void GetPath(StringBuilder sb,
            string separator, bool includeThis)
        {
            var parent = Parent;
            if (parent != null)
                parent.GetPath(sb, separator, true);

            if (!includeThis)
                return;

            if (sb.Length > 0)
                sb.Append(separator);

            sb.Append(Name);
        }

        public void Sort()
        {
            _groups.Sort(new GroupSorter());
            _entries.Sort(new EntrySorter());
        }
    }
}