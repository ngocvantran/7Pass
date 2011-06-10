using System;
using System.IO;
using System.Windows.Threading;
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
            Stream stream, byte[] masterKey)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!CheckSignature(stream))
            {
                throw new FormatException(
                    "Invalid format detected");
            }

            var headers = FileFormat
                .ReadHeaders(stream);
            headers.Verify();

            return ReadDatabase(stream,
                headers, masterKey);
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

            var headers = FileFormat
                .ReadHeaders(stream);
            headers.Verify();

            var masterKey = GetMasterKey(
                headers, password, keyFile);

            return ReadDatabase(stream,
                headers, masterKey);
        }

        /// <summary>
        /// Determines whether the specified database
        /// uses a large number of transformations.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns><c>true</c> if the specified database uses
        /// a large number of transformations; otherwise, <c>false</c>.
        /// </returns>
        public static bool LargeTransformRounds(Stream stream)
        {
            var headers = FileFormat
                .ReadHeaders(stream);

            return headers.TransformRounds > 6000;
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

        private static byte[] GetMasterKey(Headers headers,
            string password, byte[] keyFile)
        {
            return new PasswordData(password, keyFile)
                .TransformKey(headers.TransformSeed,
                    headers.TransformRounds);
        }

        private static DbPersistentData ReadDatabase(
            Stream stream, Headers headers, byte[] masterKey)
        {
            var decrypted = FileFormat.Decrypt(
                stream, headers, masterKey);

            if (decrypted == null)
                return null;

            using (decrypted)
            using (var buffer = new MemoryStream())
            {
                BufferEx.CopyStream(decrypted, buffer);

                return new DbPersistentData
                {
                    MasterKey = masterKey,
                    Xml = buffer.ToArray(),
                    Protection = CryptoSerializer
                        .Serialize(headers),
                };
            }
        }
    }
}