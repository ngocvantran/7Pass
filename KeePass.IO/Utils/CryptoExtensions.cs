using System;
using System.Text;

namespace KeePass.IO.Utils
{
    internal static class CryptoExtensions
    {
        /// <summary>
        /// Descrypts the specified value.
        /// </summary>
        /// <param name="crypto">The crypto stream.</param>
        /// <param name="value">The value.</param>
        /// <returns>Decrypted value.</returns>
        public static string Decrypt(
            this CryptoRandomStream crypto, string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var encrypted = Convert
                .FromBase64String(value);
            var pad = crypto.GetRandomBytes(
                (uint)encrypted.Length);

            for (var i = 0; i < encrypted.Length; i++)
                encrypted[i] ^= pad[i];

            return Encoding.UTF8.GetString(
                encrypted, 0, encrypted.Length);
        }
    }
}