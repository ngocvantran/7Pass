using System;

namespace KeePass.Generator
{
    internal static class CharacterSets
    {
        public static ICharacterSet[] GetAll()
        {
            return new ICharacterSet[]
            {
                new UpperCaseSet(),
                new LowerCaseSet(),
                new DigitsSet(),
                new MinusSet(),
                new UnderlineSet(),
                new SpaceSet(),
                new SpecialSet(),
                new BracketSet(),
                new HighAnsiSet(),
            };
        }
    }
}