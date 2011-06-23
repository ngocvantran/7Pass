using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Linq;
using KeePass.IO.Utils;

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

        public void DownloadAsync(string path,
            Action<byte[]> complete,
            Action<WebException> error)
        {
            _client.Request(
                new DownloadAction(complete, error),
                GetPath(path), null);
        }

        public void ListAsync(string path,
            Action<IList<ItemInfo>> complete,
            Action<WebException> error)
        {
            _client.Request(
                new ListAction(complete, error),
                GetPath(path), null);
        }

        public void UploadAsync(string path, byte[] content,
            Action complete, Action<WebException> error)
        {
            _client.Request(
                new UploadAction(complete, error),
                GetPath(path), content);
        }

        private Uri GetPath(string path)
        {
            return new UriBuilder(_server)
            {
                Path = path
            }.Uri;
        }

        private class DownloadAction : IWebAction
        {
            private readonly Action<byte[]> _complete;
            private readonly Action<WebException> _error;

            public string Method
            {
                get { return "GET"; }
            }

            public DownloadAction(Action<byte[]> complete,
                Action<WebException> error)
            {
                if (complete == null) throw new ArgumentNullException("complete");
                if (error == null) throw new ArgumentNullException("error");

                _error = error;
                _complete = complete;
            }

            public void Complete(HttpStatusCode status, Stream response)
            {
                using (var buffer = new MemoryStream())
                {
                    BufferEx.CopyStream(response, buffer);
                    _complete(buffer.ToArray());
                }
            }

            public void Error(WebException ex)
            {
                _error(ex);
            }

            public void Initialize(HttpWebRequest request) {}
        }

        private class ListAction : IWebAction
        {
            private readonly Action<IList<ItemInfo>> _complete;
            private readonly Action<WebException> _error;

            public string Method
            {
                get { return "PROPFIND"; }
            }

            public ListAction(Action<IList<ItemInfo>> complete,
                Action<WebException> error)
            {
                if (complete == null) throw new ArgumentNullException("complete");
                if (error == null) throw new ArgumentNullException("error");
                _error = error;
                _complete = complete;
            }

            public void Complete(HttpStatusCode status, Stream response)
            {
                var d = (XNamespace)"DAV:";
                var items = new List<ItemInfo>();
                var doc = XDocument.Load(response);

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

                _complete(items);
            }

            public void Error(WebException ex)
            {
                _error(ex);
            }

            public void Initialize(HttpWebRequest request)
            {
                request.Headers["Depth"] = "1";
            }
        }

        private class UploadAction : IWebAction
        {
            private readonly Action _complete;
            private readonly Action<WebException> _error;

            public string Method
            {
                get { return "PUT"; }
            }

            public UploadAction(Action complete, Action<WebException> error)
            {
                if (complete == null) throw new ArgumentNullException("complete");
                if (error == null) throw new ArgumentNullException("error");

                _error = error;
                _complete = complete;
            }

            public void Complete(HttpStatusCode status, Stream response)
            {
                _complete();
            }

            public void Error(WebException ex)
            {
                _error(ex);
            }

            public void Initialize(HttpWebRequest request) {}
        }
    }
}