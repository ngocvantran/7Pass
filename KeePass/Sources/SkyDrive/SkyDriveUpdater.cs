using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
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
            {
                throw new NotImplementedException();
            });
        }

        public override void Upload(ListItem item, Action<ListItem> uploaded)
        {
            throw new NotImplementedException();
        }
    }
}
