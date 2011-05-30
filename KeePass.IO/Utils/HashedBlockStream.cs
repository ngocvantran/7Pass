using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace KeePass.IO.Utils
{
    internal class HashedBlockStream : Stream
    {
        private readonly BinaryReader _reader;
        private readonly bool _reading;
        private readonly BinaryWriter _writer;

        private Stream _baseStream;
        private byte[] _buffer;
        private uint _bufferIndex;
        private bool _eof;
        private int _position;

        public override bool CanRead
        {
            get { return _reading; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return !_reading; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public HashedBlockStream(Stream stream,
            bool reading)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (reading)
            {
                if (!stream.CanRead)
                    throw new InvalidOperationException();
            }
            else if (!stream.CanWrite)
                throw new InvalidOperationException();

            _reading = reading;
            _baseStream = stream;
            _buffer = new byte[reading
                ? 0 : 1024 * 1024];

            var utf8 = new UTF8Encoding(false, false);

            if (_reading)
                _reader = new BinaryReader(stream, utf8);
            else
                _writer = new BinaryWriter(stream, utf8);
        }

        public override void Close()
        {
            if (_baseStream == null)
                return;

            if (_reading)
                _reader.Close();
            else
            {
                if (_position == 0) // No data left in buffer
                    WriteHashedBlock(); // Write terminating block
                else
                {
                    WriteHashedBlock(); // Write remaining buffered data
                    WriteHashedBlock(); // Write terminating block
                }

                _writer.Flush();
                _writer.Close();
            }

            _baseStream.Close();
            _baseStream = null;
        }

        public override void Flush()
        {
            if (!_reading)
                _writer.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!_reading)
                throw new InvalidOperationException();

            var remaining = count;

            while (remaining > 0)
            {
                if (_position == _buffer.Length)
                {
                    if (ReadHashedBlock() == false)
                        return count - remaining;
                }

                var nCopy = Math.Min(remaining,
                    _buffer.Length - _position);

                Buffer.BlockCopy(_buffer, _position,
                    buffer, offset, nCopy);

                offset += nCopy;
                _position += nCopy;

                remaining -= nCopy;
            }

            return count;
        }

        public override long Seek(long lOffset, SeekOrigin soOrigin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long lValue)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_reading)
                throw new InvalidOperationException();

            while (count > 0)
            {
                if (_position == _buffer.Length)
                    WriteHashedBlock();

                var copy = Math.Min(count,
                    _buffer.Length - _position);

                Array.Copy(buffer, offset,
                    _buffer, _position, copy);

                offset += copy;
                _position += copy;

                count -= copy;
            }
        }

        private bool ReadHashedBlock()
        {
            if (_eof)
                return false;

            _position = 0;

            if (_reader.ReadUInt32() != _bufferIndex)
                throw new InvalidDataException();

            _bufferIndex++;

            var actualHash = _reader.ReadBytes(32);

            if ((actualHash == null) || (actualHash.Length != 32))
                throw new InvalidDataException();

            var bufferSize = _reader.ReadInt32();
            if (bufferSize < 0)
                throw new InvalidDataException();

            if (bufferSize == 0)
            {
                if (actualHash.Any(x => x != 0))
                    throw new InvalidDataException();

                _eof = true;
                _buffer = new byte[0];

                return false;
            }

            _buffer = _reader.ReadBytes(bufferSize);

            if (_buffer == null || _buffer.Length != bufferSize)
                throw new InvalidDataException();

            var expectedHash = BufferEx.GetHash(_buffer);
            if (!BufferEx.Equals(actualHash, expectedHash))
                throw new InvalidDataException();

            return true;
        }

        private void WriteHashedBlock()
        {
            _writer.Write(_bufferIndex);
            _bufferIndex++;

            if (_position > 0)
            {
                var sha256 = new SHA256Managed();

                var hash = sha256.ComputeHash(
                    _buffer, 0, _position);

                _writer.Write(hash);
            }
            else
            {
                _writer.Write((ulong)0); // Zero hash
                _writer.Write((ulong)0);
                _writer.Write((ulong)0);
                _writer.Write((ulong)0);
            }

            _writer.Write(_position);

            if (_position > 0)
                _writer.Write(_buffer, 0, _position);

            _position = 0;
        }
    }
}