using System;
using KeePass.Sources.DropBox;
using KeePass.Sources.Web;
using KeePass.Storage;

namespace KeePass.Sources
{
    internal static class DatabaseUpdater
    {
        public static void Update(DatabaseInfo info,
            Func<DatabaseInfo, bool> queryUpdate,
            ReportUpdateResult report)
        {
            var details = info.Details;
            switch (details.Source)
            {
                case DropBoxUpdater.NAME:
                    DropBoxUpdater.Update(info,
                        queryUpdate, report);
                    break;

                case WebUpdater.NAME:
                    WebUpdater.Update(info,
                        queryUpdate, report);
                    break;
            }
        }
    }
}