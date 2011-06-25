using System;
using System.Net;

namespace KeePass.Sources.WebDav.Api
{
    internal class NullAuthenticator : IAuthenticator
    {
        public string GetToken(Uri uri, string method)
        {
            return null;
        }

        public IAuthenticator Next(WebException ex)
        {
            return null;
        }
    }
}