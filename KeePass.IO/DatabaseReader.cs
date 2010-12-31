using System;
using System.IO;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.GZip;
using KeePass.IO.Utils;

namespace KeePass.IO
{
    public static class DatabaseReader
    {
        public static bool CheckSignature(Stream stream)
        {
            return FileFormat.Verify(stream);
        }

        public static DbPersistentData GetXml(
            Stream stream, string password)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!CheckSignature(stream))
            {
                throw new FormatException(
                    "Invalid format detected");
            }

            var headers = ReadHeaders(stream);
            headers.Verify();

            using (var decrypted = Decrypt(stream, headers, password))
            using (var buffer = new MemoryStream())
            {
                BufferEx.CopyStream(decrypted, buffer);

                return new DbPersistentData
                {
                    Xml = buffer.ToArray(),
                    Protection = CryptoSerializer
                        .Serialize(headers),
                };
            }
        }

        public static Database Load(DbPersistentData data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            using (var buffer = new MemoryStream(data.Xml))
            {
                var crypto = CryptoSerializer
                    .Deserialize(data.Protection);

                var parser = new XmlParser(crypto);
                return parser.Parse(buffer);
            }
        }

        private static Stream Decrypt(Stream source,
            Headers headers, string password)
        {
            var masterKey = new PasswordData(password)
                .TransformKey(headers.TransformSeed,
                    headers.TransformRounds);

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

            VerifyStartBytes(headers, stream);

            stream = new HashedBlockStream(stream);
            return headers.Compression == Compressions.GZip
                ? new GZipInputStream(stream) : stream;
        }

        private static Headers ReadHeaders(Stream stream)
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

        private static void VerifyStartBytes(
            Headers headers, Stream stream)
        {
            var actual = new BinaryReader(stream)
                .ReadBytes(32);
            var expected = headers.StreamStartBytes;

            if (!BufferEx.Equals(actual, expected))
                throw new InvalidDataException();
        }
    }
}