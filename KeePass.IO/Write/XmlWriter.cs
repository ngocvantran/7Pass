using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using KeePass.IO.Data;
using KeePass.IO.Utils;

namespace KeePass.IO.Write
{
    // ReSharper disable PossibleNullReferenceException

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
    internal class XmlWriter
    {
        private XElement _deletedObjs;
        private XDocument _doc;
        private IDictionary<string, XElement> _entries;
        private IDictionary<string, XElement> _groups;

        /// <summary>
        /// Decrypts the protected fields.
        /// </summary>
        /// <param name="crypto">The crypto random stream.</param>
        public void Decrypt(CryptoRandomStream crypto)
        {
            if (crypto == null)
                throw new ArgumentNullException("crypto");

            var values = GetProtectedValues();

            foreach (var item in values)
            {
                item.Value = crypto
                    .Decrypt(item.Value);
            }
        }

        /// <summary>
        /// Deletes the specified group.
        /// </summary>
        /// <param name="group">The group.</param>
        public void Delete(Group group)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            var time = GetTime(DateTime.Now);

            _deletedObjs.Add(new XElement("DeletedObject",
                new XElement("UUID", group.ID),
                new XElement("DeletionTime", time)));

            _groups[group.ID].Remove();
            Remove(group);
        }

        /// <summary>
        /// Deletes the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void Delete(Entry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            var time = GetTime(DateTime.Now);
            var element = _entries[entry.ID];

            element.Remove();
            _entries.Remove(entry.ID);

            _deletedObjs.Add(new XElement("DeletedObject",
                new XElement("UUID", entry.ID),
                new XElement("DeletionTime", time)));
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
            var strings = element
                .Elements("String");

            var existings = strings
                .ToDictionary(
                    x => x.Element("Key").Value,
                    x => x.Element("Value"));
            var addTarget = strings.Last();

            foreach (var pair in fields)
            {
                XElement existing;
                if (existings.TryGetValue(pair.Key, out existing))
                    existing.Value = pair.Value;
                else
                {
                    addTarget.AddAfterSelf(new XElement("String",
                        new XElement("Key", pair.Key),
                        new XElement("Value", pair.Value)));
                }
            }

            var removes = existings.Keys
                .Except(fields.Keys)
                .ToList();

            foreach (var remove in removes)
                existings[remove].Remove();

            var time = DateTime.Now;
            entry.LastModified = time;

            element
                .Element("Times")
                .Element("LastModificationTime")
                .Value = GetTime(time);
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

            var time = DateTime.Now;
            group.LastModified = time;

