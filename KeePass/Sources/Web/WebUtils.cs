using System;
using System.Net;
using Newtonsoft.Json;

namespace KeePass.Sources.Web
{
    internal static class WebUtils
    {
        public static NetworkCredential CreateCredentials(
            string user, string password, string domain)
        {
            if (string.IsNullOrEmpty(user) &&
                string.IsNullOrEmpty(password))
                return null;

            var credentials = new NetworkCredential(user, password);

            if (!string.IsNullOrEmpty(domain))
                credentials.Domain = domain;

            return credentials;
        }

        public static WebUrlInfo Deserialize(string info)
        {
            return JsonConvert.DeserializeObject<WebUrlInfo>(info);
        }

        public static void Download(string url, ICredentials credentials,
            Action<HttpWebRequest, Func<HttpWebResponse>> report)
        {
            var request = WebRequest.CreateHttp(url);
            request.UserAgent = "7Pass";

            if (credentials != null)
                request.Credentials = credentials;

            request.BeginGetResponse(ar =>
                report(request, () => (HttpWebResponse)
                    request.EndGetResponse(ar)),
                null);
        }

        public static bool IsValidUrl(
            string url, bool allowRelative)
        {
            var kind = allowRelative
                ? UriKind.RelativeOrAbsolute
                : UriKind.Absolute;

            if (!Uri.IsWellFormedUriString(url, kind))
                return false;

            try
            {
                var uri = new Uri(url);
                switch (uri.Scheme.ToUpper())
                {
                    case "HTTP":
                    case "HTTPS":
                        return true;
                }

                return false;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }

        public static string Serialize(WebRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var info = new WebUrlInfo
            {
                Url = request.RequestUri.ToString()
            };

            var credentials = request.Credentials
                as NetworkCredential;

            if (credentials != null)
            {
                info.User = credentials.UserName;
                info.Domain = credentials.Domain;
                info.Password = credentials.Password;
            }

            return JsonConvert.SerializeObject(info);
        }
    }
}