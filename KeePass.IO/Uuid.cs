using System;

namespace KeePass.IO
{
    public static class Uuid
    {
        public static string NewUuid()
        {
            var bytes = Guid.NewGuid().ToByteArray();
            return Convert.ToBase64String(bytes);
        }
    }
}