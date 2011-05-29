using System;

namespace KeePass.IO.Utils
{
    internal class CryptoFactory
    {
        private readonly CrsAlgorithm _algorithm;
        private readonly byte[] _key;

        public CryptoFactory(byte[] key,
            CrsAlgorithm algorithm)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            _key = key;
            _algorithm = algorithm;
        }

        public CryptoRandomStream Create()
        {
            return new CryptoRandomStream(
                _algorithm, _key);
        }
    }
}