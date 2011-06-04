using System;
using System.IO;
using DropNet;
using DropNet.Exceptions;
using DropNet.Models;
using KeePass.IO.Utils;
using KeePass.Storage;

namespace KeePass.Sources.DropBox
{
    internal static class DropBoxUpdater
    {
        public static string GetUrl(
            this DropNetClient client,
            string path)
        {
            var login = client.UserLogin;

            return string.Format(
                "dropbox://{0}:{1}@dropbox.com{2}",
                login.Token, login.Secret, path);
        }

        public static void Update(DatabaseInfo info,
            Func<DatabaseInfo, bool> queryUpdate,
            Action<SyncCompleteInfo> report)
        {
            if (info == null) throw new ArgumentNullException("info");
            if (queryUpdate == null) throw new ArgumentNullException("queryUpdate");
            if (report == null) throw new ArgumentNullException("report");

            var details = info.Details;
            var url = new Uri(details.Url);
            var client = CreateClient(url.UserInfo);

            var syncInfo = new SyncInfo
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
                    syncInfo.Database = buffer.ToArray();
                }
            });

            Synchronize(client, syncInfo, x =>
            {
                if (queryUpdate(info))
                    report(x);
            });
        }

        private static DropNetClient CreateClient(string userInfo)
        {
            var parts = userInfo.Split(':');

            return new DropNetClient(
                DropBoxInfo.KEY, DropBoxInfo.SECRET,
                parts[0], parts[1]);
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
            DropNetClient client,
            SyncInfo info, MetaData meta,
            Action<SyncCompleteInfo> report)
        {
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
                client.GetFileAsync(info.Path,
                    x => report(new SyncCompleteInfo
                    {
                        Path = info.Path,
                        Database = x.RawBytes,
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
                    Path = client.GetUrl(path),
                    Result = SyncResults.Conflict,
                }),
                x => report(new SyncCompleteInfo
                {
                    Path = info.Path,
                    Result = SyncResults.Failed,
                }));
        }

        private static void Synchronize(DropNetClient client,
            SyncInfo info, Action<SyncCompleteInfo> report)
        {
            client.GetMetaDataAsync(info.Path, meta =>
                OnFileMetaReady(client, info, meta, report),
                ex => report(new SyncCompleteInfo
                {
                    Path = info.Path,
                    Result = SyncResults.Failed,
                }));
        }

        private static void UploadFileAsync(
            this DropNetClient client,
            string path, byte[] fileData,
            Action<MetaData> success,
            Action<DropboxException> failure)
        {
            var orgPath = path;

            var fileName = Path.GetFileName(path);
            path = Path.GetDirectoryName(path)
                .Replace('\\', '/');

            client.UploadFileAsync(
                path, fileName, fileData,
                x => client.GetMetaDataAsync(
                    orgPath, success, failure),
                failure);
        }
    }
}