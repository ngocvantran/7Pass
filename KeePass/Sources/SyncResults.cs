using System;

namespace KeePass.Sources
{
    internal enum SyncResults
    {
        NoChange,
        Uploaded,
        Downloaded,
        Conflict,
        Failed,
    }
}