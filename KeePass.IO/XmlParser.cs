using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using KeePass.IO.Utils;

namespace KeePass.IO
{
    internal class XmlParser
    {
        private readonly CryptoRandomStream _crypto;

        public XmlParser(CryptoRandomStream crypto)
        {
            if (crypto == null)
                throw new ArgumentNullException("crypto");
            _crypto = crypto;
        }

        public Database Parse(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            var settings = new XmlReaderSettings
            {
                CloseInput = false,
                IgnoreComments = true,
                IgnoreWhitespace = true,
                IgnoreProcessingInstructions = true,
            };

            using (var reader = XmlReader.Create(stream, settings))
            {
                if (!reader.ReadToFollowing("KeePassFile"))
                    return null;

                if (!reader.ReadToDescendant("Meta"))
                    return null;

                IDictionary<string, ImageSource> icons;
                using (var subReader = reader.ReadSubtree())
                    icons = ParseIcons(subReader);

                if (reader.Name != "Root" &&
                    !reader.ReadToNextSibling("Root"))
                {
                    return null;
                }

                if (!reader.ReadToDescendant("Group"))
                    return null;

                using (var subReader = reader.ReadSubtree())
                {
                    var root = ParseGroup(subReader);
                    return new Database(root, icons);
                }
            }
        }

        private static bool IsProtected(XmlReader reader)
        {
            if (!reader.HasAttributes)
                return false;

            if (!reader.MoveToAttribute("Protected"))
                return false;

            return reader.Value == "True";
        }

        private void ParseChildren(XmlReader reader, Group group)
        {
            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "Group":
                        using (var subReader = reader.ReadSubtree())
                            group.Add(ParseGroup(subReader));
                        reader.ReadEndElement();
                        break;

                    case "Entry":
                        using (var subReader = reader.ReadSubtree())
                            group.Add(ParseEntry(subReader));
                        reader.ReadEndElement();
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }

            group.Sort();
        }

        private Entry ParseEntry(XmlReader reader)
        {
            var id = ReadId(reader);
            var icon = ParseIcon(reader);

            var lastModified = ReadLastModified(reader);
            var fields = ReadFields(reader);

            if (fields.Count == 0)
                return null;

            var histories = ReadHistories(reader);
            var entry = new Entry(fields)
            {
                ID = id,
                Icon = icon,
                Histories = histories,
                LastModified = lastModified,
            };

            return entry;
        }

        private Group ParseGroup(XmlReader reader)
        {
            var id = ReadId(reader);

            if (reader.Name != "Name")
                reader.ReadToNextSibling("Name");

            var name = reader
                .ReadElementContentAsString();

            var icon = ParseIcon(reader);

            var group = new Group
            {
                ID = id,
                Name = name,
                Icon = icon,
            };

            ParseChildren(reader, group);

            return group;
        }

        private static IconData ParseIcon(XmlReader reader)
        {
            var data = new IconData();

            if (reader.Name != "IconID")
                reader.ReadToNextSibling("IconID");

            data.Standard = reader
                .ReadElementContentAsInt();

            if (reader.Name == "CustomIconUUID")
            {
                data.Custom = reader
                    .ReadElementContentAsString();
            }

            return data;
        }

        private static IDictionary<string, ImageSource>
            ParseIcons(XmlReader reader)
        {
            var icons = new Dictionary<string, ImageSource>();

            if (reader.ReadToFollowing("CustomIcons"))
            {
                while (reader.ReadToFollowing("UUID"))
                {
                    var id = reader.ReadElementContentAsString();

                    if (reader.Name != "Data" &&
                        !reader.ReadToNextSibling("Data"))
                    {
                        continue;
                    }

                    var data = Convert.FromBase64String(
                        reader.ReadElementContentAsString());

                    var image = new BitmapImage();
                    image.SetSource(new MemoryStream(data));

                    icons.Add(id, image);
                }
            }

            return icons;
        }

        private Dictionary<string, string>
            ReadFields(XmlReader reader)
        {
            var fields = new Dictionary<string, string>();

            if (reader.ReadToFollowing("String"))
            {
                while (reader.Name == "String")
                {
                    reader.Read();

                    var name = reader.ReadElementContentAsString();
                    fields.Add(name, ReadValue(reader));

                    reader.ReadEndElement();
                }
            }

            return fields;
        }

        private List<Entry> ReadHistories(XmlReader reader)
        {
            var histories = new List<Entry>();

            if (reader.ReadToFollowing("History"))
            {
                if (reader.ReadToDescendant("Entry"))
                {
                    while (reader.Name == "Entry")
                    {
                        using (var subReader = reader.ReadSubtree())
                        {
                            var history = ParseEntry(subReader);
                            if (history != null)
                                histories.Add(history);
                        }

                        reader.ReadEndElement();
                    }
                }
            }

            return histories;
        }

        private static string ReadId(XmlReader reader)
        {
            reader.ReadToDescendant("UUID");
            return reader.ReadElementContentAsString();
        }

        private static DateTime ReadLastModified(XmlReader reader)
        {
            if (reader.ReadToFollowing("Times"))
            {
                using (var subReader = reader.ReadSubtree())
                {
                    if (subReader.ReadToDescendant("LastModificationTime"))
                        return subReader.ReadElementContentAsDateTime();
                }
            }

            return DateTime.MinValue;
        }

        private string ReadValue(XmlReader reader)
        {
            var isProtected = IsProtected(reader);
            reader.MoveToContent();
            var value = reader.ReadElementContentAsString();

            if (isProtected && !string.IsNullOrEmpty(value))
            {
                var encrypted = Convert
                    .FromBase64String(value);
                var pad = _crypto.GetRandomBytes(
                    (uint)encrypted.Length);

                for (var i = 0; i < encrypted.Length; i++)
                    encrypted[i] ^= pad[i];

                value = Encoding.UTF8.GetString(
                    encrypted, 0, encrypted.Length);
            }

            return value;
        }
    }
}