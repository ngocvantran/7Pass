using System;
using System.IO;
using System.Net;
using Hammock;
using Hammock.Authentication.OAuth;
using Hammock.Web;
using KeePass.Sources.DropBox.Api.Serialize;

namespace KeePass.Sources.DropBox.Api
{
    internal class Client
    {
        private readonly RestClient _client;
        private readonly RestClient _fileClient;
        private LoginInfo _loginInfo;

        public bool IsAuthenticated
        {
            get { return _client.Credentials != null; }
        }

        public Client(string userToken, string userSecret)
            : this()
        {
            InitAuthenticator(userToken, userSecret);
        }

        public Client()
        {
            var serializer = new JsonSerializer();

            _client = new RestClient
            {
                VersionPath = "0",
                UserAgent = "7Pass",
                Deserializer = serializer,
                Authority = "https://api.dropbox.com/",
            };

            _fileClient = new RestClient
            {
                VersionPath = "0",
                UserAgent = "7Pass",
                Deserializer = serializer,
                Authority = "https://api-content.dropbox.com/",
            };
        }

        public void DownloadAsync(string path, Action<Stream> report)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException(
                    "User token is missing. Login first.");
            }

            if (!path.StartsWith("/"))
                path = "/" + path;

            path = "files/dropbox" + path;

            var request = new RestRequest
            {
                Path = path,
                Method = WebMethod.Get,
            };

            _fileClient.BeginRequest(request, (req, res, state) =>
                report(res.StatusCode == HttpStatusCode.OK
                    ? res.ContentStream : null));
        }

        public LoginInfo GetLoginInfo()
        {
            return _loginInfo;
        }

        public void ListAsync(string path, Action<MetaData> report)
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException(
                    "User token is missing. Login first.");
            }

            if (!path.StartsWith("/"))
                path = "/" + path;

            path = "metadata/dropbox" + path;

            var request = new RestRequest
            {
                Path = path,
                Method = WebMethod.Post,
            };

            _client.BeginRequest<MetaData>(request, (req, res, state) =>
                report(res.StatusCode == HttpStatusCode.OK
                    ? res.ContentEntity : null));
        }

        public void LoginAsync(string email,
            string password, Action<LoginInfo> report)
        {
            var request = new RestRequest
            {
                Path = "token",
                Method = WebMethod.Post,
            };
            request.AddField("email", email);
            request.AddField("password", password);
            request.AddField("oauth_consumer_key", DropBoxInfo.KEY);

            ClearAuthenticator();
            _client.BeginRequest<LoginInfo>(request, (req, res, state) =>
            {
                if (res.StatusCode != HttpStatusCode.OK)
                {
                    report(null);
                    return;
                }

                var data = res.ContentEntity;
                InitAuthenticator(data.Token, data.Secret);
                report(data);
            });
        }

        private void ClearAuthenticator()
        {
            _loginInfo = null;
            _client.Credentials = null;
            _fileClient.Credentials = null;
        }

        private void InitAuthenticator(string token, string secret)
        {
            _loginInfo = new LoginInfo
            {
                Token = token,
                Secret = secret,
            };

            var credentials = OAuthCredentials.ForAccessToken(
                DropBoxInfo.KEY, DropBoxInfo.SECRET, token, secret);

            _client.Credentials = credentials;
            _fileClient.Credentials = credentials;
        }
    }
}