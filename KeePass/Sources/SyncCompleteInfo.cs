using System;

namespace KeePass.Sources
{
    internal class SyncCompleteInfo
    {
        public byte[] Database { get; set; }
        public string Modified { get; set; }
        public string Path { get; set; }
        public SyncResults Result { get; set; }
    }
}