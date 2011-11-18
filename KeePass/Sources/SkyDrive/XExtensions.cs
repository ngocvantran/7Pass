using System;
using System.Xml.Linq;

namespace KeePass.Sources.SkyDrive
{
    internal static class XExtensions
    {
        public static T GetValue<T>(this XElement node,
            Func<XElement, T> property, params string[] path)
        {
            if (node == null)
                return default(T);

            if (path != null)
            {
                foreach (var step in path)
                {
                    node = node.Element(step);

                    if (node == null)
                        return default(T);
                }
            }

            return property(node);
        }
    }
}