using System;
using System.IO.IsolatedStorage;

namespace KeePass.Utils
{
    internal static class IsolatedStorageFileExtensions
    {
        public static void DeleteDirectory(
            this IsolatedStorageFile store,
            string path, bool recursive)
        {
            if (recursive)
            {
                var basePath = path + "/";
                
                var folders = store.GetDirectoryNames(basePath);
                foreach (var folder in folders)
                {
                    DeleteDirectory(store,
                        basePath + folder, true);
                }

                var files = store.GetFileNames(basePath);
                foreach (var file in files)
                    store.DeleteFile(basePath + file);
            }

            store.DeleteDirectory(path);
        }
    }
}