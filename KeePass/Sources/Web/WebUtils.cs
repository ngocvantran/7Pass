using System;
using System.Net;

namespace KeePass.Sources.Web
{
    internal static class WebUtils
    {
        public static void Download(string url,
            Action<HttpWebRequest, Func<HttpWebResponse>> report)
        {
            var request = WebRequest.CreateHttp(url);
            request.UserAgent = "7Pass";
            request.BeginGetResponse(ar =>
                report(request, () => (HttpWebResponse)
                    request.EndGetResponse(ar)),
                null);
        }
    }
}