using System;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using RestSharp;

namespace KeePass.Sources.SkyDrive
{
    internal class SkyDriveClient
    {
        private readonly string _token;
        private readonly RestClient _client;

        public SkyDriveClient(string token)
        {
            if (token == null)
                throw new ArgumentNullException("token");

            _token = token;
            _client = new RestClient(
                "https://beta.apis.live.net/v5.0/");
            _client.AddDefaultParameter("token",
                token, ParameterType.UrlSegment);
        }

        public void Download(string path,
            Action<MetaListItemInfo, string, byte[]> complete)
        {
            byte[] bytes = null;
            MetaListItemInfo item = null;

            var waitInfo = new ManualResetEvent(false);
            var waitContent = new ManualResetEvent(false);

            _client.ExecuteAsync(
                Request(path + "/content"), x =>
                {
                    bytes = x.RawBytes;
                    waitContent.Set();
                });

            _client.ExecuteAsync(
                Request(path), x =>
                {
                    try
                    {
                        var root = JsonConvert
                            .DeserializeXNode(x.Content, "root")
                            .Root;

                        if (root == null)
                            return;

                        item = new MetaListItemInfo(root);
                    }
                    finally
                    {
                        waitInfo.Set();
                    }
                });

            ThreadPool.QueueUserWorkItem(_ =>
            {
                waitInfo.WaitOne();
                waitContent.WaitOne();

                var id = string.Concat(item.Path,
                    Environment.NewLine, _token);

                complete(item, id, bytes);
            });
        }

        public void GetEmail(Action<string> complete)
        {
            _client.ExecuteAsync(Request("me"), x =>
            {
                var doc = JsonConvert.DeserializeXNode(
                    x.Content, "root");

                var email = doc.Root.GetValue(
                    "emails", "preferred");

                complete(email);
            });
        }

        public void List(string path, Action<MetaListItemInfo,
            MetaListItemInfo[]> complete)
        {
            if (string.IsNullOrEmpty(path))
                path = "me/skydrive";

            MetaListItemInfo parent = null;
            var items = new MetaListItemInfo[0];

            var waitItems = new ManualResetEvent(false);
            var waitParent = new ManualResetEvent(false);

            _client.ExecuteAsync(Request(path + "/files"),
                x =>
                {
                    try
                    {
                        var root = JsonConvert
                            .DeserializeXNode(x.Content, "root")
                            .Root;

                        if (root == null)
                            return;

                        items = root
                            .Elements("data")
                            .Select(e => new MetaListItemInfo(e))
                            .ToArray();
                    }
                    finally
                    {
                        waitItems.Set();
                    }
                });

            _client.ExecuteAsync(Request(path), x =>
            {
                try
                {
                    var root = JsonConvert
                        .DeserializeXNode(x.Content, "root")
                        .Root;

                    if (root == null)
                        return;

                    parent = new MetaListItemInfo(root);
                }
                finally
                {
                    waitParent.Set();
                }
            });

            ThreadPool.QueueUserWorkItem(_ =>
            {
                waitItems.WaitOne();
                waitParent.WaitOne();
                complete(parent, items);
            });
        }

        private static RestRequest Request(string resource)
        {
            return new RestRequest(resource +
                "?access_token={token}")
            {
                RequestFormat = DataFormat.Json,
            };
        }
    }
}