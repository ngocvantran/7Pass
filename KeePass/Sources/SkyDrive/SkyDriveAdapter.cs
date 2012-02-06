using System;
using System.IO;
using KeePass.IO.Utils;
using KeePass.Storage;

namespace KeePass.Sources.SkyDrive
{
    internal class SkyDriveAdapter : ServiceAdapterBase
    {
        private SkyDriveClient _client;
        private SyncInfo _info;

        public override void Conflict(ListItem item,
            Action<ListItem, string> uploaded)
        {
            var meta = (MetaListItemInfo)item.Tag;
            var name = GetNonConflictName(meta.Title);

            _client.Upload(meta.Parent, name,
                _info.Database, x => uploaded(item, x));
        }

        private static string GetNonConflictName(string name)
        {
            var extension = Path.GetExtension(name);
            var fileName = Path.GetFileNameWithoutExtension(name);

            return string.Concat(fileName,
                " (7Pass' conflicted copy ",
                DateTime.Today.ToString("yyyy-MM-dd"),
                ")", extension);
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
            _client.RefreshToken(() =>
                _client.GetFileMeta(_info.Path, x =>
                    ready(Translate(x))));
        }

        public override void Upload(ListItem item,
            Action<ListItem> uploaded)
        {
            var meta = (MetaListItemInfo)item.Tag;
            _client.Upload(meta.Parent, meta.Title,
                _info.Database, x =>
                {
                    _info.Path = x;
                    uploaded(item);
                });
        }

        private static ListItem Translate(
            MetaListItemInfo item)
        {
            var info = new ListItem
            {
                Tag = item,
            };

            if (item != null)
                info.Timestamp = item.Modified;

            return info;
        }
    }
}