using System;
using System.IO;
using System.IO.IsolatedStorage;
using KeePass.IO;
using KeePass.IO.Utils;

namespace KeePass.Data
{
    internal static class AppSettingsService
    {
        public static void Clear()
        {
            KeyCache.Database = null;
        }

        public static void ClearPassword()
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                if (store.FileExists(Consts.DECRYPTED))
                    store.DeleteFile(Consts.DECRYPTED);
            }
        }

        public static bool HasDatabase()
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                return store.FileExists(Consts.DATABASE);
            }
        }

        public static bool HasPassword()
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                return store.FileExists(Consts.DECRYPTED);
            }
        }

        public static void LoadSettings()
        {
            if (!HasPassword())
                return;

            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                using (var stream = store.OpenFile(
                    Consts.DECRYPTED, FileMode.Open, FileAccess.Read))
                {
                    using (var buffer = new MemoryStream())
                    {
                        BufferEx.CopyStream(stream, buffer);
                        var xml = buffer.ToArray();

                        KeyCache.Database =
                            DatabaseReader.Load(xml);
                    }
                }
            }
        }

        public static bool Open(string password,
            bool savePassword)
        {
            Clear();

            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                if (!store.FileExists(Consts.DATABASE))
                    return false;

                try
                {
                    using (var fs = store.OpenFile(
                        Consts.DATABASE, FileMode.Open))
                    {
                        var xml = DatabaseReader.GetXml(fs, password);

                        if (savePassword)
                            Save(store, xml);

                        KeyCache.Database = DatabaseReader.Load(xml);
                        return true;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        private static void Save(IsolatedStorageFile store, byte[] xml)
        {
            using (var fs = store.CreateFile(Consts.DECRYPTED))
            {
                fs.Write(xml, 0, xml.Length);
                fs.Flush();
            }
        }
    }
}