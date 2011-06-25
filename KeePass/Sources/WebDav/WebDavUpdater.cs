using System;
using System.IO;
using System.Linq;
using System.Net;
using KeePass.IO.Utils;
using KeePass.Sources.WebDav.Api;
using KeePass.Storage;

namespace KeePass.Sources.WebDav
{
    internal static class WebDavUpdater
    {
        public static string GetUrl(this WebDavClient client,
            string path)
        {
            return string.Join("\n", new[]
            {
                path,
                client.User,
                client.Password
            });
        }

        public static void Update(DatabaseInfo info,
            Func<DatabaseInfo, bool> queryUpdate,
            Action<SyncCompleteInfo> report)
        {
            if (info == null) throw new ArgumentNullException("info");
            if (queryUpdate == null) throw new ArgumentNullException("queryUpdate");
            if (report == null) throw new ArgumentNullException("report");

            var details = info.Details;
            var parts = details.Url.Split('\n');

            var client = new WebDavClient(
                parts[1], parts[2]);

            var syncInfo = new SyncInfo
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
                    syncInfo.Database = buffer.ToArray();
                }
            });

            Synchronize(client, syncInfo, x =>
            {
                if (queryUpdate(info))
                    report(x);
            });
        }

        private static string GetNonConflictPath(string path)
        {
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

        private static void OnFileMetaReady(
            WebDavClient client,
            SyncInfo info, ItemInfo meta,
            Action<SyncCompleteInfo> report)
        {
            // File deleted from server
            if (meta == null)
            {
                // Has local change, upload to server
                client.UploadFileAsync(info.Path,
                    info.Database,
                    x => report(new SyncCompleteInfo
                    {
                        Path = info.Path,
                        Modified = x.Modified,
                        Result = SyncResults.Uploaded,
                    }),
                    x => report(new SyncCompleteInfo
                    {
                        Path = info.Path,
                        Result = SyncResults.Failed,
                    }));

                return;
            }

            // No change from server side
            if (meta.Modified == info.Modified)
            {
                if (!info.HasLocalChanges)
                {
                    report(new SyncCompleteInfo
                    {
                        Path = info.Path,
                        Result = SyncResults.NoChange,
                    });

                    return;
                }

                // Has local change, upload to server
                client.UploadFileAsync(info.Path,
                    info.Database,
                    x => report(new SyncCompleteInfo
                    {
                        Path = info.Path,
                        Modified = x.Modified,
                        Result = SyncResults.Uploaded,
                    }),
                    x => report(new SyncCompleteInfo
                    {
                        Path = info.Path,
                        Result = SyncResults.Failed,
                    }));

                return;
            }

            // Has changes from server
            if (!info.HasLocalChanges)
            {
                // Database should be updated
                client.DownloadAsync(info.Path,
                    x => report(new SyncCompleteInfo
                    {
                        Database = x,
                        Path = info.Path,
                        Modified = meta.Modified,
                        Result = SyncResults.Downloaded,
                    }),
                    ex => report(new SyncCompleteInfo
                    {
                        Path = info.Path,
                        Result = SyncResults.Failed,
                    }));

                return;
            }

            // Conflict
            var path = GetNonConflictPath(info.Path);

            client.UploadFileAsync(path, info.Database,
                x => report(new SyncCompleteInfo
                {
                    Modified = x.Modified,
                    Path = GetUrl(client, path),
                    Result = SyncResults.Conflict,
                }),
                x => report(new SyncCompleteInfo
                {
                    Path = info.Path,
                    Result = SyncResults.Failed,
                }));
        }

        private static void Synchronize(WebDavClient client,
            SyncInfo info, Action<SyncCompleteInfo> report)
        {
            client.ListAsync(info.Path, items =>
                OnFileMetaReady(client, info,
                    items.FirstOrDefault(), report),
                ex => report(new SyncCompleteInfo
                {
                    Path = info.Path,
                    Result = SyncResults.Failed,
                }));
        }

        private static void UploadFileAsync(
            this WebDavClient client,
            string path, byte[] fileData,
            Action<ItemInfo> success,
            Action<WebException> failure)
        {
            client.UploadAsync(
                path, fileData, () => client.ListAsync(path,
                    y => success(y.FirstOrDefault()), failure),
                failure);
        }
    }
}