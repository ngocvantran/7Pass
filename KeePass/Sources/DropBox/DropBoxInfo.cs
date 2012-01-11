using System;
using DropNet;

namespace KeePass.Sources.DropBox
{
    internal static class DropBoxInfo
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

        public static string GetUrl(
            this DropNetClient client,
            string path)
        {
            var login = client.UserLogin;

            return string.Format(
                "dropbox://{0}:{1}@dropbox.com{2}",
                login.Token, login.Secret, path);
        }
    }
}