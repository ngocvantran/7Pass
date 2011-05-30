using System;
using System.IO;

namespace KeePass.IO.Utils
{
    internal static class CryptoSerializer
    {
        public static CryptoRandomStream Create(Headers headers)
        {
            return new CryptoRandomStream(
                headers.CrsAlgorithm,
                headers.ProtectedStreamKey);
        }

        public static CryptoRandomStream Deserialize(byte[] serialized)
        {
            using (var buffer = new MemoryStream(serialized))
            {
                var reader = new BinaryReader(buffer);
                var algorithm = (CrsAlgorithm)reader.ReadByte();
                var key = reader.ReadBytes(serialized.Length - 1);

                return new CryptoRandomStream(algorithm, key);
            }
        }

        public static byte[] Serialize(Headers headers)
        {
            using (var buffer = new MemoryStream())
            {
                var writer = new BinaryWriter(buffer);
                writer.Write((byte)headers.CrsAlgorithm);
                writer.Write(headers.ProtectedStreamKey);

                return buffer.ToArray();
            }
        }
    }
}