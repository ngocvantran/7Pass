using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace KeePass.IO
{
    internal static class XmlParser
    {
        public static Group Parse(Stream stream)
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

                if (!reader.ReadToDescendant("Root"))
                    return null;

                if (!reader.ReadToDescendant("Group"))
                    return null;

                using (var subReader = reader.ReadSubtree())
                    return ParseGroup(subReader);
            }
        }

        private static void ParseChildren(XmlReader reader, Group group)
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

        private static Entry ParseEntry(XmlReader reader)
        {
            var id = ReadId(reader);
            var fields = new Dictionary<string, string>();

            reader.ReadToFollowing("String");
            while (reader.Name == "String")
            {
                reader.Read();
                var name = reader.ReadElementContentAsString();
                var value = reader.ReadElementContentAsString();

                fields.Add(name, value);
                reader.ReadEndElement();
            }

            if (fields.Count == 0)
                return null;

            var entry = new Entry(fields)
            {
                ID = id,
            };

            return entry;
        }

        private static Group ParseGroup(XmlReader reader)
        {
            var id = ReadId(reader);

            if (reader.Name != "Name")
                reader.ReadToNextSibling("Name");

            var name = reader
                .ReadElementContentAsString();

            var group = new Group
            {
                ID = id,
                Name = name,
            };

            ParseChildren(reader, group);

            return group;
        }

        private static Guid ReadId(XmlReader reader)
        {
            reader.ReadToDescendant("UUID");
            var id = reader.ReadElementContentAsString();
            return new Guid(Convert.FromBase64String(id));
        }
    }
}