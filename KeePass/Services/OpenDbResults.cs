using System;

namespace KeePass.Services
{
    internal enum OpenDbResults
    {
        Success,
        NoDatabase,
        CorruptedFile,
        IncorrectPassword,
    }
}