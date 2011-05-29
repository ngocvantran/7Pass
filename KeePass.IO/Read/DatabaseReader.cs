using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Threading;
using ICSharpCode.SharpZipLib.GZip;
using KeePass.IO.Data;
using KeePass.IO.Utils;

namespace KeePass.IO.Read
{
    public static class DatabaseReader
    {
        public static bool CheckSignature(Stream stream)
        {
            return FileFormat.Verify(stream);
        }

        public static DbPersistentData GetXml(
            Stream stream, string password, byte[] keyFile)
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

            var decrypted = Decrypt(stream,
                headers, password, keyFile);
            if (decrypted == null)
                return null;

            using (decrypted)
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

        public static Database Load(DbPersistentData data,
            Dispatcher dispatcher)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            using (var buffer = new MemoryStream(data.Xml))
            {
                var crypto = CryptoSerializer
                    .Deserialize(data.Protection);

                return new XmlParser(crypto, buffer, dispatcher)
                    .Parse();
            }
        }

        private static Stream Decrypt(Stream source,
            Headers headers, string password, byte[] keyFile)
        {
            var masterKey = new PasswordData(password, keyFile)
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

            if (!VerifyStartBytes(headers, stream))
                return null;

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