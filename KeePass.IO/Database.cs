using System;
using System.IO;
using System.Security.Cryptography;

namespace KeePass.IO
{
    public class Database {}

    public class DatabaseReader
    {
        public void Load(Stream stream, string password)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            CheckSignature(stream);

            var headers = ReadHeaders(stream);
            headers.Verify();

            var reader = new BinaryReader(
                CreateStream(stream, headers, password));

            VerifyStartBytes(headers, reader);
            var compression = headers.Compression;
        }

        private static void CheckSignature(Stream stream)
        {
            if (!FileFormat.Verify(stream))
            {
                throw new FormatException(
                    "Invalid format detected");
            }
        }

        private static Stream CreateStream(Stream source,
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

            var transform = eas.CreateDecryptor();
            return new CryptoStream(source,
                transform, CryptoStreamMode.Read);
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
            Headers headers, BinaryReader reader)
        {
            var actual = reader.ReadBytes(32);
            var expected = headers.StreamStartBytes;

            if (!BufferEx.Equals(actual, expected))
                throw new InvalidDataException();
        }
    }
}