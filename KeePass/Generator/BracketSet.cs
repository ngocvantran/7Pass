using System;

namespace KeePass.Generator
{
    internal class BracketSet : ICharacterSet
    {
        public char[] Charaters
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
    }
}