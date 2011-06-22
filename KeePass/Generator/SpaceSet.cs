using System;

namespace KeePass.Generator
{
    internal class SpaceSet : ICharacterSet
    {
        public char[] Characters
        {
            get { return new[] {' '}; }
        }

        public string Name
        {
            get { return Resources.Space; }
        }

        public int Strength
        {
            get { return 16; }
        }
    }
}