using System;
using System.Xml;

namespace KeePass.IO.Utils
{
    internal static class XmlReaderExtensions
    {
        public static Guid ReadElementContentAsGuid(
            this XmlReader reader)
        {
            var value = reader
                .ReadElementContentAsString();
            return new Guid(Convert.FromBase64String(value));
        }
    }
}