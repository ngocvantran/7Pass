using System;
using System.IO;
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
            switch (info.Details.Source)
            {
                case DROPBOX_UPDATER:
                    DropBoxUpdater.Update(info, queryUpdate,
                        x => Report(info, x, report));
                    break;

                case WEB_UPDATER:
                    WebUpdater.Update(info,
                        queryUpdate, report);
                    break;
            }
        }

        private static void Report(DatabaseInfo info,
            SyncCompleteInfo result, ReportUpdateResult report)
        {
            string msg = null;
            var details = info.Details;

            switch (result.Result)
            {
                case SyncResults.Downloaded:
                    using (var buffer = new MemoryStream(result.Database))
                        info.SetDatabase(buffer, details);
                    break;

                case SyncResults.Uploaded:
                    details.Modified = result.Modified;
                    info.SaveDetails();
                    break;

                case SyncResults.Conflict:
                    details.Url = result.Path;
                    details.Modified = result.Modified;
                    info.SaveDetails();
                    break;

                case SyncResults.Failed:
                    msg = Properties.Resources
                        .DownloadError;
                    break;
            }

            report(info, result.Result, msg);
        }
    }
}