            element
                .Element("Times")
                .Element("LastModificationTime")
                .Value = GetTime(time);
        }

        /// <summary>
        /// Encrypts the protected fields.
        /// </summary>
        /// <param name="crypto">The crypto random stream.</param>
        public void Encrypt(CryptoRandomStream crypto)
        {
            if (crypto == null)
                throw new ArgumentNullException("crypto");

            var values = GetProtectedValues();

            foreach (var item in values)
            {
                item.Value = crypto
                    .Encrypt(item.Value);
            }
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

            _deletedObjs = _doc.Root
                .Element("Root")
                .Element("DeletedObjects");

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

            var time = DateTime.Now;
            entry.LastModified = time;

            element
                .Element("Times")
                .Element("LocationChanged")
                .Value = GetTime(time);
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

            var time = DateTime.Now;
            group.LastModified = time;

            element
                .Element("Times")
                .Element("LocationChanged")
                .Value = GetTime(time);
        }

        /// <summary>
        /// Saves the new group.
        /// </summary>
        /// <param name="group">The group.</param>
        public void New(Group group)
        {
            if (group == null)
                throw new ArgumentNullException("group");

            var time = DateTime.Now;
            group.LastModified = time;

            var timeValue = GetTime(time);
            var element = new XElement("Group",
                new XElement("UUID", group.ID),
                new XElement("Name", group.Name),
                new XElement("Notes"),
                new XElement("IconID", 0),
                new XElement("Times",
                    new XElement("LastModificationTime", timeValue),
                    new XElement("CreationTime", timeValue),
                    new XElement("LastAccessTime", timeValue),
                    new XElement("ExpiryTime", timeValue),
                    new XElement("Expires", "False"),
                    new XElement("UsageCount", 0),
                    new XElement("LocationChanged", timeValue)),
                new XElement("IsExpanded", "True"),
                new XElement("DefaultAutoTypeSequence"),
                new XElement("EnableAutoType", "null"),
                new XElement("EnableSearching", "null"),
                new XElement("LastTopVisibleEntry", "AAAAAAAAAAAAAAAAAAAAAA=="));

            var parent = _groups[group.Parent.ID];
            parent.Add(element);

            parent
                .Element("Times")
                .Element("LastModificationTime")
                .Value = timeValue;

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

            var time = DateTime.Now;
            entry.LastModified = time;

            var timeValue = GetTime(time);
            var element = new XElement("Entry",
                new XElement("UUID", entry.ID),
                new XElement("IconID", 0),
                new XElement("ForegroundColor"),
                new XElement("BackgroundColor"),
                new XElement("OverrideURL"),
                new XElement("Tags"),
                new XElement("Times",
                    new XElement("LastModificationTime", timeValue),
                    new XElement("CreationTime", timeValue),
                    new XElement("LastAccessTime", timeValue),
                    new XElement("ExpiryTime", timeValue),
                    new XElement("Expires", "False"),
                    new XElement("UsageCount", 0),
                    new XElement("LocationChanged", timeValue)));

            var fields = entry.GetAllFields();
            element.Add(fields
                .Select(x =>
                    new XElement("String",
                        new XElement("Key", x.Key),
                        new XElement("Value", x.Value, x.Key == "Password"
                            ? new XAttribute("Protected", "True")
                            : null))));

            element.Add(
                new XElement("AutoType",
                    new XElement("Enabled", "True"),
                    new XElement("DataTransferObfuscation", 0),
                    new XElement("Association",
                        new XElement("Window", "Target Window"),
                        new XElement("KeystrokeSequence",
                            "{USERNAME}{TAB}{PASSWORD}{TAB}{ENTER}"))),
                new XElement("History"));

            var group = _groups[entry.Group.ID];
            group.Add(element);

            group
                .Element("Times")
                .Element("LastModificationTime")
                .Value = timeValue;

            _entries.Add(entry.ID, element);
        }

        /// <summary>
        /// Saves the updated XML to the specified stream.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <param name="recycleBin">The recycle bin.</param>
        public void Save(Stream xml, Group recycleBin)
        {
            if (xml == null)
                throw new ArgumentNullException("xml");

            SetRecycleBin(recycleBin);

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

        private IEnumerable<ProtectedValue> GetProtectedValues()
        {
            return _doc
                .Descendants("Value")
                .Select(x => new ProtectedValue(x))
                .Where(x => x.IsValid)
                .Where(x => x.IsProtected)
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .ToList();
        }

        private static string GetTime(DateTime time)
        {
            return XmlConvert.ToString(time,
                XmlDateTimeSerializationMode.RoundtripKind);
        }

        private void Remove(Group group)
        {
            foreach (var entry in group.Entries)
                _entries.Remove(entry.ID);

            foreach (var child in group.Groups)
                Remove(child);

            _groups.Remove(group.ID);
        }

        private void SetRecycleBin(Group recycleBin)
        {
            var recycleBinId = recycleBin != null
                ? recycleBin.ID
                : "AAAAAAAAAAAAAAAAAAAAAA==";

            var meta = _doc.Root
                .Element("Meta");

            var idElement = meta
                .Element("RecycleBinUUID");

            if (idElement.Value == recycleBinId)
                return;

            idElement.Value = recycleBinId;
            meta.Element("RecycleBinChanged")
                .Value = GetTime(DateTime.Now);
        }

        public class ProtectedValue
        {
            private readonly XElement _element;

            public bool IsProtected
            {
                get
                {
                    var attr = _element
                        .Attribute("Protected");

                    return attr != null &&
                        attr.Value == "True";
                }
            }

            public bool IsValid
            {
                get
                {
                    var parent = _element.Parent;
                    return parent != null &&
                        parent.Name == "String";
                }
            }

            public string Value
            {
                get { return _element.Value; }
                set { _element.Value = value; }
            }

            public ProtectedValue(XElement element)
            {
                if (element == null)
                    throw new ArgumentNullException("element");
                _element = element;
            }
        }
    }

    // ReSharper restore PossibleNullReferenceException
}