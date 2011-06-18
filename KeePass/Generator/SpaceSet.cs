using System;

namespace KeePass.Generator
{
    internal class SpaceSet : ICharacterSet
    {
        public char[] Charaters
        {
            get { return new[] {' '}; }
        }

        public string Name
        {
            get { return Resources.Space; }
        }
    }
}