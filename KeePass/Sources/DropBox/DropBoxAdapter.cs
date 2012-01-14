using System;
using System.IO;
using DropNet;
using DropNet.Models;
using KeePass.IO.Utils;
using KeePass.Storage;

namespace KeePass.Sources.DropBox
{
    internal class DropBoxAdapter : ServiceAdapterBase
    {
        private DropNetClient _client;
        private SyncInfo _info;

        public override void Conflict(ListItem item,
            Action<ListItem, string> uploaded)
        {
            var path = GetNonConflictPath();

            UploadFileAsync(path, x => uploaded(
                Translate(x), _client.GetUrl(path)));
        }

        public override void Download(ListItem item,
            Action<ListItem, byte[]> downloaded)
        {
            _client.GetFileAsync(_info.Path,
                x => downloaded(item, x.RawBytes),
                OnError);
        }

        public override SyncInfo Initialize(DatabaseInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            var details = info.Details;
            var url = new Uri(details.Url);
            _client = CreateClient(url.UserInfo);

            _info = new SyncInfo
            {
                Path = url.LocalPath,
                Modified = details.Modified,
                HasLocalChanges = details.HasLocalChanges,
            };

            info.OpenDatabaseFile(x =>
            {
                using (var buffer = new MemoryStream())
                {
                    BufferEx.CopyStream(x, buffer);
                    _info.Database = buffer.ToArray();
                }
            });

            return _info;
        }

        public override void List(Action<ListItem> ready)
        {
            _client.GetMetaDataAsync(_info.Path,
                meta => ready(Translate(meta)),
                OnError);
        }

        public override void Upload(ListItem item,
            Action<ListItem> uploaded)
        {
            UploadFileAsync(_info.Path,
                meta => uploaded(Translate(meta)));
        }

        private static DropNetClient CreateClient(string userInfo)
        {
            var parts = userInfo.Split(':');
            
            return DropBoxUtils.Create(
                parts[0], parts[1]);
        }

        private string GetNonConflictPath()
        {
            var path = _info.Path;
            var dir = Path.GetDirectoryName(path);
            var extension = Path.GetExtension(path);
            var fileName = Path.GetFileNameWithoutExtension(path);

            fileName = string.Concat(fileName,
                " (7Pass' conflicted copy ",
                DateTime.Today.ToString("yyyy-MM-dd"),
                ")", extension);

            return Path.Combine(dir, fileName)
                .Replace('\\', '/');
        }

        private static ListItem Translate(MetaData meta)
        {
            return new ListItem
            {
                Tag = meta,
                Timestamp = meta.Modified,
            };
        }

        private void UploadFileAsync(string path,
            Action<MetaData> completed)
        {
            var orgPath = path;

            var fileName = Path.GetFileName(path);
            path = Path.GetDirectoryName(path)
                .Replace('\\', '/');

            _client.UploadFileAsync(path,
                fileName, _info.Database,
                x => _client.GetMetaDataAsync(
                    orgPath, completed, OnError),
                OnError);
        }
    }
}