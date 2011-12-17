using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KeePass.IO.Utils;
using KeePass.Storage;

namespace KeePass.Sources.SkyDrive
{
    internal class SkyDriveAdapter : ServiceAdapterBase
    {
        private SyncInfo _info;
        private SkyDriveClient _client;

        public override void Conflict(ListItem item,
            Action<ListItem, string> uploaded)
        {
            throw new NotImplementedException();
        }

        public override void Download(ListItem item,
            Action<ListItem, byte[]> downloaded)
        {
            _client.Download(_info.Path, (_, __, bytes) =>
                downloaded(item, bytes));
        }

        public override SyncInfo Initialize(DatabaseInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            var details = info.Details;

            string id;
            _client = SkyDriveClient
                .ParsePath(details.Url, out id);

            _info = new SyncInfo
            {
                Path = id,
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
            _client.List(_info.Path, (parent, items) =>
                ready(Translate(items)));
        }

        private static ListItem Translate(
            IEnumerable<MetaListItemInfo> items)
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

        public override void Upload(ListItem item,
            Action<ListItem> uploaded)
        {
            _client.Upload(_info.Path, _info.Database);
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
    }
}
