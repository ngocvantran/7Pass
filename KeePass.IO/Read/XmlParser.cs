using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using KeePass.IO.Data;
using KeePass.IO.Utils;

namespace KeePass.IO.Read
{
    internal class XmlParser
    {
        private readonly CryptoRandomStream _crypto;
        private readonly Dispatcher _dispatcher;
        private readonly Stream _stream;

        public XmlParser(CryptoRandomStream crypto,
            Stream stream, Dispatcher dispatcher)
        {
            if (crypto == null) throw new ArgumentNullException("crypto");
            if (stream == null) throw new ArgumentNullException("stream");
            if (dispatcher == null) throw new ArgumentNullException("dispatcher");

            _crypto = crypto;
            _stream = stream;
            _dispatcher = dispatcher;
        }

        public Database Parse()
        {
            var settings = new XmlReaderSettings
            {
                CloseInput = false,
                IgnoreComments = true,
                IgnoreWhitespace = true,
                IgnoreProcessingInstructions = true,
            };

            using (var reader = XmlReader.Create(_stream, settings))
            {
                if (!reader.ReadToFollowing("KeePassFile"))
                    return null;

                if (!reader.ReadToDescendant("Meta"))
                    return null;

                string recycleBinId = null;
                var recyleBinEnabled = false;

                var config = new DatabaseConfiguration();
                var icons = new Dictionary<string, ImageSource>();
                using (var subReader = reader.ReadSubtree())
                {
                    subReader.ReadToFollowing("Generator");

                    while (!string.IsNullOrEmpty(subReader.Name))
                    {
                        subReader.Skip();

                        switch (subReader.Name)
                        {
                            case "DefaultUserName":
                                config.DefaultUserName = subReader
                                    .ReadElementContentAsString();
                                break;

                            case "MemoryProtection":
                                using (var configReader = subReader.ReadSubtree())
                                    ParseProtection(configReader, config);
                                break;

                            case "CustomIcons":
                                using (var iconsReader = subReader.ReadSubtree())
                                    ParseIcons(iconsReader, _dispatcher, icons);
                                break;

                            case "RecycleBinEnabled":
                                var value = subReader
                                    .ReadElementContentAsString();
                                recyleBinEnabled = value == "True";
                                break;

                            case "RecycleBinUUID":
                                recycleBinId = subReader
                                    .ReadElementContentAsString();
                                break;
                        }
                    }
                }

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
                    var database = new Database(root, icons)
                    {
                        Configuration = config,
                        RecycleBinEnabled = recyleBinEnabled,
                    };

                    if (!string.IsNullOrEmpty(recycleBinId))
                    {
                        database.RecycleBin = database
                            .GetGroup(recycleBinId);
                    }

                    return database;
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
                        {
                            var entry = ParseEntry(subReader);
                            if (entry != null)
                                group.Add(entry);
                        }

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

            // Needed to ensure protected
            // fields decryption
            ReadHistories(reader);

            if (fields.Count == 0)
                return null;

            var entry = new Entry(fields)
            {
                ID = id,
                Icon = icon,
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

            var notes = string.Empty;
            if (reader.Name == "Notes" ||
                reader.ReadToNextSibling("Notes"))
            {
                notes = reader.ReadElementContentAsString();
            }

            var icon = ParseIcon(reader);
            var lastModified = ReadLastModified(reader);

            var group = new Group
            {
                ID = id,
                Name = name,
                Icon = icon,
                Notes = notes,
                LastModified = lastModified,
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

        private static void ParseIcons(
            XmlReader reader, Dispatcher dispatcher,
            IDictionary<string, ImageSource> icons)
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

                BitmapImage image = null;

                if (dispatcher.CheckAccess())
                {
                    image = new BitmapImage();
                    image.SetSource(new MemoryStream(data));
                }
                else
                {
                    var wait = new ManualResetEvent(false);

                    dispatcher.BeginInvoke(() =>
                    {
                        image = new BitmapImage();
                        image.SetSource(new MemoryStream(data));

                        wait.Set();
                    });

                    wait.WaitOne();
                }

                icons.Add(id, image);
            }
        }

        private static void ParseProtection(XmlReader reader,
            DatabaseConfiguration config)
        {
            if (reader.Name != "MemoryProtection")
                reader.Read();

            reader.Read();

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                switch (reader.Name)
                {
                    case "ProtectTitle":
                        config.ProtectTitle = reader
                            .ReadElementContentAsString() == "True";
                        break;

                    case "ProtectUserName":
                        config.ProtectUserName = reader
                            .ReadElementContentAsString() == "True";
                        break;

                    case "ProtectPassword":
                        config.ProtectPassword = reader
                            .ReadElementContentAsString() == "True";
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }
        }

        private string ReadFieldValue(
            XmlReader reader, out bool isProtected)
        {
            isProtected = IsProtected(reader);
            reader.MoveToContent();
            var value = reader.ReadElementContentAsString();

            if (isProtected && !string.IsNullOrEmpty(value))
                value = _crypto.Decrypt(value);

            return value;
        }

        private IList<Field> ReadFields(XmlReader reader)
        {
            var fields = new List<Field>();

            if (reader.Name != "String")
                reader.ReadToFollowing("String");

            while (reader.Name == "String")
            {
                reader.Read();

                bool isProtected;
                var name = reader.ReadElementContentAsString();
                var value = ReadFieldValue(reader, out isProtected);

                fields.Add(new Field
                {
                    Name = name,
                    Value = value,
                    Protected = isProtected,
                });

                reader.ReadEndElement();
            }

            return fields;
        }

        private IList<Entry> ReadHistories(XmlReader reader)
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
            if (reader.Name != "Times")
                reader.ReadToFollowing("Times");

            DateTime result;
            using (var subReader = reader.ReadSubtree())
            {
                if (subReader.Name != "LastModificationTime")
                    subReader.ReadToFollowing("LastModificationTime");

                result = subReader.ReadElementContentAsDateTime();
            }

            reader.ReadEndElement();

            return result;
        }
    }
}