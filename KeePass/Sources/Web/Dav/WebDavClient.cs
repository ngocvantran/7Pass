using System;
using System.Net;

namespace KeePass.Sources.Web.Dav
{
    internal class WebDavClient
    {
        private readonly WebDavRequest _client;
        private readonly string _server;

        public WebDavClient(string server,
            string user, string password)
        {
            if (server == null)
                throw new ArgumentNullException("server");

            _server = server;
            _client = new WebDavRequest(user, password);
        }

        public void List(string path)
        {
            var action = new ListAction();
            var uri = new UriBuilder(_server)
            {
                Path = path
            };

            _client.Request(action,
                uri.Uri, null);
        }

        private class ListAction : IWebAction
        {
            public string Method
            {
                get { return "PROPFIND"; }
            }

            public void Complete(HttpWebResponse response)
            {
                throw new NotImplementedException();
            }

            public void Error(WebException ex)
            {
                throw new NotImplementedException();
            }

            public void Initialize(HttpWebRequest request)
            {
                request.Headers["Depth"] = "1";
            }
        }
    }
}