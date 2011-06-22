using System;

namespace KeePass.Generator
{
    internal class BracketSet : ICharacterSet
    {
        public char[] Characters
        {
            get
            {
                return new[]
                {
                    '[', ']', '{', '}',
                    '(', ')', '<', '>'
                };
            }
        }

        public string Name
        {
            get { return Resources.Brackets; }
        }

        public int Strength
        {
            get { return 17; }
        }
    }
}