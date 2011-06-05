using System;
using KeePassLib.Utility;

namespace KeePass.IO
{
    public static class Uuid
    {
        public static string NewUuid()
        {
            var bytes = Guid.NewGuid().ToByteArray();
            return MemUtil.ByteArrayToHexString(bytes);
        }
    }
}