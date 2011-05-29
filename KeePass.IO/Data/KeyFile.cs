using System;
using System.IO;
using System.Text;
using System.Xml;
using KeePassLib.Utility;

namespace KeePass.IO.Data
{
    public static class KeyFile
    {
        public static byte[] GetKey(Stream data)
        {
            switch (data.Length)
            {
                case 32:
                {
                    var reader = new BinaryReader(data);
                    return reader.ReadBytes(32);
                }

                case 64:
                {
                    var reader = new StreamReader(
                        data, Encoding.UTF8);

                    var hex = reader.ReadToEnd();
                    return MemUtil.HexStringToByteArray(hex);
                }

                default:
                    return ReadXml(data);
            }
        }

        private static byte[] ReadXml(Stream data)
        {
            data.Position = 0;

            var settings = new XmlReaderSettings
            {
                CloseInput = false,
                IgnoreComments = true,
                IgnoreWhitespace = true,
                IgnoreProcessingInstructions = true,
            };

            using (var reader = XmlReader.Create(data, settings))
            {
                try
                {
                    if (!reader.ReadToFollowing("KeyFile"))
                        return null;

                    if (!reader.ReadToDescendant("Key"))
                        return null;

                    if (!reader.ReadToDescendant("Data"))
                        return null;

                    var base64 = reader.ReadElementContentAsString();
                    return Convert.FromBase64String(base64);
                }
                catch (XmlException)
                {
                    return null;
                }
            }
        }
    }
}