using System;
using System.Linq;

namespace KeePass.Generator
{
    internal class SpecialSet : ICharacterSet
    {
        public char[] Charaters
        {
            get
            {
                return "!\"#$%&'()*+,-./:;<=>?[\\]^_{|}~"
                    .ToArray();
            }
        }

        public string Name
        {
            get { return Resources.Special; }
        }
    }
}