using System;

namespace KeePass.Generator
{
    internal class UnderlineSet : ICharacterSet
    {
        public char[] Charaters
        {
            get { return new[] {'_'}; }
        }

        public string Name
        {
            get { return Resources.Underline; }
        }
    }
}