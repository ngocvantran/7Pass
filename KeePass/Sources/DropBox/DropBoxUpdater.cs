using System;
using KeePass.Sources.DropBox.Api;
using KeePass.Storage;

namespace KeePass.Sources.DropBox
{
    internal static class DropBoxUpdater
    {
        public static void Update(DatabaseInfo info,
            Func<DatabaseInfo, bool> queryUpdate,
            ReportUpdateResult report)
        {
            if (info == null) throw new ArgumentNullException("info");
            if (queryUpdate == null) throw new ArgumentNullException("queryUpdate");
            if (report == null) throw new ArgumentNullException("report");

            var details = info.Details;
            var url = new Uri(details.Url);

            var client = CreateClient(url.UserInfo);
            client.DownloadAsync(url.LocalPath, x =>
            {
                if (!queryUpdate(info))
                    return;

                if (x == null)
                {
                    report(info, false,
                        DropBoxResources.DownloadError);

                    return;
                }

                info.SetDatabase(x, details);
                report(info, true, null);
            });
        }

        private static Client CreateClient(string userInfo)
        {
            var parts = userInfo.Split(':');
            return new Client(parts[0], parts[1]);
        }
    }
}