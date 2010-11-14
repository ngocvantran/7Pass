using System;

namespace KeePass.IO
{
    /// <summary>
    /// Compression algorithm specifiers.
    /// </summary>
    public enum PwCompressionAlgorithm : uint
    {
        /// <summary>
        /// No compression.
        /// </summary>
        None = 0,

        /// <summary>
        /// GZip compression.
        /// </summary>
        GZip,

        /// <summary>
        /// Virtual field: currently known number of algorithms. Should not be used
        /// by plugins or libraries -- it's used internally only.
        /// </summary>
        Count
    }
}