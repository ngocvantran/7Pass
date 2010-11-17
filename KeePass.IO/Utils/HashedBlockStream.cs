using System;
using System.IO;
using System.Linq;
using System.Text;

namespace KeePass.IO.Utils
{
    internal class HashedBlockStream : Stream
    {
        private Stream _baseStream;
        private byte[] _buffer;
        private uint _bufferIndex;
        private bool _eof;
        private int _position;
        private BinaryReader _reader;

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
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

        public HashedBlockStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (stream.CanRead == false)
                throw new InvalidOperationException();

            _buffer = new byte[0];
            _baseStream = stream;

            _reader = new BinaryReader(stream,
                new UTF8Encoding(false, false));
        }

        public override void Close()
        {
            if (_baseStream == null)
                return;

            _reader.Close();
            _reader = null;

            _baseStream.Close();
            _baseStream = null;
        }

        public override void Flush() {}

        public override int Read(byte[] buffer, int offset, int count)
        {
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
            throw new NotSupportedException();
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
    }
}