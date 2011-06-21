using System;
using System.Net;

namespace KeePass.Sources.Web.Dav
{
    internal class WebDavRequest
    {
        private IAuthenticator _auth;

        public WebDavRequest(string user, string password)
        {
            if (!string.IsNullOrEmpty(user))
                _auth = new BasicAuthenticator(user, password);
            else
                _auth = new NullAuthenticator();
        }

        public void Request(IWebAction action,
            Uri uri, byte[] content)
        {
            var authenticator = _auth;
            var request = WebRequest.CreateHttp(uri);

            var method = action.Method;
            request.Method = method;

            var token = authenticator
                .GetToken(uri, method);

            if (token != null)
                request.Headers["Authorization"] = token;

            action.Initialize(request);

            Action getResponse = () => request.BeginGetResponse(
                HandleResponse(request, uri,
                    action, content, authenticator),
                null);

            if (content == null)
            {
                getResponse();
                return;
            }

            request.ContentType = "text/xml";
            request.BeginGetRequestStream(x =>
            {
                using (var buffer = request.EndGetRequestStream(x))
                    buffer.Write(content, 0, content.Length);

                getResponse();
            }, content);
        }

        private AsyncCallback HandleResponse(
            WebRequest request, Uri uri,
            IWebAction action, byte[] content,
            IAuthenticator authenticator)
        {
            return ar =>
            {
                try
                {
                    using (var response = (HttpWebResponse)
                        request.EndGetResponse(ar))
                    {
                        action.Complete(response);
                    }
                }
                catch (WebException ex)
                {
                    if (authenticator != _auth)
                    {
                        // Another thread has already updated the authenticator
                        Request(action, uri, content);
                        return;
                    }

                    // Do we have any alternative?
                    var next = _auth.Next(ex);
                    if (next == null)
                        action.Error(ex);

                    _auth = next;
                    Request(action, uri, content);
                }
            };
        }
    }
}