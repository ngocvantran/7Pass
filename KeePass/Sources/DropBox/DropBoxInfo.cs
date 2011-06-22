using System;
using DropNet;

namespace KeePass.Sources.DropBox
{
    internal class DropBoxInfo
    {
#warning Your DropBox info is needed
        public const string KEY = "YOUR_KEY";
        public const string SECRET = "YOUR_SECRET";

        public static DropNetClient Create()
        {
            return new DropNetClient(KEY, SECRET);
        }

        public static DropNetClient Create(
            string token, string secret)
        {
            return new DropNetClient(
                KEY, SECRET, token, secret);
        }
    }
}