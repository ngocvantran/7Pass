using System;
using System.Security.Cryptography;
using System.Text;

namespace KeePass.IO.Utils
{
    internal class PasswordData
    {
        private readonly byte[] _hash;
        private readonly byte[] _utf8;

        public PasswordData(string password)
        {
            _utf8 = Encoding.UTF8.GetBytes(password);
            _hash = BufferEx.GetHash(_utf8);
            _hash = BufferEx.GetHash(_hash);
        }

        public byte[] TransformKey(byte[] transformSeed, int rounds)
        {
            var block = BufferEx.Clone(_hash);

            var aes = new AesManaged
            {
                KeySize = 256,
                IV = new byte[16],
                Key = transformSeed,
            };

            for (var i = 1; i <= rounds; i++)
            {
                // ECB mode is not available in Silverlight
                // Always use a new encrytor to emulate ECB mode.

                aes.CreateEncryptor().TransformBlock(
                    block, 0, 16, block, 0);
                aes.CreateEncryptor().TransformBlock(
                    block, 16, 16, block, 16);
            }

            return BufferEx.GetHash(block);
        }
    }
}