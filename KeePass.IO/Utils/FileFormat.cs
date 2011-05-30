using System;
using System.IO;

namespace KeePass.IO.Utils
{
    internal static class FileFormat
    {
        public const ulong SIGNATURE = 0xb54bfb679aa2d903;

        public static Version ReadVersion(
            BinaryReader reader)
        {
            var minor = reader.ReadInt16();
            var major = reader.ReadInt16();

            return new Version(major, minor);
        }

        public static bool Sign(BinaryReader reader)
        {
            var signature = reader.ReadUInt64();
            return signature == SIGNATURE;
        }

        public static bool Verify(Stream stream)
        {
            var reader = new BinaryReader(stream);

            return Sign(reader) &&
                Version(reader);
        }

        private static bool Version(BinaryReader reader)
        {
            var version = ReadVersion(reader);

            return version.Major == 2 ||
                version.Major == 3;
        }
    }
}