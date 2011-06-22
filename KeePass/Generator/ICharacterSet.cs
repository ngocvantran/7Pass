using System;

namespace KeePass.Generator
{
    internal interface ICharacterSet
    {
        /// <summary>
        /// Gets the charaters.
        /// </summary>
        char[] Characters { get; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the strength value to
        /// estimate password quality.
        /// </summary>
        int Strength { get; }
    }
}