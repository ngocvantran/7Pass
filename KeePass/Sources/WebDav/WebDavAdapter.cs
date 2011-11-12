using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KeePass.IO.Utils;
using KeePass.Sources.WebDav.Api;
using KeePass.Storage;

namespace KeePass.Sources.WebDav
{
    internal class WebDavAdapter : ServiceAdapterBase
    {
        private WebDavClient _client;
        private SyncInfo _info;

        public override void Conflict(ListItem item,
            Action<ListItem, string> uploaded)
        {
            var path = GetNonConflictPath();

            UploadFileAsync(path, x =>
                uploaded(x, _client.GetUrl(path)));
        }

        public override void Download(ListItem item,
            Action<ListItem, byte[]> downloaded)
        {
            _client.DownloadAsync(_info.Path,
                x => downloaded(item, x), OnError);
        }

        public override SyncInfo Initialize(DatabaseInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            var details = info.Details;
            var parts = details.Url.Split('\n');

            _client = new WebDavClient(
                parts[1], parts[2]);

            _info = new SyncInfo
            {
                Path = parts[0],
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
            _client.ListAsync(_info.Path,
                x => ready(Translate(x)),
                OnError, OnError);
        }

        public override void Upload(ListItem item,
            Action<ListItem> uploaded)
        {
            UploadFileAsync(_info.Path, uploaded);
        }

        private string GetNonConflictPath()
        {
            var path = _info.Path;
            var local = new Uri(path).LocalPath;
            var dir = Path.GetDirectoryName(local);
            var extension = Path.GetExtension(local);
            var fileName = Path.GetFileNameWithoutExtension(local);

            fileName = string.Concat(fileName,
                " (7Pass' conflicted copy ",
                DateTime.Today.ToString("yyyy-MM-dd"),
                ")", extension);

            var newPath = Path.Combine(dir, fileName)
                .Replace('\\', '/');

            return path.Replace(local, newPath);
        }

        private static ListItem Translate(
            IEnumerable<ItemInfo> items)
        {
            var item = items
                .FirstOrDefault();

            var info = new ListItem
            {
                Tag = item,
            };

            if (item != null)
                info.Timestamp = item.Modified;

            return info;
        }

        private void UploadFileAsync(string path,
            Action<ListItem> uploaded)
        {
            _client.UploadAsync(path, _info.Database,
                () => _client.ListAsync(path,
                    y => uploaded(Translate(y)),
                    OnError, OnError),
                OnError);
        }
    }
}