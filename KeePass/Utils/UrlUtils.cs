using System;

namespace KeePass.Utils
{
    public static class UrlUtils
    {
        public static string GetUntilPath(this Uri uri)
        {
            var full = uri.ToString();
            var index = full.IndexOf('?');

            return index != -1
                ? full.Substring(0, index)
                : full;
        }

        public static bool IsValidUrl(
            Uri baseUrl, string url)
        {
            if (!Uri.IsWellFormedUriString(url,
                UriKind.RelativeOrAbsolute))
            {
                return false;
            }

            try
            {
                var uri = new Uri(baseUrl, url);
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

        public static bool IsValidUrl(string url)
        {
            if (!Uri.IsWellFormedUriString(
                url, UriKind.Absolute))
            {
                return false;
            }

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
    }
}