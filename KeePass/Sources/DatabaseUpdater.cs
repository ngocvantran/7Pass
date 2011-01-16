using System;
using KeePass.Sources.DropBox;
using KeePass.Sources.Web;
using KeePass.Storage;

namespace KeePass.Sources
{
    internal static class DatabaseUpdater
    {
        public const string DROPBOX_UPDATER = "DropBox";
        public const string WEB_UPDATER = "Web";

        public static void Update(DatabaseInfo info,
            Func<DatabaseInfo, bool> queryUpdate,
            ReportUpdateResult report)
        {
            var details = info.Details;
            switch (details.Source)
            {
                case DROPBOX_UPDATER:
                    DropBoxUpdater.Update(info,
                        queryUpdate, report);
                    break;

                case WEB_UPDATER:
                    WebUpdater.Update(info,
                        queryUpdate, report);
                    break;
            }
        }
    }
}