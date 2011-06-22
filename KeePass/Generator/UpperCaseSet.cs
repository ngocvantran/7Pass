using System;
using System.Linq;

namespace KeePass.Generator
{
    internal class UpperCaseSet : ICharacterSet
    {
        public char[] Characters
        {
            get
            {
                return Enumerable
                    .Range('A', 'Z' - 'A')
                    .Select(x => (char)x)
                    .ToArray();
            }
        }

        public string Name
        {
            get { return Resources.Upper; }
        }

        public int Strength
        {
            get { return 26; }
        }
    }
}