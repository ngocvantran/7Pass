using System;

namespace KeePass.Data
{
    public class FieldChangedEventArgs : EventArgs
    {
        private readonly string _key;

        public string Key
        {
            get { return _key; }
        }

        public FieldChangedEventArgs(string key)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            _key = key;
        }
    }
}