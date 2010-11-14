using System;
using System.Linq;
using System.Security.Cryptography;

namespace KeePass.IO
{
    internal static class BufferEx
    {
        public static byte[] Clone(byte[] input)
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

        public static bool Equals(byte[] x, byte[] y)
        {
            if (x.Length != y.Length)
                return false;

            return !x.Where((t, i) => t != y[i]).Any();
        }

        public static byte[] GetHash(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            var hash = new SHA256Managed();
            return hash.ComputeHash(bytes);
        }
    }
}