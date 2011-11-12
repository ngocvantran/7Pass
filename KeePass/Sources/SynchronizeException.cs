using System;

namespace KeePass.Sources
{
    internal class SynchronizeException : Exception
    {
        public SynchronizeException() {}

        public SynchronizeException(string message,
            Exception innerException)
            : base(message, innerException) {}
    }
}