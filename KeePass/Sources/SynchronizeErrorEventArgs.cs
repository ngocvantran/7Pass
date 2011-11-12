using System;

namespace KeePass.Sources
{
    internal class SynchronizeErrorEventArgs : EventArgs
    {
        private readonly SynchronizeException _ex;

        public SynchronizeException Exception
        {
            get { return _ex; }
        }

        public SynchronizeErrorEventArgs(SynchronizeException ex)
        {
            if (ex == null)
                throw new ArgumentNullException("ex");

            _ex = ex;
        }
    }
}