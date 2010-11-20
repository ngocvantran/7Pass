/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2010 Dominik Reichl <dominik.reichl@t-online.de>

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Security.Cryptography;

namespace KeePass.IO.Utils
{
    /// <summary>
    /// A random stream class. The class is initialized using random
    /// bytes provided by the caller. The produced stream has random
    /// properties, but for the same seed always the same stream
    /// is produced, i.e. this class can be used as stream cipher.
    /// </summary>
    internal sealed class CryptoRandomStream
    {
        private readonly CrsAlgorithm _algorithm;
        private readonly byte[] _pbState;
        private readonly Salsa20Cipher _salsa20;

        private byte _i;
        private byte _j;

        /// <summary>
        /// Construct a new cryptographically secure random stream object.
        /// </summary>
        /// <param name="genAlgorithm">Algorithm to use.</param>
        /// <param name="pbKey">Initialization key. Must not be <c>null</c> and
        /// must contain at least 1 byte.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the
        /// <paramref name="pbKey" /> parameter is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException">Thrown if the
        /// <paramref name="pbKey" /> parameter contains no bytes or the
        /// algorithm is unknown.</exception>
        public CryptoRandomStream(CrsAlgorithm genAlgorithm, byte[] pbKey)
        {
            if (pbKey == null)
                throw new ArgumentNullException("pbKey");

            _algorithm = genAlgorithm;

            var uKeyLen = (uint)pbKey.Length;
            if (uKeyLen == 0)
                throw new ArgumentException();

            switch (genAlgorithm)
            {
                case CrsAlgorithm.ArcFourVariant:
                    _pbState = new byte[256];
                    for (uint w = 0; w < 256; ++w)
                        _pbState[w] = (byte)w;

                    unchecked
                    {
                        byte j = 0, t;
                        uint inxKey = 0;

                        for (uint w = 0; w < 256; ++w) // Key setup
                        {
                            j += (byte)(_pbState[w] + pbKey[inxKey]);

                            t = _pbState[0]; // Swap entries
                            _pbState[0] = _pbState[j];
                            _pbState[j] = t;

                            ++inxKey;
                            if (inxKey >= uKeyLen) inxKey = 0;
                        }
                    }

                    GetRandomBytes(512); // Increases security, see cryptanalysis
                    break;

                case CrsAlgorithm.Salsa20:
                {
                    var sha256 = new SHA256Managed();
                    var pbKey32 = sha256.ComputeHash(pbKey);
                    var pbIV = new byte[]
                    {
                        0xE8, 0x30, 0x09, 0x4B,
                        0x97, 0x20, 0x5D, 0x2A
                    }; // Unique constant

                    _salsa20 = new Salsa20Cipher(pbKey32, pbIV);
                }
                    break;

                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Get <paramref name="uRequestedCount" /> random bytes.
        /// </summary>
        /// <param name="uRequestedCount">Number of random bytes to retrieve.</param>
        /// <returns>Returns <paramref name="uRequestedCount" /> random bytes.</returns>
        public byte[] GetRandomBytes(uint uRequestedCount)
        {
            if (uRequestedCount == 0)
                return new byte[0];

            var pbRet = new byte[uRequestedCount];

            switch (_algorithm)
            {
                case CrsAlgorithm.ArcFourVariant:
                    unchecked
                    {
                        for (uint w = 0; w < uRequestedCount; ++w)
                        {
                            ++_i;
                            _j += _pbState[_i];

                            byte t = _pbState[_i]; // Swap entries
                            _pbState[_i] = _pbState[_j];
                            _pbState[_j] = t;

                            t = (byte)(_pbState[_i] + _pbState[_j]);
                            pbRet[w] = _pbState[t];
                        }
                    }
                    break;

                case CrsAlgorithm.Salsa20:
                    _salsa20.Encrypt(pbRet, pbRet.Length, false);
                    break;

                default:
                    throw new ArgumentException();
            }

            return pbRet;
        }
    }
}