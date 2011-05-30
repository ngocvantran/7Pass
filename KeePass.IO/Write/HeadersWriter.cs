using System;
using System.IO;
using KeePass.IO.Utils;

namespace KeePass.IO.Write
{
    internal class HeadersWriter
    {
        private readonly BinaryWriter _writer;

        public HeadersWriter(BinaryWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            _writer = writer;
        }

        public void Write(Headers headers, Version version)
        {
            if (headers == null)
                throw new ArgumentNullException("headers");

            // Signature
            _writer.Write(FileFormat.SIGNATURE);

            // Database version
            _writer.Write((short)version.Minor);
            _writer.Write((short)version.Major);

            // Headers
            Write(headers, HeaderFields.CipherID);
            Write(headers, HeaderFields.CompressionFlags);
            Write(headers, HeaderFields.MasterSeed);
            Write(headers, HeaderFields.TransformSeed);
            Write(headers, HeaderFields.TransformRounds);
            Write(headers, HeaderFields.EncryptionIV);
            Write(headers, HeaderFields.ProtectedStreamKey);
            Write(headers, HeaderFields.StreamStartBytes);
            Write(headers, HeaderFields.InnerRandomStreamID);
            Write(headers, HeaderFields.EndOfHeader);

            _writer.Flush();
        }

        private void Write(Headers header, HeaderFields field)
        {
            var bytes = header[field];

            _writer.Write((byte)field);
            _writer.Write((short)bytes.Length);
            _writer.Write(bytes, 0, bytes.Length);
        }
    }
}