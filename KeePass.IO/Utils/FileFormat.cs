using System;
using System.IO;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.GZip;

namespace KeePass.IO.Utils
{
    internal static class FileFormat
    {
        public const ulong SIGNATURE = 0xb54bfb679aa2d903;

        public static Headers ReadHeaders(Stream stream)
        {
            HeaderFields field;
            var fields = new Headers();
            var reader = new BinaryReader(stream);

            do
            {
                field = (HeaderFields)reader.ReadByte();
                var size = reader.ReadInt16();
                fields.Add(field, reader.ReadBytes(size));
            } while (field != HeaderFields.EndOfHeader);

            return fields;
        }

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

        public static Stream Decrypt(Stream source,
            Headers headers, byte[] masterKey)
        {
            byte[] easKey;
            using (var buffer = new MemoryStream())
            {
                var masterSeed = headers.MasterSeed;
                buffer.Write(masterSeed, 0, masterSeed.Length);
                buffer.Write(masterKey, 0, masterKey.Length);

                easKey = BufferEx.GetHash(buffer.ToArray());
            }

            var eas = new AesManaged
            {
                KeySize = 256,
                Key = BufferEx.Clone(easKey),
                IV = BufferEx.Clone(headers.EncryptionIV)
            };

            Stream stream = new CryptoStream(source,
                eas.CreateDecryptor(),
                CryptoStreamMode.Read);

            if (!VerifyStartBytes(headers, stream))
                return null;

            stream = new HashedBlockStream(stream, true);
            return headers.Compression == Compressions.GZip
                ? new GZipInputStream(stream) : stream;
        }

        private static bool VerifyStartBytes(
            Headers headers, Stream stream)
        {
            var actual = new BinaryReader(stream)
                .ReadBytes(32);
            var expected = headers.StreamStartBytes;

            return BufferEx.Equals(actual, expected);
        }
    }
}