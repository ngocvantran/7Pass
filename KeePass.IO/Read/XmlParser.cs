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
using KeePassLib.Utility;

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

                var binaries = new Dictionary<int, byte[]>();
                var icons = new Dictionary<string, ImageSource>();

                using (var subReader = reader.ReadSubtree())
                {
                    subReader.ReadToFollowing("Generator");

                    while (!string.IsNullOrEmpty(subReader.Name))
                    {
                        subReader.Skip();

                        switch (subReader.Name)
                        {
                            case "RecycleBinUUID":
                                recycleBinId = subReader
                                    .ReadElementContentAsString();
                                break;

                            case "RecycleBinEnabled":
                                var value = subReader
                                    .ReadElementContentAsString();
                                recyleBinEnabled = value == "True";
                                break;

                            case "CustomIcons":
                                using (var iconReader = subReader.ReadSubtree())
                                    ParseIcons(iconReader, _dispatcher, icons);
                                break;

                            case "Binaries":
                                using (var binReader = subReader.ReadSubtree())
                                    binaries = ReadBinaries(binReader);
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
                    var root = ParseGroup(subReader, binaries);
                    var database = new Database(root, icons)
                    {
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

        private void ParseChildren(XmlReader reader, Group group,
            IDictionary<int, byte[]> binaries)
        {
            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "Group":
                        using (var subReader = reader.ReadSubtree())
                            group.Add(ParseGroup(subReader, binaries));

                        reader.ReadEndElement();
                        break;

                    case "Entry":
                        using (var subReader = reader.ReadSubtree())
                        {
                            var entry = ParseEntry(
                                subReader, binaries);

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

        private Entry ParseEntry(XmlReader reader,
            IDictionary<int, byte[]> binaries)
        {
            var id = ReadId(reader);
            var icon = ParseIcon(reader);
            var lastModified = ReadLastModified(reader);
            var fields = ReadFields(reader);
            var attachments = ReadAttachments(reader, binaries);

            // Needed to ensure protected
            // fields decryption
            ReadHistories(reader, binaries);

            if (fields.Count == 0)
                return null;

            var entry = new Entry(fields)
            {
                ID = id,
                Icon = icon,
                Attachments = attachments,
                LastModified = lastModified,
            };

            return entry;
        }

        private Group ParseGroup(XmlReader reader,
            IDictionary<int, byte[]> binaries)
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

            ParseChildren(reader, group, binaries);

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

        private Attachment[] ReadAttachments(XmlReader reader,
            IDictionary<int, byte[]> binaries)
        {
            var result = new List<Attachment>();

            while (reader.Name == "Binary")
            {
                reader.Read();
                var name = reader.ReadElementContentAsString();

                reader.MoveToAttribute("Ref");
                var id = XmlConvert.ToInt32(reader.Value);

                byte[] value;
                if (binaries.TryGetValue(id, out value))
                {
                    result.Add(new Attachment
                    {
                        ID = id,
                        Name = name,
                        Value = value,
                    });
                }

                reader.Read();
                reader.ReadEndElement();
            }

            return result.ToArray();
        }

        private static Dictionary<int, byte[]>
            ReadBinaries(XmlReader reader)
        {
            var binaries = new Dictionary<int, byte[]>();

            while (reader.ReadToFollowing("Binary"))
            {
                reader.MoveToAttribute("ID");
                var id = XmlConvert.ToInt32(reader.Value);

                reader.MoveToAttribute("Compressed");
                var compressed = reader.Value == "True";

                reader.MoveToContent();
                var binary = Convert.FromBase64String(
                    reader.ReadElementContentAsString());

                if (compressed)
                    binary = MemUtil.Decompress(binary);

                binaries.Add(id, binary);
            }

            return binaries;
        }

        private Dictionary<string, string>
            ReadFields(XmlReader reader)
        {
            var fields = new Dictionary<string, string>();

            if (reader.Name != "String")
                reader.ReadToFollowing("String");

            while (reader.Name == "String")
            {
                reader.Read();

                var name = reader.ReadElementContentAsString();
                fields.Add(name, ReadValue(reader));

                reader.ReadEndElement();
            }

            return fields;
        }

        private List<Entry> ReadHistories(XmlReader reader,
            IDictionary<int, byte[]> binaries)
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
                            var history = ParseEntry(
                                subReader, binaries);

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

        private string ReadValue(XmlReader reader)
        {
            var isProtected = IsProtected(reader);
            reader.MoveToContent();
            var value = reader.ReadElementContentAsString();

            if (isProtected && !string.IsNullOrEmpty(value))
                value = _crypto.Decrypt(value);

            return value;
        }
    }
}