using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using KeePass.IO.Utils;

namespace KeePass.Sources.WebDav.Api
{
    internal class WebDavClient
    {
        private readonly WebDavRequest _client;
        private readonly string _password;
        private readonly string _user;

        public string Password
        {
            get { return _password; }
        }

        public string User
        {
            get { return _user; }
        }

        public WebDavClient(string user, string password)
        {
            _user = user;
            _password = password;
            _client = new WebDavRequest(user, password);
        }

        public void DownloadAsync(string path,
            Action<byte[]> complete,
            Action<WebException> error)
        {
            _client.Request(
                new DownloadAction(complete, error),
                new Uri(path), null);
        }

        public string GetUrl(string path)
        {
            return string.Join("\n", new[]
            {
                path, _user, _password
            });
        }

        public void ListAsync(string path,
            Action<IList<ItemInfo>> complete,
            Action htmlDetected,
            Action<WebException> error)
        {
            _client.Request(
                new ListAction(complete, htmlDetected, error),
                new Uri(path), null);
        }

        public void UploadAsync(string path, byte[] content,
            Action complete, Action<WebException> error)
        {
            _client.Request(
                new UploadAction(complete, error),
                new Uri(path), content);
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
            private readonly Action _htmlDetected;

            public string Method
            {
                get { return "PROPFIND"; }
            }

            public ListAction(Action<IList<ItemInfo>> complete,
                Action htmlDetected, Action<WebException> error)
            {
                if (complete == null) throw new ArgumentNullException("complete");
                if (htmlDetected == null) throw new ArgumentNullException("htmlDetected");
                if (error == null) throw new ArgumentNullException("error");

                _error = error;
                _complete = complete;
                _htmlDetected = htmlDetected;
            }

            public void Complete(HttpStatusCode status, Stream response)
            {
                var d = (XNamespace)"DAV:";
                var items = new List<ItemInfo>();

                XElement root;

                try
                {
                    var doc = XDocument.Load(response);
                    root = doc.Root;

                    if (root == null)
                    {
                        _error(new WebException(
                            "Invalid response"));

                        return;
                    }
                }
                catch (NotSupportedException ex)
                {
                    _error(new WebException(
                        "Invalid response", ex));

                    return;
                }
                catch (XmlException ex)
                {
                    _error(new WebException(
                        "Invalid response", ex));

                    return;
                }

                if (string.Equals(root.Name.LocalName, "html",
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    _htmlDetected();
                    return;
                }

                try
                {
                    foreach (var item in root.Elements(d + "response"))
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
                }
                catch (NullReferenceException ex)
                {
                    _error(new WebException(
                        "Invalid response", ex));

                    return;
                }

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