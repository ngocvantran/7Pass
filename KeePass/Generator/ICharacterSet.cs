using System;

namespace KeePass.Generator
{
    internal interface ICharacterSet
    {
        /// <summary>
        /// Gets the charaters.
        /// </summary>
        char[] Charaters { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }
    }
}