using System;

namespace KeePass.IO.Utils
{
    internal sealed class Salsa20Cipher
    {
        private static readonly uint[] _sigma = new uint[]
        {
            0x61707865, 0x3320646E, 0x79622D32, 0x6B206574
        };

        private readonly byte[] _output = new byte[64];

        private readonly uint[] _state = new uint[16];
        private readonly uint[] _x = new uint[16]; // Working buffer

        private int _outputPos = 64;

        public Salsa20Cipher(byte[] pbKey32, byte[] pbIV8)
        {
            KeySetup(pbKey32);
            IvSetup(pbIV8);
        }

        public void Encrypt(byte[] m, int nByteCount, bool bXor)
        {
            if (m == null) throw new ArgumentNullException("m");
            if (nByteCount > m.Length) throw new ArgumentException();

            int nBytesRem = nByteCount, nOffset = 0;
            while (nBytesRem > 0)
            {
                if (_outputPos == 64)
                    NextOutput();

                var nCopy = Math.Min(64 - _outputPos, nBytesRem);

                if (bXor)
                {
                    XorArray(_output, _outputPos,
                        m, nOffset, nCopy);
                }
                else
                {
                    Array.Copy(_output, _outputPos,
                        m, nOffset, nCopy);
                }

                _outputPos += nCopy;
                nBytesRem -= nCopy;
                nOffset += nCopy;
            }
        }

        private void IvSetup(byte[] pbIV)
        {
            if (pbIV == null) throw new ArgumentNullException("pbIV");
            if (pbIV.Length != 8) throw new ArgumentException();

            _state[6] = U8To32Little(pbIV, 0);
            _state[7] = U8To32Little(pbIV, 4);
            _state[8] = 0;
            _state[9] = 0;
        }

        private void KeySetup(byte[] k)
        {
            if (k == null) throw new ArgumentNullException("k");
            if (k.Length != 32) throw new ArgumentException();

            _state[1] = U8To32Little(k, 0);
            _state[2] = U8To32Little(k, 4);
            _state[3] = U8To32Little(k, 8);
            _state[4] = U8To32Little(k, 12);
            _state[11] = U8To32Little(k, 16);
            _state[12] = U8To32Little(k, 20);
            _state[13] = U8To32Little(k, 24);
            _state[14] = U8To32Little(k, 28);
            _state[0] = _sigma[0];
            _state[5] = _sigma[1];
            _state[10] = _sigma[2];
            _state[15] = _sigma[3];
        }

        private void NextOutput()
        {
            var x = _x; // Local alias for working buffer

            // Compiler/runtime might remove array bound checks after this
            if (x.Length < 16)
                throw new InvalidOperationException();

            Array.Copy(_state, x, 16);

            unchecked
            {
                for (var i = 0; i < 10; ++i)
                {
                    x[4] ^= Rotl32(x[0] + x[12], 7);
                    x[8] ^= Rotl32(x[4] + x[0], 9);
                    x[12] ^= Rotl32(x[8] + x[4], 13);
                    x[0] ^= Rotl32(x[12] + x[8], 18);
                    x[9] ^= Rotl32(x[5] + x[1], 7);
                    x[13] ^= Rotl32(x[9] + x[5], 9);
                    x[1] ^= Rotl32(x[13] + x[9], 13);
                    x[5] ^= Rotl32(x[1] + x[13], 18);
                    x[14] ^= Rotl32(x[10] + x[6], 7);
                    x[2] ^= Rotl32(x[14] + x[10], 9);
                    x[6] ^= Rotl32(x[2] + x[14], 13);
                    x[10] ^= Rotl32(x[6] + x[2], 18);
                    x[3] ^= Rotl32(x[15] + x[11], 7);
                    x[7] ^= Rotl32(x[3] + x[15], 9);
                    x[11] ^= Rotl32(x[7] + x[3], 13);
                    x[15] ^= Rotl32(x[11] + x[7], 18);
                    x[1] ^= Rotl32(x[0] + x[3], 7);
                    x[2] ^= Rotl32(x[1] + x[0], 9);
                    x[3] ^= Rotl32(x[2] + x[1], 13);
                    x[0] ^= Rotl32(x[3] + x[2], 18);
                    x[6] ^= Rotl32(x[5] + x[4], 7);
                    x[7] ^= Rotl32(x[6] + x[5], 9);
                    x[4] ^= Rotl32(x[7] + x[6], 13);
                    x[5] ^= Rotl32(x[4] + x[7], 18);
                    x[11] ^= Rotl32(x[10] + x[9], 7);
                    x[8] ^= Rotl32(x[11] + x[10], 9);
                    x[9] ^= Rotl32(x[8] + x[11], 13);
                    x[10] ^= Rotl32(x[9] + x[8], 18);
                    x[12] ^= Rotl32(x[15] + x[14], 7);
                    x[13] ^= Rotl32(x[12] + x[15], 9);
                    x[14] ^= Rotl32(x[13] + x[12], 13);
                    x[15] ^= Rotl32(x[14] + x[13], 18);
                }

                for (var i = 0; i < 16; ++i)
                    x[i] += _state[i];

                for (var i = 0; i < 16; ++i)
                {
                    _output[i << 2] = (byte)x[i];
                    _output[(i << 2) + 1] = (byte)(x[i] >> 8);
                    _output[(i << 2) + 2] = (byte)(x[i] >> 16);
                    _output[(i << 2) + 3] = (byte)(x[i] >> 24);
                }

                _outputPos = 0;
                ++_state[8];

                if (_state[8] == 0)
                    ++_state[9];
            }
        }

        private static uint Rotl32(uint x, int b)
        {
            unchecked
            {
                return ((x << b) | (x >> (32 - b)));
            }
        }

        private static uint U8To32Little(byte[] pb, int iOffset)
        {
            unchecked
            {
                return (pb[iOffset] |
                    ((uint)pb[iOffset + 1] << 8) |
                        ((uint)pb[iOffset + 2] << 16) |
                            ((uint)pb[iOffset + 3] << 24));
            }
        }

        private static void XorArray(byte[] pbSource,
            int nSourceOffset, byte[] pbBuffer,
            int nBufferOffset, int nLength)
        {
            if (pbSource == null)
                throw new ArgumentNullException("pbSource");

            if (nSourceOffset < 0)
                throw new ArgumentException();

            if (pbBuffer == null)
                throw new ArgumentNullException("pbBuffer");

            if (nBufferOffset < 0)
                throw new ArgumentException();

            if (nLength < 0)
                throw new ArgumentException();

            if ((nSourceOffset + nLength) > pbSource.Length)
                throw new ArgumentException();

            if ((nBufferOffset + nLength) > pbBuffer.Length)
                throw new ArgumentException();

            for (var i = 0; i < nLength; ++i)
            {
                pbBuffer[nBufferOffset + i] ^=
                    pbSource[nSourceOffset + i];
            }
        }

        ~Salsa20Cipher()
        {
            // Clear sensitive data
            Array.Clear(_state, 0, _state.Length);
            Array.Clear(_x, 0, _x.Length);
        }
    }
}