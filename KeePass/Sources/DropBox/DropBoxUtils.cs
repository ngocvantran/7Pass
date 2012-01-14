using System;
using DropNet;
using KeePass.Utils;

namespace KeePass.Sources.DropBox
{
    internal static class DropBoxUtils
    {
        public static DropNetClient Create()
        {
            return new DropNetClient(
                ApiKeys.DROPBOX_KEY,
                ApiKeys.DROPBOX_SECRET);
        }

        public static DropNetClient Create(
            string token, string secret)
        {
            return new DropNetClient(
                ApiKeys.DROPBOX_KEY,
                ApiKeys.DROPBOX_SECRET,
                token, secret);
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