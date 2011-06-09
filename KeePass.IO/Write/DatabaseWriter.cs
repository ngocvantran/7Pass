using System;
using System.IO;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib.GZip;
using KeePass.IO.Data;
using KeePass.IO.Read;
using KeePass.IO.Utils;

namespace KeePass.IO.Write
{
    public class DatabaseWriter
    {
        private Headers _headers;
        private byte[] _masterKey;
        private Version _version;
        private XmlWriter _xmlWriter;

        /// <summary>
        /// Deletes the specified group.
        /// </summary>
        /// <param name="group">The group.</param>
        public void Delete(Group group)
        {
            _xmlWriter.Delete(group);
        }

        /// <summary>
        /// Deletes the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void Delete(Entry entry)
        {
            _xmlWriter.Delete(entry);
        }

        /// <summary>
        /// Updates the details of the specified group.
        /// </summary>
        /// <param name="group">The group.</param>
        public void Details(Group group)
        {
            _xmlWriter.Details(group);
        }

        /// <summary>
        /// Updates the details of the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void Details(Entry entry)
        {
            _xmlWriter.Details(entry);
        }

        public void LoadExisting(Stream existing, byte[] masterKey)
        {
            if (existing == null) throw new ArgumentNullException("existing");
            if (masterKey == null) throw new ArgumentNullException("masterKey");

            var reader = new BinaryReader(existing);
            if (!FileFormat.Sign(reader))
            {
                throw new FormatException(
                    "Invalid format detected");
            }

            _version = FileFormat
                .ReadVersion(reader);

            _masterKey = masterKey;
            _headers = DatabaseReader
                .ReadHeaders(existing);

            _xmlWriter = new XmlWriter();

            using (var decrypt = DatabaseReader.Decrypt(
                existing, _headers, masterKey))
            {
                _xmlWriter.Load(decrypt);

                var crypto = CryptoSerializer
                    .Create(_headers);
                _xmlWriter.Decrypt(crypto);
            }
        }

        /// <summary>
        /// Updates the location of the specified group.
        /// </summary>
        /// <param name="group">The group.</param>
        public void Location(Group group)
        {
            _xmlWriter.Location(group);
        }

        /// <summary>
        /// Updates the location of the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void Location(Entry entry)
        {
            _xmlWriter.Location(entry);
        }

        /// <summary>
        /// Saves the new group.
        /// </summary>
        /// <param name="group">The group.</param>
        public void New(Group group)
        {
            _xmlWriter.New(group);
        }

        /// <summary>
        /// Saves the new entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public void New(Entry entry)
        {
            _xmlWriter.New(entry);
        }

        public void Save(Stream stream, Group recycleBin)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            // Encrypt protected fields
            var crypto = CryptoSerializer
                .Create(_headers);
            _xmlWriter.Encrypt(crypto);

            // Headers
            var bw = new BinaryWriter(stream);
            new HeadersWriter(bw)
                .Write(_headers, _version);

            // Main XML
            using (var encrypted = Encrypt(stream,
                _headers, _masterKey))
            {
                _xmlWriter.Save(encrypted,
                    recycleBin);
            }
        }

        internal static Stream Encrypt(Stream source,
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
                eas.CreateEncryptor(),
                CryptoStreamMode.Write);

            stream.Write(headers.StreamStartBytes, 0,
                headers.StreamStartBytes.Length);

            stream = new HashedBlockStream(stream, false);
            return headers.Compression == Compressions.GZip
                ? new GZipOutputStream(stream) : stream;
        }
    }
}