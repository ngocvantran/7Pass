using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace KeePass.Generator
{
    internal class GenerationResults : IList
    {
        private const int CACHE_SIZE = 50;

        private readonly char[] _characters;
        private readonly int _count;
        private readonly int _length;
        private readonly Random _rnd;
        private readonly Queue<DataItem> _simpleCache;

        public int Count
        {
            get { return _count; }
        }

        public object this[int index]
        {
            get
            {
                var result = _simpleCache.FirstOrDefault(
                    item => item.Index == index);

                if (result == null)
                {
                    if (_simpleCache.Count >= CACHE_SIZE)
                        _simpleCache.Dequeue();

                    result = new DataItem(
                        index, Generate());
                    _simpleCache.Enqueue(result);
                }

                return result;
            }
            set { throw new NotSupportedException(); }
        }

        #region Not supported stuff

        public bool IsFixedSize
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsSynchronized
        {
            get { throw new NotSupportedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotSupportedException(); }
        }

        public int Add(object value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(object value)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        public void Remove(object value)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        #endregion

        public GenerationResults(char[] characters, int length)
        {
            if (characters == null)
                throw new ArgumentNullException("characters");

            _length = length;
            _rnd = new Random();
            _characters = characters;


            var count = Math.Pow(
                _characters.Length, length);
            _count = (int)Math.Min(count, 10000);

            // Initialize cache
            _simpleCache = new Queue<DataItem>(Enumerable
                .Range(0, CACHE_SIZE)
                .Select(x => new DataItem(
                    x, Generate())));
        }

        public int IndexOf(object value)
        {
            if (value == null)
            {
                Debug.WriteLine("IndexOf(null)");
                return -1;
            }

            var item = (DataItem)value;
            return item.Index;
        }

        private string Generate()
        {
            var chars = new char[_length];
            for (var i = 0; i < _length; i++)
            {
                var index = _rnd.Next(0,
                    _characters.Length - 1);
                chars[i] = _characters[index];
            }

            return new string(chars);
        }

        public class DataItem
        {
            private readonly int _index;
            private readonly string _password;

            public int Index
            {
                get { return _index; }
            }

            public string Password
            {
                get { return _password; }
            }

            public DataItem(int index, string password)
            {
                _index = index;
                _password = password;
            }

            public override string ToString()
            {
                return _password.Length < 20 ? _password
                    : _password.Substring(0, 19) + "…";
            }
        }
    }
}