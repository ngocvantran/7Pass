using System;
using System.Collections.Generic;

namespace KeePass.IO
{
    internal class Headers : Dictionary<HeaderFields, byte[]>
    {
        private static readonly byte[] _easEngineId = new byte[]
        {
            0x31, 0xC1, 0xF2, 0xE6, 0xBF, 0x71, 0x43, 0x50,
            0xBE, 0x58, 0x05, 0x21, 0x6A, 0xFC, 0x5A, 0xFF
        };

        public Compressions Compression
        {
            get
            {
                return (Compressions)BitConverter.ToChar(
                    this[HeaderFields.CompressionFlags], 0);
            }
        }

        public byte[] EncryptionIV
        {
            get { return this[HeaderFields.EncryptionIV]; }
        }

        public byte[] MasterSeed
        {
            get { return this[HeaderFields.MasterSeed]; }
        }

        public byte[] StreamStartBytes
        {
            get { return this[HeaderFields.StreamStartBytes]; }
        }

        public int TransformRounds
        {
            get
            {
                return BitConverter.ToInt32(
                    this[HeaderFields.TransformRounds], 0);
            }
        }

        public byte[] TransformSeed
        {
            get { return this[HeaderFields.TransformSeed]; }
        }

        public void Verify()
        {
            Verify(HeaderFields.MasterSeed, 32,
                "The length of the master key seed is invalid!");

            Verify(HeaderFields.TransformSeed, 32,
                "The length of the transform seed is invalid!");

            Verify(HeaderFields.EncryptionIV, 16,
                "The length of the encryption IV is invalid!");

            Verify(HeaderFields.StreamStartBytes, 32,
                "The length of the stream start bytes is invalid!");

            var data = Verify(HeaderFields.CipherID, 16,
                "The length of the cipher engine ID is invalid!");

            if (!BufferEx.Equals(_easEngineId, data))
            {
                throw new FormatException(
                    "Only AES encryption is supported!");
            }

            data = Verify(HeaderFields.CompressionFlags, 4,
                "The length of compression format is invalid!");

            var compression = (Compressions)
                BitConverter.ToChar(data, 0);

            if (compression > Compressions.GZip)
            {
                throw new FormatException(
                    "Only no compression and GZip compression are supported!");
            }
        }

        private byte[] Verify(HeaderFields field,
            int length, string error)
        {
            byte[] data;

            if (!TryGetValue(field, out data) ||
                data.Length != length)
            {
                throw new FormatException(error);
            }

            return data;
        }
    }
}