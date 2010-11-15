using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace KeePass
{
    public static class AppSettingsService
    {
        private const string KEY = "Settings.bin";

        public static void Activated()
        {
            LoadSettings();
        }

        public static void Closing()
        {
            SaveSettings();
        }

        public static void Deactivated()
        {
            SaveSettings();
        }

        public static void Launching()
        {
            LoadSettings();
        }

        private static void LoadSettings()
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                if (!store.FileExists(KEY))
                    return;

                using (var stream = store.OpenFile(
                    KEY, FileMode.Open, FileAccess.Read))
                {
                    var reader = new BinaryReader(stream);

                    KeyCache.StorePassword = reader.ReadBoolean();
                    KeyCache.Password = reader.ReadString();
                }
            }
        }

        private static void SaveSettings()
        {
            using (var store = IsolatedStorageFile
                .GetUserStoreForApplication())
            {
                using (var stream = store.CreateFile(KEY))
                {
                    var writer = new BinaryWriter(stream);

                    writer.Write(KeyCache.StorePassword);

                    if (KeyCache.StorePassword)
                        writer.Write(KeyCache.Password ?? string.Empty);
                    else
                        writer.Write(string.Empty);

                    writer.Flush();
                }
            }
        }
    }
}