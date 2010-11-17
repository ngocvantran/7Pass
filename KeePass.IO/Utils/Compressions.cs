using System;

namespace KeePass.IO.Utils
{
    public enum Compressions : uint
    {
        /// <summary>
        /// No compression.
        /// </summary>
        None = 0,

        /// <summary>
        /// GZip compression.
        /// </summary>
        GZip,
    }
}