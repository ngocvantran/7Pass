using System;
using System.Net;
using System.Text;

namespace KeePass.Sources.WebDav.Api
{
    internal class BasicAuthenticator : IAuthenticator
    {
        private readonly string _password;
        private readonly string _token;
        private readonly string _user;

        public BasicAuthenticator(string user, string password)
        {
            _user = user;
            _password = password;

            _token = string.Format("{0}:{1}",
                user, password ?? string.Empty);

            _token = "Basic " + Convert.ToBase64String(
                Encoding.UTF8.GetBytes(_token));
        }

        public string GetToken(Uri uri, string method)
        {
            return _token;
        }

        public IAuthenticator Next(WebException ex)
        {
            // No user name provided, just throw
            if (string.IsNullOrEmpty(_user))
                return null;

            var response = (HttpWebResponse)ex.Response;
            var header = response.Headers["WWW-Authenticate"];

            if (response.StatusCode != HttpStatusCode.Unauthorized ||
                string.IsNullOrEmpty(header))
                return null;

            var digest = new DigestToken(header,
                _user, _password);

            return new DigestAuthenticator(digest);
        }
    }
}