using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace KeePass.IO.Utils
{
    public static class BufferEx
    {
        public static void CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[32768];
            while (true)
            {
                var read = input.Read(buffer,
                    0, buffer.Length);

                if (read <= 0)
                    return;

                output.Write(buffer, 0, read);
            }
        }

        internal static byte[] Clone(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            var length = input.Length;
            var output = new byte[length];

            if (length == 0)
                return output;

            Buffer.BlockCopy(input, 0, output, 0, length);

            return output;
        }

        internal static bool Equals(byte[] x, byte[] y)
        {
            if (x.Length != y.Length)
                return false;

            return !x.Where((t, i) => t != y[i]).Any();
        }

        internal static byte[] GetHash(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            var hash = new SHA256Managed();
            return hash.ComputeHash(bytes);
        }
    }
}