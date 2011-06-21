using System;
using System.Collections.Generic;
using System.Net;
using System.Xml.Linq;

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

            public void Complete(string content, HttpStatusCode status)
            {
                var d = (XNamespace)"DAV:";
                var items = new List<ItemInfo>();
                var doc = XDocument.Parse(content);

                // ReSharper disable PossibleNullReferenceException
                foreach (var item in doc.Root.Elements(d + "response"))
                {
                    var path = item
                        .Element(d + "href")
                        .Value;
                    path = Uri.UnescapeDataString(path);

                    var properties = item
                        .Element(d + "propstat")
                        .Element(d + "prop");

                    var lastModified = properties
                        .Element(d + "getlastmodified")
                        .Value;

                    var isDir = properties
                        .Element(d + "resourcetype")
                        .Element(d + "collection") != null;
                

                    items.Add(new ItemInfo
                    {
                        Path = path,
                        IsDir = isDir,
                        Modified = lastModified,
                    });
                }
                // ReSharper restore PossibleNullReferenceException
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