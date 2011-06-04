using System;

namespace KeePass.Sources
{
    public class SyncInfo
    {
        public byte[] Database { get; set; }
        public bool HasLocalChanges { get; set; }
        public string Modified { get; set; }
        public string Path { get; set; }
    }
}