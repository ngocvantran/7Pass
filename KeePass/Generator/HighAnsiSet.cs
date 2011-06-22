using System;
using System.Linq;

namespace KeePass.Generator
{
    internal class HighAnsiSet : ICharacterSet
    {
        public char[] Characters
        {
            get
            {
                return Enumerable
                    .Range('~', 255 - '~')
                    .Select(x => (char)x)
                    .ToArray();
            }
        }

        public string Name
        {
            get { return Resources.HighAnsi; }
        }
    }
}