using System;

namespace KeePass.Storage
{
    internal enum OpenDbResults
    {
        Success,
        CorruptedFile,
        IncorrectPassword,
    }
}