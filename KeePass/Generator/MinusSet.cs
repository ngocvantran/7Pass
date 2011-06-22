using System;

namespace KeePass.Generator
{
    internal class MinusSet : ICharacterSet
    {
        public char[] Characters
        {
            get { return new[] {'-'}; }
        }

        public string Name
        {
            get { return Resources.Minus; }
        }

        public int Strength
        {
            get { return 16; }
        }
    }
}