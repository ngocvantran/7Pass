using System;
using System.Linq;

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

        /// <summary>
        /// Generates the password for a new entry.
        /// </summary>
        /// <returns></returns>
        public static string NewEntry()
        {
            var sets = new ICharacterSet[]
            {
                new UpperCaseSet(),
                new LowerCaseSet(),
                new DigitsSet(),
            }
                .SelectMany(x => x.Characters)
                .ToArray();

            return new GenerationResults(sets, 20)
                .Generate();
        }
    }
}