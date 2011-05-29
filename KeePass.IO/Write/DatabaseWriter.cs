using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using KeePass.IO.Data;

namespace KeePass.IO.Write
{
    /// <summary>
    /// Implements database modification.
    /// How to use:
    /// <list>
    /// <item><see cref="Load"/></item>
    /// <item><see cref="Decrypt"/></item>
    /// <item>Perform modifications</item>
    /// <item><see cref="Encrypt"/></item>
    /// <item><see cref="Save"/></item>
    /// </list>
    /// </summary>
    public class DatabaseWriter
    {
        private XDocument _doc;
        private IDictionary<string, XElement> _entries;
        private IDictionary<string, XElement> _groups;

        /// <summary>
        /// Decrypts the protected fields.
        /// </summary>
        public void Decrypt()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the details of the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void Details(Entry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            var element = _entries[entry.ID];

            element
                .Element("History")
                .Add(Clone(element));

            var fields = entry.GetAllFields();
            var existings = element
                .Elements("String")
                .ToDictionary(x => x.Element("Key").Value);

            foreach (var pair in fields)
            {
                XElement existing;
                if (existings.TryGetValue(pair.Key, out existing))
                    existing.Value = pair.Value;
                else
                {
                    element.Add(new XElement("String",
                        new XElement("Key", pair.Key),
                        new XElement("Value", pair.Value)));
                }
            }

            var removes = existings.Keys
                .Except(fields.Keys)
                .ToList();

            foreach (var remove in removes)
                existings[remove].Remove();

            element
                .Element("Times")
                .Element("LastModificationTime")
                .Value = GetTime();
        }

        /// <summary>
        /// Updates the details of the specified group.
        /// </summary>
        /// <param name="group">The group.</param>
        public void Details(Group group)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            var element = _groups[group.ID];

            element
                .Element("Name")
                .Value = group.Name;

            element
                .Element("Times")
                .Element("LastModificationTime")
                .Value = GetTime();
        }

        /// <summary>
        /// Encrypts the protected fields.
        /// </summary>
        public void Encrypt()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads the specified XML stream.
        /// </summary>
        /// <param name="xml">The XML stream.</param>
        public void Load(Stream xml)
        {
            if (xml == null)
                throw new ArgumentNullException("xml");

            _doc = XDocument.Load(xml);

            _groups = _doc.Descendants("Group")
                .Select(x => new
                {
                    Element = x,
                    Id = x.Element("UUID"),
                })
                .Where(x => x.Id != null)
                .ToDictionary(x => x.Id.Value, x => x.Element);

            _entries = _doc.Descendants("Entry")
                .Select(x => new
                {
                    x.Parent,
                    Element = x,
                    Id = x.Element("UUID"),
                })
                .Where(x => x.Id != null)
                .Where(x => x.Parent.Name != "History")
                .ToDictionary(x => x.Id.Value, x => x.Element);
        }

        /// <summary>
        /// Updates the location of the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void Location(Entry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            var element = _entries[entry.ID];
            element.Remove();

            var parent = _groups[entry.Group.ID];
            parent.Add(element);

            element
                .Element("Times")
                .Element("LocationChanged")
                .Value = GetTime();
        }

        /// <summary>
        /// Updates the location of the specified group.
        /// </summary>
        /// <param name="group">The group.</param>
        public void Location(Group group)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            var element = _groups[group.ID];
            element.Remove();

            var parent = _groups[group.Parent.ID];
            parent.Add(element);

            element
                .Element("Times")
                .Element("LocationChanged")
                .Value = GetTime();
        }

        /// <summary>
        /// Saves the new group.
        /// </summary>
        /// <param name="group">The group.</param>
        public void New(Group group)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            var time = GetTime();

            var element = new XElement("Group",
                new XElement("UUID", group.ID),
                new XElement("Name", group.Name),
                new XElement("Notes"),
                new XElement("IconID", 0),
                new XElement("Times",
                    new XElement("LastModificationTime", time),
                    new XElement("CreationTime", time),
                    new XElement("LastAccessTime", time),
                    new XElement("ExpiryTime", time),
                    new XElement("Expires", false),
                    new XElement("UsageCount", 0),
                    new XElement("LocationChanged", time)),
                new XElement("IsExpanded", true),
                new XElement("DefaultAutoTypeSequence"),
                new XElement("EnableAutoType", "null"),
                new XElement("EnableSearching", "null"),
                new XElement("LastTopVisibleEntry", "AAAAAAAAAAAAAAAAAAAAAA=="));

            var parent = _groups[group.Parent.ID];
            parent.Add(element);

            parent
                .Element("Times")
                .Element("LastModificationTime")
                .Value = GetTime();

            _groups.Add(group.ID, element);
        }

        /// <summary>
        /// Saves the new entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void New(Entry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            var time = GetTime();

            var element = new XElement("Entry",
                new XElement("UUID", entry.ID),
                new XElement("IconID", 0),
                new XElement("ForegroundColor"),
                new XElement("BackgroundColor"),
                new XElement("OverrideURL"),
                new XElement("Tags"),
                new XElement("Times",
                    new XElement("LastModificationTime", time),
                    new XElement("CreationTime", time),
                    new XElement("LastAccessTime", time),
                    new XElement("ExpiryTime", time),
                    new XElement("Expires", false),
                    new XElement("UsageCount", 0),
                    new XElement("LocationChanged", time)));

            var fields = entry.GetAllFields();
            element.Add(fields
                .Select(x =>
                    new XElement("String",
                        new XElement("Key", x.Key),
                        new XElement("Value", x.Value, x.Key == "Password"
                            ? new XAttribute("Protected", true)
                            : null))));

            element.Add(
                new XElement("AutoType",
                    new XElement("Enabled", true),
                    new XElement("DataTransferObfuscation", 0),
                    new XElement("Association",
                        new XElement("Window", "Target Window"),
                        new XElement("KeystrokeSequence", "{USERNAME}{TAB}{PASSWORD}{TAB}{ENTER}"))),
                new XElement("History"));

            var group = _groups[entry.Group.ID];
            group.Add(element);

            group
                .Element("Times")
                .Element("LastModificationTime")
                .Value = GetTime();

            _entries.Add(entry.ID, element);
        }

        /// <summary>
        /// Saves the updated XML to the specified stream.
        /// </summary>
        /// <param name="xml">The XML.</param>
        public void Save(Stream xml)
        {
            if (xml == null)
                throw new ArgumentNullException("xml");

            _doc.Save(xml);
        }

        private static XElement Clone(XElement element)
        {
            return new XElement(element.Name,
                element.Attributes(),
                element.Nodes().Select(n =>
                {
                    var e = n as XElement;

                    if (e != null)
                    {
                        return e.Name != "History"
                            ? Clone(e) : null;
                    }

                    return n;
                })
                    .Where(x => x != null));
        }

        private static string GetTime()
        {
            return XmlConvert.ToString(DateTime.Now,
                XmlDateTimeSerializationMode.RoundtripKind);
        }
    }
}