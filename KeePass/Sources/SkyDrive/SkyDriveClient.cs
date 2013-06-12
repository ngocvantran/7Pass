using System;
using System.Linq;
using System.Net;
using System.Threading;
using KeePass.Utils;
using Newtonsoft.Json;
using RestSharp;

namespace KeePass.Sources.SkyDrive
{
    internal class SkyDriveClient
    {
        private readonly RestClient _client;
        private AccessTokenData _token;

        public SkyDriveClient(string tokenData)
            : this(Parse(tokenData)) {}

        private SkyDriveClient(AccessTokenData token)
        {
            if (token == null)
                throw new ArgumentNullException("token");

            _client = new RestClient
            {
                UserAgent = "7Pass",
                BaseUrl = "https://apis.live.net/v5.0/",
            };

            SetToken(token);
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

                var id = GetSyncPath(item.Path);
                complete(item, id, bytes);
            });
        }

        public void GetFileMeta(string path,
            Action<MetaListItemInfo> complete)
        {
            _client.ExecuteAsync(Request(path), x =>
            {
                var root = JsonConvert
                    .DeserializeXNode(x.Content, "root")
                    .Root;

                if (root == null)
                {
                    complete(null);
                    return;
                }

                complete(new MetaListItemInfo(root));
            });
        }

        public static void GetToken(
            string code, Action<string> complete)
        {
            var data = string.Format(
                Resources.AuthTokenData,
                ApiKeys.SKYDRIVE_CLIENT_ID,
                ApiKeys.SKYDRIVE_REDIRECT,
                ApiKeys.SKYDRIVE_SECRET, code);

            var client = new WebClient();
            client.Headers[HttpRequestHeader.ContentType] =
                "application/x-www-form-urlencoded";
            
            client.UploadStringCompleted +=
                (sender, args) => complete(args.Result);
            client.UploadStringAsync(
                new Uri(Resources.AuthTokenUrl),
                "POST", data);
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

        public static SkyDriveClient ParsePath(
            string url, out string id)
        {
            var data = JsonConvert
                .DeserializeObject<PathData>(url);

            id = data.ID;
            return new SkyDriveClient(data.Token);
        }

        public void RefreshToken(Action completed)
        {
            var data = string.Format(
                Resources.TokenRefreshData,
                ApiKeys.SKYDRIVE_CLIENT_ID,
                ApiKeys.SKYDRIVE_SECRET,
                ApiKeys.SKYDRIVE_REDIRECT,
                _token.refresh_token);

            var client = new WebClient();
            client.Headers[HttpRequestHeader.ContentType] =
                "application/x-www-form-urlencoded";

            client.UploadStringCompleted += (sender, args) =>
            {
                SetToken(Parse(args.Result));
                completed();
            };
            client.UploadStringAsync(
                new Uri(Resources.TokenRefreshUrl),
                "POST", data);
        }

        public void Rename(string path, string name,
            Action<string> completed)
        {
            var request = new RestRequest(path)
            {
                Method = Method.PUT,
                RequestFormat = DataFormat.Json,
            };

            request.AddHeader("Authorization",
                "Bearer " + _token.access_token);

            request.JsonSerializer = new
                StupidAssemblyRedirectWorkAround(name);
            request.AddBody(new object());

            _client.ExecuteAsync(request, x =>
            {
                var root = JsonConvert
                    .DeserializeXNode(x.Content, "root")
                    .Root;

                if (root != null)
                    completed(root.GetValue("id"));
            });
        }

        public void Upload(string folder,
            string name, byte[] content,
            Action<string, string> completed)
        {
            var request = Request("{folder}/files/");
            request.Method = Method.POST;
            request.AddUrlSegment("folder", folder);
            request.AddFile("file", content, name);

            _client.ExecuteAsync(request, x =>
            {
                var root = JsonConvert
                    .DeserializeXNode(x.Content, "root")
                    .Root;

                if (root == null)
                    return;

                var path = root.GetValue("id");
                var pathData = GetSyncPath(path);
                completed(path, pathData);
            });
        }

        private string GetSyncPath(string id)
        {
            return JsonConvert.SerializeObject(
                new PathData
                {
                    ID = id,
                    Token = _token
                });
        }

        private static AccessTokenData Parse(string tokenData)
        {
            return JsonConvert.DeserializeObject
                <AccessTokenData>(tokenData);
        }

        private static RestRequest Request(string resource)
        {
            return new RestRequest(resource +
                "?access_token={token}")
            {
                RequestFormat = DataFormat.Json,
            };
        }

        private void SetToken(AccessTokenData token)
        {
            _token = token;

            var tokenPar = _client.DefaultParameters
                .FirstOrDefault(x => x.Name == "token");

            if (tokenPar != null)
                tokenPar.Value = token.access_token;
            else
            {
                _client.AddDefaultParameter("token",
                    token.access_token,
                    ParameterType.UrlSegment);
            }
        }

        private class StupidAssemblyRedirectWorkAround : RestSharp.Serializers.ISerializer
        {
            private readonly string _json;
            public string ContentType { get; set; }
            public string DateFormat { get; set; }
            public string Namespace { get; set; }
            public string RootElement { get; set; }

            public StupidAssemblyRedirectWorkAround(string name)
            {
                if (name == null)
                    throw new ArgumentNullException("name");

                _json = string.Format(
                    "{{name: \"{0}\"}}", name);
                ContentType = "application/json";
            }

            public string Serialize(object obj)
            {
                return _json;
            }
        }
    }
}