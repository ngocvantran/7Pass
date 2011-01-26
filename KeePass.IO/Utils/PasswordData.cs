using System;
using System.Security.Cryptography;
using System.Text;

namespace KeePass.IO.Utils
{
    internal class PasswordData
    {
        private readonly byte[] _hash;

        public PasswordData(string password, byte[] keyFile)
        {
            if (!string.IsNullOrEmpty(password))
            {
                var utf8 = Encoding.UTF8.GetBytes(password);
                _hash = BufferEx.GetHash(utf8);
            }

            if (keyFile != null)
            {
                if (_hash != null)
                {
                    var current = _hash.Length;
                    Array.Resize(ref _hash, current + keyFile.Length);
                    Array.Copy(keyFile, 0, _hash,
                        current, keyFile.Length);
                }
                else
                    _hash = keyFile;
            }

            if (_hash == null)
            {
                throw new InvalidOperationException(
                    "At least password or key file must be provided");
            }

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