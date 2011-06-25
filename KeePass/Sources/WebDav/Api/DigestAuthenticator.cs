using System;
using System.Net;

namespace KeePass.Sources.WebDav.Api
{
    internal class DigestAuthenticator : IAuthenticator
    {
        private readonly DigestToken _digest;

        public DigestAuthenticator(DigestToken digest)
        {
            if (digest == null)
                throw new ArgumentNullException("digest");
            _digest = digest;
        }

        public string GetToken(Uri uri, string method)
        {
            return _digest.GetDigest(
                uri.AbsolutePath, method);
        }

        public IAuthenticator Next(WebException ex)
        {
            return null;
        }
    }
}