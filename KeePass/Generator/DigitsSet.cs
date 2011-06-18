using System;
using System.Linq;

namespace KeePass.Generator
{
    internal class DigitsSet : ICharacterSet
    {
        public char[] Charaters
        {
            get
            {
                return Enumerable
                    .Range('0', 10)
                    .Select(x => (char)x)
                    .ToArray();
            }
        }

        public string Name
        {
            get { return Resources.Digits; }
        }
    }
}